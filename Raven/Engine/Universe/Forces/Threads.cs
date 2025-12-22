using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Raven.Engine.Universes.Forces;

public static class Threads {
    static int _task_count = 0; public static int TaskCount => _task_count;
    static int _max_tasks = 32; public static int MaxTasks => _max_tasks;
    
    public static void IncrementTaskCount() => Interlocked.Increment(ref _task_count);
    public static void DecrementTaskCount() => Interlocked.Decrement(ref _task_count);

    static ConcurrentDictionary<Guid, ThreadInfo> threads = new();
    static ConcurrentQueue<ThreadRequestPacket> ThreadRequestQueue = new ();

    public static string list_all_active_threads {
        get {
            string output = "";
            lock (threads) {
                foreach (var keyValuePair in threads.OrderBy(a => a.Value.start_time)) {
                    output += "  " + keyValuePair.Value.formatted_info + (keyValuePair.Value.finished ? " [Done]" : "");
                    output += "\n";
                }
            }

            return output;
        }
    }

    internal static CancellationTokenSource cancellation_token_source = new CancellationTokenSource();
    internal static CancellationToken cancellation_token => cancellation_token_source.Token;

    public static bool IsCancellationRequested => cancellation_token_source.IsCancellationRequested;
    
    class ThreadInfo {
        public string name;
        public string caller_filename;
        public string caller_member_name;

        public bool finished = false;
        
        public string formatted_info => $"{(name.Length > 0 ? "" + name + " " : "")}[{new FileInfo(caller_filename).Name}::{caller_member_name}]";
        
        public ThreadRequestPacket? packet;

        public double start_time;
        
        public ThreadInfo(string name, string caller_filename, string caller_member_name) {
            this.name = name;
            this.caller_filename = caller_filename;
            this.caller_member_name = caller_member_name;
            if (Clock.game_time != null)
                start_time = Clock.game_time.TotalGameTime.TotalMilliseconds;
            else
                start_time = 0;
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
    
    public static void Initialize() {
        StartTask($"Dispatcher", DispatcherThread, cancellation_token_source.Token);
        StartTask($"ThreadInfoPruner", ThreadInfoPruner, cancellation_token_source.Token);
    }   
    
    public static void Request(ThreadRequestPacket request, [CallerFilePath] string caller_filename = "", [CallerMemberName] string member_name = "") {
        request.CallerFilename = caller_filename;
        request.CallerMemberName = member_name;
        ThreadRequestQueue.Enqueue(request);
    }

    private static TimeSpan dispatch_wait = new TimeSpan((long)1000);
    private static int prune_wait_ms = 1000;
    
    private async static void DispatcherThread() {
        while (State.running) {
            //instead of looping repeatedly including the sleep, use a goto
            //to repeatedly jump back to the check until the queue is empty, then
            //go back to sleeping every loop to save time
            
            possibly_still_items_in_queue:
            if (ThreadRequestQueue.Count > 0) {
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

    private async static void ThreadInfoPruner() {
        while (State.running) {
            foreach (var t in threads.Keys) {
                if (threads[t].finished) {
                    threads.TryRemove(t, out _);
                }
            }
            Debug.WriteLine("Attempted Prune");
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
                threads[task_guid].finished = true;
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
                threads[task_guid].finished = true;
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
                threads[task_guid].finished = true;
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
                threads[task_guid].finished = true;
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
                threads[task_guid].finished = true;
                DecrementTaskCount();
            }
        }, cancellation_token).ContinueWith(t => {
            if (t.IsFaulted) {
                throw new Exception($"Task failed: [{caller_filename}::{member_name}] {t.Exception}");
            }
        }, TaskContinuationOptions.OnlyOnFaulted);
    }
}