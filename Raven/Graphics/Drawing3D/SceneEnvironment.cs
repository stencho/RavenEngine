using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Raven.Engine;
using Raven.Graphics.Drawing2D;
using Raven.Graphics.Drawing3D;
using Raven.Graphics.Drawing3D.Effects;
using Raven.Graphics.Skybox;
using static Raven.Engine.State;
using DirectionalLight = Raven.Graphics.Drawing3D.Effects.DirectionalLight;

namespace Raven.Graphics;

public static class SkyboxData {
    public static SkyBoxTesselator skybox_t = new SkyBoxTesselator();
    
    public static VertexPositionColorNormalTexture[] skybox_data;
    public static int[] skybox_indices;
    
    public static float skybox_height = 0f;
}

public class SceneEnvironment : GameSystem {
    public SkyboxRenderer skybox;
    public DirectionalLight sunlight;

    public float current_time_ms = 0;
    
    public float entire_day_cycle_length_s = 20;
    public float entire_day_cycle_length_ms => entire_day_cycle_length_s * 1000;
    
    public float current_day_value => current_time_ms / entire_day_cycle_length_ms;

    public float time_multiplier = 1f;
    public bool time_stopped = true;
    
    public Vector3 sun_direction => Vector3.Normalize(Vector3.One);
    
    public Color sky_maximum_darkness = Color.FromNonPremultiplied(7, 5, 9, 255);
    
    public Draw2D.GradientLineGenerator sky_color_cycle;
    public Draw2D.GradientLineGenerator atmosphere_color_cycle;

    public Color sky_color => sky_color_cycle.get_color_at(current_day_value);
    public Color atmosphere_color  => atmosphere_color_cycle.get_color_at(current_day_value);

    public SceneEnvironment() : base() {
        skybox = new SkyboxRenderer(this);
        sunlight = new DirectionalLight(this);
        
        sky_color_cycle = new Draw2D.GradientLineGenerator(sky_maximum_darkness);
        sky_color_cycle.add_lerp(sky_maximum_darkness, 0f); 
        sky_color_cycle.add_lerp(sky_maximum_darkness, (1.0f/24f) * 3f); 
        sky_color_cycle.add_lerp(sky_maximum_darkness, (1.0f/24f) * 5f); 

        sky_color_cycle.add_lerp(Color.FromNonPremultiplied(100, 50, 150, 255), (1.0f/24f) * 7f);
        sky_color_cycle.add_lerp(Color.FromNonPremultiplied(76, 70, 178, 255), (1.0f/24f) * 10f);
        sky_color_cycle.add_lerp(Color.FromNonPremultiplied(76, 70, 178, 255), (1.0f/24f) * 12f);
        sky_color_cycle.add_lerp(Color.FromNonPremultiplied(76, 70, 178, 255), (1.0f/24f) * 14f);
        sky_color_cycle.add_lerp(Color.FromNonPremultiplied(100, 50, 150, 255), (1.0f/24f) * 17f);
        
        sky_color_cycle.add_lerp(sky_maximum_darkness, (1.0f/24f) * 19f);
        sky_color_cycle.add_lerp(sky_maximum_darkness, (1.0f/24f) * 21f);
        sky_color_cycle.add_lerp(sky_maximum_darkness, 1f);

        sky_color_cycle.build_debug_band_texture();
        
        atmosphere_color_cycle = new Draw2D.GradientLineGenerator(sky_maximum_darkness);
        atmosphere_color_cycle.add_lerp(sky_maximum_darkness, 0f); 
        atmosphere_color_cycle.add_lerp(sky_maximum_darkness, (1.0f/24f) * 3f); 
        atmosphere_color_cycle.add_lerp(sky_maximum_darkness, (1.0f/24f) * 5f); 
        
        atmosphere_color_cycle.add_lerp(Color.FromNonPremultiplied(220, 180, 200, 255), (1.0f/24f) * 7f);
        atmosphere_color_cycle.add_lerp(Color.FromNonPremultiplied(210, 200, 230, 255), (1.0f/24f) * 10f);
        atmosphere_color_cycle.add_lerp(Color.FromNonPremultiplied(210, 200, 255, 255), (1.0f/24f) * 12f);
        atmosphere_color_cycle.add_lerp(Color.FromNonPremultiplied(210, 200, 230, 255), (1.0f/24f) * 14f);
        atmosphere_color_cycle.add_lerp(Color.FromNonPremultiplied(220, 180, 200, 255), (1.0f/24f) * 17f);
        
        atmosphere_color_cycle.add_lerp(sky_maximum_darkness, (1.0f/24f) * 19f);
        atmosphere_color_cycle.add_lerp(sky_maximum_darkness, (1.0f/24f) * 21f);
        atmosphere_color_cycle.add_lerp(sky_maximum_darkness, 1f);

        atmosphere_color_cycle.build_debug_band_texture();
        
        current_time_ms = entire_day_cycle_length_ms * 0.75f;
    }
    
    public override void UpdateGraphics() {
        //haven't maxed out the day yet
        if (current_time_ms <= entire_day_cycle_length_ms)
            current_time_ms += (!time_stopped ? (float)Clock.render_delta_time_ms : 0) * time_multiplier;
        
        //have maxed out day, subtract a day
        if (current_time_ms > entire_day_cycle_length_ms) current_time_ms -= entire_day_cycle_length_ms;
        
        //have just subtracted a day- since the time is now probably negative, subtract the absolute value from the total day
        if (current_time_ms < 0) current_time_ms = entire_day_cycle_length_ms - Math.Abs(current_time_ms);
    }
    
    public void set_time_of_day(float normalized_time) {
        current_time_ms = normalized_time * entire_day_cycle_length_ms;
    }
    
    
    public override void Update() {}
    public override void UpdateEndOfFrame() {}
    public override void UpdateGraphicsEndOfFrame() {}
}