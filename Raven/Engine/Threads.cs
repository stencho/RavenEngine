using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Raven.Engine;

public static class Threads {
    static int _task_count = 0; public static int TaskCount => _task_count;
    static int _max_tasks = 128; public static int MaxTasks => _max_tasks;
    
    public static void IncrementTaskCount() => Interlocked.Increment(ref _task_count);
    public static void DecrementTaskCount() => Interlocked.Decrement(ref _task_count);

    static ConcurrentDictionary<Guid, ThreadInfo> threads = new();
    static ConcurrentQueue<ThreadRequestPacket> ThreadRequestQueue = new ();

    private static TimeSpan dispatch_wait = new TimeSpan((long)1000);
    private static int prune_wait_ms = 100;
    
    public static string list_all_active_threads {
        get {
            string output = "";
            lock (threads) {
                var arr = threads.ToArray().OrderBy(a => a.Value.start_time.Ticks).ToArray();
                foreach (var kvp in arr) {
                    output += "  " + kvp.Value.formatted_info + (kvp.Value.Finished ? $" [Done in {kvp.Value.run_time_ticks} ticks ({kvp.Value.run_time_ms:0.000}ms)]" : "");
                    output += "\n";
                }
            }
            //prune();
            return output;
        }
        
    }

    internal static CancellationTokenSource cancellation_token_source = new CancellationTokenSource();
    internal static CancellationToken cancellation_token => cancellation_token_source.Token;

    public static bool IsCancellationRequested => cancellation_token_source.IsCancellationRequested;
    public static void Cancel() => cancellation_token_source.Cancel();
    
    class ThreadInfo {
        public string name;
        public string caller_filename;
        public string caller_member_name;

        bool _finished = false;
        public bool Finished =>  _finished;
        
        public string formatted_info => $"{(name.Length > 0 ? "" + name + " " : "")}[{new FileInfo(caller_filename).Name}::{caller_member_name}]";
        
        public ThreadRequestPacket? packet;

        public DateTime start_time;
        public DateTime end_time;

        public double run_time_ms => (end_time - start_time).TotalMilliseconds;
        public long run_time_ticks => (end_time - start_time).Ticks;
        
        public ThreadInfo(string name, string caller_filename, string caller_member_name) {
            this.name = name;
            this.caller_filename = caller_filename;
            this.caller_member_name = caller_member_name;
            
            start_time = DateTime.Now;
        }

        public void FinishTask() {
            end_time = DateTime.Now;
            _finished = true;
        }
    }
    
    public class ThreadRequestPacket {
        public string Name { get; set; } = "";
        public string CallerFilename { get; set; } = "";
        public string CallerMemberName { get; set; } = "";
        
        Action MainAction { get; set; }
        Action CallbackAction { get; set; }
        
        public void InvokeAll() {
            InvokeMainAction();
            InvokeCallbackAction();
        }

        public void InvokeMainAction() {          
            MainAction?.Invoke();
        }
        public void InvokeCallbackAction() {
            CallbackAction?.Invoke();
        }

        public ThreadRequestPacket() {}
        public ThreadRequestPacket(Action main_action) {
            MainAction = main_action;
        }
        public ThreadRequestPacket(Action main_action, Action callback_action) {
            MainAction = main_action;
            CallbackAction = callback_action;
        }
    }

    private const bool use_pruner = true;
    
    public static void Initialize() {
        StartTask($"Dispatcher", DispatcherThread, cancellation_token_source.Token);
        if (use_pruner)
            StartTask($"ThreadInfo Pruner", ThreadInfoPruner, cancellation_token_source.Token);
    }   
    
    public static void Request(ThreadRequestPacket request, [CallerFilePath] string caller_filename = "", [CallerMemberName] string member_name = "") {
        request.CallerFilename = caller_filename;
        request.CallerMemberName = member_name;
        ThreadRequestQueue.Enqueue(request);
    }
    
    private async static void DispatcherThread() {
        while (State.running) {
            
            //instead of looping repeatedly including the sleep, use a goto
            //to repeatedly jump back to the check until the queue is empty, then
            //go back to sleeping every loop to save time
            
            possibly_still_items_in_queue:
            if (ThreadRequestQueue.Count > 0) {
                //also do similar while waiting for room in the task queue
                waiting_for_task_slot:
                if (_task_count < _max_tasks) {
                    if (ThreadRequestQueue.TryDequeue(out var request)) {
                        StartTask(request);
                    } 
                } else goto waiting_for_task_slot;
                goto possibly_still_items_in_queue;
            }

            await Task.Delay(dispatch_wait);
        }
    }

    static void prune() {
        lock (threads) {
            foreach (var t in threads.Keys) {
                if (threads[t].Finished) {
                    threads.TryRemove(t, out _);
                }
            }
        } 
    }
    
    private async static void ThreadInfoPruner() {
        while (State.running) {
            prune();

            //Debug.WriteLine("Attempted Prune");
            Thread.Sleep(prune_wait_ms);
        }
    }
    
    public static Task StartTask(ThreadRequestPacket packet) {
        Guid task_guid = Guid.NewGuid();
        
        return Task.Run(() => {
            IncrementTaskCount();
            try {
                threads.TryAdd(task_guid, new ThreadInfo(packet.Name, packet.CallerFilename, packet.CallerMemberName) {packet = packet});
                packet.InvokeMainAction();
            } finally {
                packet.InvokeCallbackAction();
                threads[task_guid].FinishTask();
                DecrementTaskCount();
            }
        }, cancellation_token).ContinueWith(t => {
            if (t.IsFaulted) {
                throw new Exception($"Task failed: [{packet.CallerFilename}::{packet.CallerMemberName}] {t.Exception}");
            }
        }, TaskContinuationOptions.OnlyOnFaulted);
    }
    
    public static Task StartTask(string task_name, Action action, [CallerFilePath] string caller_filename = "", [CallerMemberName] string member_name = "") {
        Guid task_guid = Guid.NewGuid();
        
        return Task.Run(() => {
            IncrementTaskCount();
            try {
                threads.TryAdd(task_guid, new ThreadInfo(task_name, caller_filename, member_name));
                action.Invoke();
            } finally {
                threads[task_guid].FinishTask();
                DecrementTaskCount();
            }
        }, cancellation_token).ContinueWith(t => {
            if (t.IsFaulted) {
                throw new Exception($"Task failed: [{caller_filename}::{member_name}] {t.Exception}");
            }
        }, TaskContinuationOptions.OnlyOnFaulted);
    }

    public static Task StartTask(string task_name, Action action, CancellationToken cancellation_token, [CallerFilePath] string caller_filename = "", [CallerMemberName] string member_name = "") {            
        Guid task_guid = Guid.NewGuid();
        
        return Task.Run(() => {
            IncrementTaskCount();
            try {
                threads.TryAdd(task_guid, new ThreadInfo(task_name, caller_filename, member_name));
                action.Invoke();
            } finally {
                threads[task_guid].FinishTask();
                DecrementTaskCount();
            }
        }, cancellation_token).ContinueWith(t => {
            if (t.IsFaulted) {
                throw new Exception($"Task failed: [{caller_filename}::{member_name}] {t.Exception}");
            }
        }, TaskContinuationOptions.OnlyOnFaulted);
    }
    
    public static Task StartTask(string task_name, Action action, out Guid guid, [CallerFilePath] string caller_filename = "", [CallerMemberName] string member_name = "") {
        Guid task_guid = Guid.NewGuid();
        guid = task_guid;

        return Task.Run(() => {
            IncrementTaskCount();
            try {
                threads.TryAdd(task_guid, new ThreadInfo(task_name, caller_filename, member_name));
                action.Invoke();
            } finally {
                threads[task_guid].FinishTask();
                DecrementTaskCount();
            }
        }, cancellation_token).ContinueWith(t => {
            if (t.IsFaulted) {
                throw new Exception($"Task failed: [{caller_filename}::{member_name}] {t.Exception}");
            }
        }, TaskContinuationOptions.OnlyOnFaulted);
    }

    public static Task StartTask(string task_name, Action action, out Guid guid, CancellationToken cancellation_token, [CallerFilePath] string caller_filename = "", [CallerMemberName] string member_name = "") {
        Guid task_guid = Guid.NewGuid();
        guid = task_guid;

        return Task.Run(() => {
            IncrementTaskCount();
            try {
                threads.TryAdd(task_guid, new ThreadInfo(task_name, caller_filename, member_name));
                action.Invoke();
            } finally {
                threads[task_guid].FinishTask();
                DecrementTaskCount();
            }
        }, cancellation_token).ContinueWith(t => {
            if (t.IsFaulted) {
                throw new Exception($"Task failed: [{caller_filename}::{member_name}] {t.Exception}");
            }
        }, TaskContinuationOptions.OnlyOnFaulted);
    }
}