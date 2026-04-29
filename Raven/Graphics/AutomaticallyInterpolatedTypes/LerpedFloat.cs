using System;
using Raven.Engine;

namespace Raven.Graphics.InterpolatedTypes;

public class LerpedFloat : AutoInterpolate<float> {
    public LerpedFloat(float start_value, float end_value, double length_ms, 
        InterpolationType interpolation_type = InterpolationType.Loop, 
        EngineThread interpolation_thread = EngineThread.Render) 
        : base(start_value, end_value, length_ms, interpolation_type, interpolation_thread) {
        init();
    }

    void init() {
        get_tween = tween;
    }
    
    float tween(float start, float end, double progress) {
        return float.Lerp(start, end, (float)progress);
    }
}

public class FloatLerperManual {
    private float start = 0;
    private float end = 0;

    private float length_ms = 0;
    private float position_ms = 0;

    private float lerp_position = 0;

    public float Start  => start;
    public float End => end;
    
    public float Value =>  float.Lerp(start, end, lerp_position);


    private InterpolationType InterpolationType { get; set; } = InterpolationType.Once;
    private EngineThread Thread { get; set; } = EngineThread.Render;
    
    public FloatLerperManual(float start_value, float end_value, float length_ms, 
        InterpolationType interpolation_type = InterpolationType.Once, 
        EngineThread interpolation_thread  = EngineThread.Render) {
        
        start = start_value;
        end = end_value;
        
        InterpolationType = interpolation_type;
        Thread = interpolation_thread;
        
        this.length_ms = length_ms;
    }
            
    public void Lerp() {
        var delta = Thread == EngineThread.Render ? Clock.render_delta_time_ms_f : (float)State.scene_update_thread.delta_ms;
        position_ms += delta;
        lerp();
    }
    public void LerpReverse() {
        var delta = Thread == EngineThread.Render ? Clock.render_delta_time_ms_f : (float)State.scene_update_thread.delta_ms;
        position_ms -= delta;
        lerp();
    }

    void lerp() {
        switch (InterpolationType) {
            case InterpolationType.Once:
                position_ms = float.Clamp(position_ms, 0f, length_ms);
                lerp_position = float.Clamp(position_ms / length_ms, 0f, 1f);
                break;
            
            case InterpolationType.Loop:
                lerp_position = (MathF.Abs(position_ms) % length_ms) / length_ms;
                break;
            
            case InterpolationType.Bounce:
                var flipflop = (int)(MathF.Abs(position_ms) / length_ms) % 2 == 0;
                if (flipflop) lerp_position = (MathF.Abs(position_ms) % length_ms) / length_ms;
                else lerp_position = 1f - (MathF.Abs(position_ms) % length_ms) / length_ms;
                break;
            
            case InterpolationType.OnceEvery: throw new NotImplementedException();
            case InterpolationType.OnceEveryRandom: throw new NotImplementedException();
            default: throw new ArgumentOutOfRangeException();
        }
    }
}