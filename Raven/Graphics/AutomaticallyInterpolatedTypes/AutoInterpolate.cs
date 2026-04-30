using System;
using System.Collections.Generic;
using Raven.Engine;

namespace Raven.Graphics.InterpolatedTypes;

public enum InterpolationType { Loop, Bounce, Once, OnceEvery, OnceEveryRandom }

public interface IAutoInterpolate {
    public static partial class Manager {
        public static readonly HashSet<IAutoInterpolate> interpolated = new();

        public static void Add<I>(I start_value, I end_value, double length, InterpolationType interpolation_type = InterpolationType.Loop, EngineThread interpolation_thread = EngineThread.Render) {
            interpolated.Add(new AutoInterpolate<I>(start_value, end_value, length, interpolation_type, interpolation_thread));
        }

        public static void Add(IAutoInterpolate ai) {
            interpolated.Add(ai);
        }

        public static void Remove(IAutoInterpolate ai) {
            interpolated.Remove(ai);
        } 
    
        public static void UpdateRenderThread(double delta) {
            foreach (IAutoInterpolate iai in interpolated) {
                if (iai.interpolation_thread == EngineThread.Render)
                    iai.lerp(delta);
            }
        }
    
        public static void UpdateInternalLoop(double delta) {
            foreach (var iai in interpolated) {
                if (iai.interpolation_thread == EngineThread.Update) {
                    iai.lerp(delta);
                }
            }
        }
    
    }
    
    Type type { get; }

    EngineThread interpolation_thread { get; set; } 
    
    public void lerp(double delta);
}

public partial class AutoInterpolate<I> : IAutoInterpolate {
    public Type type { get; } private Type _type = typeof(I);
    
    public I start_value, end_value;

    internal Func<I, I, double, I> get_tween;
    public I tween_value => get_tween(start_value, end_value, progress);
    
    public double time_ms = 0;
    public double length_ms = 1000;
    
    public double progress => interpolation_type == InterpolationType.Bounce && !bounce_flipflop ? 1.0 - (time_ms / length_ms) : time_ms / length_ms;
    public float progress_f => (float)progress;
    
    Func<AutoInterpolate<I>> right_end_func;
    Func<AutoInterpolate<I>> left_end_func;

    InterpolationType interpolation_type = InterpolationType.Loop;
    public EngineThread interpolation_thread { get; set; } = EngineThread.Render;
    
    public void SetRightEndFunc(Func<AutoInterpolate<I>> func) { right_end_func = func; }
    public void SetLeftEndFunc(Func<AutoInterpolate<I>> func) { left_end_func = func; }
    
    public void SetInterpolationType(InterpolationType type) { interpolation_type = type; }
    public void SetInterpolationThread(EngineThread thread) { interpolation_thread = thread; }
    
    public void SetOnceEveryInterval(double interval) { once_every_ms = interval; }
    
    public AutoInterpolate(I start_value, I end_value, double length_ms) {
        this.start_value = start_value;
        this.end_value = end_value;
        this.length_ms = length_ms;
        
        IAutoInterpolate.Manager.Add(this);
    }
    
    public AutoInterpolate(I start_value, I end_value, double length_ms, InterpolationType interpolation_type = InterpolationType.Loop, EngineThread interpolation_thread = EngineThread.Render) {
        this.start_value = start_value;
        this.end_value = end_value;
        this.length_ms = length_ms;
        
        this.interpolation_type = interpolation_type;
        this.interpolation_thread = interpolation_thread;
        
        IAutoInterpolate.Manager.Add(this);
    }

    ~AutoInterpolate() {
        IAutoInterpolate.Manager.Remove(this);
    }
    
    private bool bounce_flipflop = true;
    private double once_every_ms = 0;
    private double once_every_ms_time_since_last = 0;
    
    public void lerp(double delta) {
        switch (interpolation_type) {
            case InterpolationType.Loop:
                time_ms += delta;
                if (time_ms > length_ms) {
                    time_ms -= length_ms;
                    right_end_func?.Invoke();
                }
                break;
            
            case InterpolationType.Bounce:
                time_ms += delta;
                if (time_ms > length_ms) {
                    time_ms -= length_ms;
                    
                    bounce_flipflop = !bounce_flipflop;
                    
                    if (bounce_flipflop) right_end_func?.Invoke();
                    else left_end_func?.Invoke();
                }
                
                break;
            
            case InterpolationType.Once:       
                time_ms += delta;
                if (time_ms > length_ms) {
                    time_ms = length_ms;
                    right_end_func?.Invoke();
                }
                break;
            
            case InterpolationType.OnceEvery:
                
                break;
            
            default: throw new ArgumentOutOfRangeException();
        }
        
        time_ms += delta;
    }
}

