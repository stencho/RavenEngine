using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Raven.Engine;
using Raven.Graphics.Drawing2D;
using Raven.Graphics.Drawing3D;
using static Raven.Engine.State;

namespace Raven.Graphics.Skybox;
public static class SkyboxState {
    public static SkyBoxTesselator skybox_t = new SkyBoxTesselator();
    public static VertexPositionNormalColorUv[] skybox_data;
    public static int[] skybox_indices;
    public static int skybox_face_res = 1024;
    public static RenderTarget2D skybox_cm;
    public static RenderTarget2D skybox_cm_e;
    
    public static SunMoonSystem sun_moon = new SunMoonSystem();
}

public class SunMoonSystem {
    public static Color night_ambient = Color.FromNonPremultiplied(5,3,6, 255);

    public Color atmosphere_color = Color.FromNonPremultiplied(4, 4, 9, 255);

    public Color sky_color = Color.Lerp(Color.Purple, Color.LightSkyBlue, 0.2f);

    public Vector3 sun_direction => Vector3.Normalize((Vector3.Down * 5) + (Vector3.Down * 3) + Vector3.Forward);

    private Matrix sun_orientation = Matrix.Identity * Matrix.CreateRotationX(MathHelper.ToRadians(-75f)) * Matrix.CreateRotationZ(MathHelper.ToRadians(-15f));
    public Color current_color = Color.White;

    public Draw2D.GradientLineGenerator lerps;

    public double time_multiplier = 1f;

    //Directional light and distance fog info
    public float sun_max_brightness = 0.75f;
    public float sun_brightness_percent = 1.0f;

    public float moon_max_brightness = 0.2f;
    public float moon_brightness_percent = 0f;
    
    public double entire_day_cycle_length_ms = 20 * 1 * 1000;

    public double day_length_ratio = 0.5;

    public double day_length => day_length_ratio * entire_day_cycle_length_ms;
    public double night_length => 1 - day_length_ratio * entire_day_cycle_length_ms;

    public double current_time_ms = 0;
    public double current_time_entire_day_percent => current_time_ms / entire_day_cycle_length_ms;

    public double current_day_value => current_time_ms / entire_day_cycle_length_ms;

    public bool time_stopped = true;

    public TimeSpan cycle_ts => new TimeSpan(0, 0, 0, 0, (int)entire_day_cycle_length_ms);
    public TimeSpan cycle_ts_scaled => new TimeSpan(0, 0, 0, 0, (int)(entire_day_cycle_length_ms / time_multiplier));

    public SunMoonSystem() {
        lerps = new Draw2D.GradientLineGenerator(night_ambient);
        lerps.add_lerp(night_ambient, 0f);
        lerps.add_lerp(night_ambient, (1.0f/24f) * 5f);

        //back down to orange just before dawn
        lerps.add_lerp(Color.FromNonPremultiplied(210, 110, 130, 255), (1.0f/24f) * 6.5f);

        //midday sky
        lerps.add_lerp(Color.FromNonPremultiplied(220, 175, 245, 255), (1.0f/24f) * 7.5f);
        lerps.add_lerp(Color.FromNonPremultiplied(240, 230, 255, 255), (1.0f/24f) * 13f);
        lerps.add_lerp(Color.FromNonPremultiplied(220, 200, 255, 255), (1.0f/24f) * 17.5f);

        //back down to orange just before dusk
        lerps.add_lerp(Color.FromNonPremultiplied(210, 110, 130, 255), (1.0f/24f) * 18.5f);
        //lerps.add_lerp(Color.FromNonPremultiplied(8, 2, 10, 255), .87f);

        lerps.add_lerp(night_ambient, (1.0f/24f) * 20f);
        lerps.add_lerp(night_ambient, 1f);

        lerps.build_debug_band_texture();

        current_time_ms = entire_day_cycle_length_ms / 2f;
    }

    public void update() {
        //haven't maxed out the day yet
        if (current_time_ms <= entire_day_cycle_length_ms)
            current_time_ms += (!time_stopped ? Clock.render_delta_time_ms : 0) * time_multiplier;
        
        //have maxed out day, subtract a day
        if (current_time_ms > entire_day_cycle_length_ms) current_time_ms -= entire_day_cycle_length_ms;
        
        //have just subtracted a day- since the time is now probably negative, subtract the absolute value from the total day
        if (current_time_ms < 0) current_time_ms = entire_day_cycle_length_ms - Math.Abs(current_time_ms);

        current_color = lerps.get_color_at((float)SkyboxState.sun_moon.current_day_value);

        sky_color = Color.Lerp(Color.MidnightBlue, current_color, 0.9f) * 0.3f;
        atmosphere_color = Color.Lerp(Color.LightSkyBlue, current_color, .5f) * 0.75f;
    }

    public void set_time_of_day(double normalized_time) {
        current_time_ms = normalized_time * entire_day_cycle_length_ms;
    }


    public void configure_dlight_shader(Camera camera, GBuffer gbuffer, Effect e_directionallight) {

        //e_directionallight.Parameters["fog"].SetValue(true);z
        //e_directionallight.Parameters["fog_start"].SetValue(0.5f);

        //e_directionallight.Parameters["camera_pos"].SetValue(State.camera.position);
        //e_directionallight.Parameters["FarClip"].SetValue(State.camera.far_clip);

        e_directionallight.Parameters["NORMAL"].SetValue(gbuffer.rt_normal);
        //e_directionallight.Parameters["DEPTH"].SetValue(State.buffer.rt_depth);

        e_directionallight.Parameters["InverseView"].SetValue(Matrix.Invert(camera.view));

        e_directionallight.Parameters["AtmosphereColor"].SetValue(lerps.get_color_at((float)SkyboxState.sun_moon.current_day_value).ToVector3());
        e_directionallight.Parameters["AtmosphereIntensity"].SetValue(0.5f);

        e_directionallight.Parameters["LightColor"].SetValue(lerps.get_color_at((float)SkyboxState.sun_moon.current_day_value).ToVector3());
        e_directionallight.Parameters["LightIntensity"].SetValue(1f);

        e_directionallight.Parameters["LightDirection"].SetValue(sun_direction);
        //e_directionallight.Parameters["camera_pos"].SetValue(State.camera.position);

        e_directionallight.CurrentTechnique.Passes[0].Apply();
        graphics_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, quad_vb.VertexCount);           

    }

}