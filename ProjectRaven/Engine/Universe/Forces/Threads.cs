using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectRaven.Engine.Universes.Forces;

public static class Threads {
    static int _task_count = 0;
    static int _max_tasks = 32;
    
    public static int TaskCount => _task_count;
    public static int MaxTasks => _max_tasks;
    
    static ConcurrentQueue<ThreadRequestPacket> ThreadRequestQueue = new ();

    public static void Request(ThreadRequestPacket request) {
        ThreadRequestQueue.Enqueue(request);
    }
    
    public class ThreadRequestPacket {
        public Action ThreadAction { get; set; }
        public Action CallbackAction { get; set; }
        
        public void Invoke() {
            if (ThreadAction != null) ThreadAction();
            if (CallbackAction != null) CallbackAction();
        }

        public ThreadRequestPacket() {}
        public ThreadRequestPacket(Action thread_action) {
            ThreadAction = thread_action;
        }
        public ThreadRequestPacket(Action thread_action, Action callback_action) {
            ThreadAction = thread_action;
            CallbackAction = callback_action;
        }
    }
    
    public static void initialize() {
        StartTask(ThreadDispatcherThread, cancellation_token_source.Token);
    }   
    
    private static async void ThreadDispatcherThread() {
        while (State.running) {
            if (_task_count < _max_tasks && ThreadRequestQueue.TryDequeue(out var request)) {
                StartTask(request.Invoke, cancellation_token_source.Token);
            }
            
            Thread.Sleep(1);
        }
    }
    
    internal static CancellationTokenSource cancellation_token_source = new CancellationTokenSource();
    internal static CancellationToken cancellation_token => cancellation_token_source.Token;

    public static void reset_cancellation_token() {
        cancellation_token_source = new CancellationTokenSource();
    }


    public static void IncrementTaskCount() => Interlocked.Increment(ref _task_count);
    public static void DecrementTaskCount() => Interlocked.Decrement(ref _task_count);

    struct ThreadInfo {
        public string caller_filename;
        public string caller_member_name;

        public ThreadInfo(string caller_filename, string caller_member_name) {
            this.caller_filename = caller_filename;
            this.caller_member_name = caller_member_name;
        }
    }

    static ConcurrentDictionary<Guid, ThreadInfo> threads = new ConcurrentDictionary<Guid, ThreadInfo>();

    public static Task StartTask(Action action, [CallerFilePath] string callerfilename = "", [CallerMemberName] string membername = "") {
        Guid task_guid = Guid.NewGuid();
        
        return Task.Run(() => {
            IncrementTaskCount();
            try {
                threads.TryAdd(task_guid, new ThreadInfo(callerfilename, membername));
                action.Invoke();
            } finally {
                threads.TryRemove(task_guid, out _);
                DecrementTaskCount();
            }
        }, cancellation_token).ContinueWith(t => {
            if (t.IsFaulted) {
                Debug.WriteLine($"Task failed: ");
            }
        }, TaskContinuationOptions.OnlyOnFaulted);
    }

    public static Task StartTask(Action action, CancellationToken cancellation_token, [CallerFilePath] string callerfilename = "", [CallerMemberName] string membername = "") {            
        Guid task_guid = Guid.NewGuid();

        return Task.Run(() => {
            IncrementTaskCount();
            try {
                threads.TryAdd(task_guid, new ThreadInfo(callerfilename, membername));
                action.Invoke();
            } finally {
                threads.TryRemove(task_guid, out _);
                DecrementTaskCount();
            }
        }, cancellation_token).ContinueWith(t => {
            if (t.IsFaulted) {
                Debug.WriteLine($"Task failed: ");
            }
        }, TaskContinuationOptions.OnlyOnFaulted);
    }
    public static Task StartTask(Action action, out Guid guid, [CallerFilePath] string callerfilename = "", [CallerMemberName] string membername = "") {
        Guid task_guid = Guid.NewGuid();
        guid = task_guid;

        return Task.Run(() => {
            IncrementTaskCount();
            try {
                threads.TryAdd(task_guid, new ThreadInfo(callerfilename, membername));
                action.Invoke();
            } finally {
                 threads.TryRemove(task_guid, out _);
                DecrementTaskCount();
            }
        }, cancellation_token).ContinueWith(t => {
            if (t.IsFaulted) {
                Debug.WriteLine($"Task failed: ");
            }
        }, TaskContinuationOptions.OnlyOnFaulted);
    }

    public static Task StartTask(Action action, out Guid guid, CancellationToken cancellation_token, [CallerFilePath] string callerfilename = "", [CallerMemberName] string membername = "") {
        Guid task_guid = Guid.NewGuid();
        guid = task_guid;

        return Task.Run(() => {
            IncrementTaskCount();
            try {
                threads.TryAdd(task_guid, new ThreadInfo(callerfilename, membername));
                action.Invoke();
            } finally {
                threads.TryRemove(task_guid, out _);
                DecrementTaskCount();
            }
        }, cancellation_token).ContinueWith(t => {
            if (t.IsFaulted) {
                Debug.WriteLine($"Task failed: ");
            }
        }, TaskContinuationOptions.OnlyOnFaulted);
    }
}