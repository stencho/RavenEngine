using Microsoft.Xna.Framework;
using Raven.Engine;

namespace Raven.Graphics.InterpolatedTypes;

public class LerpedVector2 : AutoInterpolate<Vector2> {
    public LerpedVector2(Vector2 start_value, Vector2 end_value, double length_ms)
        : base(start_value, end_value, length_ms) {
        init();
    }

    public LerpedVector2(Vector2 start_value, Vector2 end_value, double length_ms, 
        InterpolationType interpolation_type = InterpolationType.Loop, 
        EngineThread interpolation_thread = EngineThread.Render) 
        : base(start_value, end_value, length_ms, interpolation_type, interpolation_thread) {
        init();
    }

    private void init() {
        get_tween = tween;
    }

    Vector2 tween(Vector2 start, Vector2 end, double progress) {
        return Vector2.LerpPrecise(start, end, (float)progress);
    }
}


public class LerpedVector2D {
    private Vector2 min,max;
    private Vector2 position_absolute;
    private Vector2 position_relative = Vector2.Zero;
    
    private EngineThread engine_thread = EngineThread.Render;
    float DELTA => engine_thread == EngineThread.Render ? Clock.render_delta_time_f : Clock.update_delta_time_f;
    
    public LerpedVector2D(Vector2 min, Vector2 max, Vector2 direction, float speed, EngineThread interpolation_thread = EngineThread.Render) {
        init(min, max);
    }
    
    void init(Vector2 min, Vector2 max) {
        this.min = min; this.max = max;
        position_absolute = min;
    }

    public void Update() {
        
    }
}
