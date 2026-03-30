using System;
using System.Diagnostics;
using System.Threading;

namespace Raven.Engine;

public class TimedUpdateLoop {
    private Clock.TickRateWatcher tick_rate_watcher = new();
    private Stopwatch stopwatch;

    public double poll_rate;
    public TimeSpan poll_goal_time => new TimeSpan((long)(10000 * (1000.0/poll_rate)));

    public double TPS => tick_rate_watcher.TicksPerSecond;
    
    CancellationTokenSource cts = new();
    private CancellationToken cancellation_token => cts.Token;

    public Action UpdateAction { get; set; }

    public TimedUpdateLoop(string thread_name, double poll_rate, Action update_action) {
        this.poll_rate = poll_rate;
        StartUpdateLoopThread(thread_name, update_action);
    }
    
    private void StartUpdateLoopThread(string name, Action update_action) {
        UpdateAction = update_action;
        Threads.StartTask(name, UpdateLoop);
    }

    public void StopUpdateLoop() {
        cts.Cancel();
    }

    private void UpdateLoop() {
        stopwatch = Stopwatch.StartNew();
        long start_tick = 0;
        
        double elapsed_ms() => (stopwatch.ElapsedTicks - start_tick) * 1000.0 / (double)Stopwatch.Frequency;
        double remaining_ms() => (poll_goal_time.TotalMilliseconds - elapsed_ms()) * 1000.0 / Stopwatch.Frequency;

        while (!State.wait_for_init) ;
        while (!Threads.IsCancellationRequested && !cts.IsCancellationRequested) {
            start_tick = stopwatch.ElapsedTicks;
            
            UpdateAction?.Invoke();

            if (remaining_ms() > 1.0) {
                Thread.Sleep((int)(remaining_ms() - 0.5));
            }
            while (remaining_ms() > 0.0 && !Threads.IsCancellationRequested) {
                Thread.SpinWait(1);
            }
            
            tick_rate_watcher.PollRateUpdate(elapsed_ms());
        }
    } 
}

public class HighFrequencyUpdateLoop {
    private Clock.TickRateWatcher tick_rate_watcher = new();
    private Stopwatch stopwatch;

    public long goal_ticks_per_update;
    public double last_ms = 0;
    public long last_ticks = 0;
    public double TPS => tick_rate_watcher.TicksPerSecond;
    
    CancellationTokenSource cts = new();
    private CancellationToken cancellation_token => cts.Token;

    public Action UpdateAction { get; set; }

    public HighFrequencyUpdateLoop(string thread_name, long goal_ticks_per_update, Action update_action) {
        this.goal_ticks_per_update = goal_ticks_per_update;
        StartUpdateLoopThread(thread_name, update_action);
    }
    
    private void StartUpdateLoopThread(string name, Action update_action) {
        UpdateAction = update_action;
        Threads.StartTask(name, UpdateLoop);
    }

    public void StopUpdateLoop() {
        cts.Cancel();
    }

    private void UpdateLoop() {
        stopwatch = Stopwatch.StartNew();
        long start_tick = 0;
        
        double elapsed_ms() => (stopwatch.ElapsedTicks - start_tick) * 1000.0 / (double)Stopwatch.Frequency;
        long elapsed_ticks() => (stopwatch.ElapsedTicks - start_tick);

        while (!State.wait_for_init) ;
        while (!Threads.IsCancellationRequested && !cts.IsCancellationRequested) {
            start_tick = stopwatch.ElapsedTicks;
            var start_ms =  stopwatch.ElapsedMilliseconds;
            UpdateAction?.Invoke();

            while (elapsed_ticks() < goal_ticks_per_update && !Threads.IsCancellationRequested) {
                Thread.SpinWait(1);
            }

            last_ticks = elapsed_ticks();

            var ms = elapsed_ms();
            last_ms = stopwatch.ElapsedMilliseconds - start_ms;
            tick_rate_watcher.PollRateUpdate(ms);
        }
    } 
}