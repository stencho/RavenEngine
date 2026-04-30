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