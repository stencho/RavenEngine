using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RavenRPG.Engine.Controls;
using RavenRPG.Renderer;
using RavenRPG.Renderer.Drawing;
using RavenRPG.Engine.World;
using RavenRPG.Renderer.Lights;
using static RavenRPG.Engine.Controls.StaticControlBinds;

namespace RavenRPG.Engine;
public static class State {
    public static class Skybox {
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

        public bool time_stopped = false;

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
                current_time_ms += (!time_stopped ? Clock.delta_time_ms : 0) * time_multiplier;
            
            //have maxed out day, subtract a day
            if (current_time_ms > entire_day_cycle_length_ms) current_time_ms -= entire_day_cycle_length_ms;
            
            //have just subtracted a day- since the time is now probably negative, subtract the absolute value from the total day
            if (current_time_ms < 0) current_time_ms = entire_day_cycle_length_ms - Math.Abs(current_time_ms);

            current_color = lerps.get_color_at((float)Skybox.sun_moon.current_day_value);

            sky_color = Color.Lerp(Color.MidnightBlue, current_color, 0.9f) * 0.3f;
            atmosphere_color = Color.Lerp(Color.LightSkyBlue, current_color, .5f) * 0.75f;
        }

        public void set_time_of_day(double normalized_time) {
            current_time_ms = normalized_time * entire_day_cycle_length_ms;
        }


        public void configure_dlight_shader(Effect e_directionallight) {

            //e_directionallight.Parameters["fog"].SetValue(true);
            //e_directionallight.Parameters["fog_start"].SetValue(0.5f);

            //e_directionallight.Parameters["camera_pos"].SetValue(EngineState.camera.position);
            //e_directionallight.Parameters["FarClip"].SetValue(EngineState.camera.far_clip);

            e_directionallight.Parameters["NORMAL"].SetValue(buffer.rt_normal);
            //e_directionallight.Parameters["DEPTH"].SetValue(EngineState.buffer.rt_depth);

            e_directionallight.Parameters["InverseView"].SetValue(Matrix.Invert(camera.view));

            e_directionallight.Parameters["AtmosphereColor"].SetValue(lerps.get_color_at((float)Skybox.sun_moon.current_day_value).ToVector3());
            e_directionallight.Parameters["AtmosphereIntensity"].SetValue(0.5f);

            e_directionallight.Parameters["LightColor"].SetValue(lerps.get_color_at((float)Skybox.sun_moon.current_day_value).ToVector3());
            e_directionallight.Parameters["LightIntensity"].SetValue(1f);

            e_directionallight.Parameters["LightDirection"].SetValue(sun_direction);
            //e_directionallight.Parameters["camera_pos"].SetValue(EngineState.camera.position);

            e_directionallight.CurrentTechnique.Passes[0].Apply();
            graphics_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, quad_vb.VertexCount);           

        }

    }
    public static Game game;
    public static ContentManager content_manager;
    public static GameWindow window;
    public static GraphicsDeviceManager graphics;
    public static GraphicsDevice graphics_device => graphics.GraphicsDevice;
    public static SpriteBatch sprite_batch;
    
    public static Vector2i resolution => gvars.get_Vector2i("resolution");

    public static GBuffer buffer;
    public static Camera camera;

    private static Camera test_camera;
    private static Model test_skull; 
    
    public static Viewport viewport => graphics_device.Viewport;
    
    public static Scene scene = new();

    public static Action Draw_2D;
    public static Action Draw_3D;
    
    public static bool is_active => game.IsActive;
    public static bool show_all_debug_info = false;
    
    public static byte buffer_count = 3;
    public static int draw_debug_buffer = -1;
    
    internal static Effect e_gbuffer;
    internal static Effect e_light_depth;
    internal static Effect e_exp_light_depth;
    internal static Effect e_skybox;
    internal static Effect e_compositor;
    internal static Effect e_clear;
    internal static Effect e_directionallight;
    internal static Effect e_spotlight;
    internal static Effect e_pointlight;

    internal static VertexBuffer quad_vb;
    internal static IndexBuffer quad_ib;
    
    internal static VertexPositionTexture[] vb_data = {
        new VertexPositionTexture(Vector3.Up + Vector3.Left,      Vector2.Zero),
        new VertexPositionTexture(Vector3.Up + Vector3.Right,     Vector2.UnitX),
        new VertexPositionTexture(Vector3.Down + Vector3.Left,    Vector2.UnitY),
        new VertexPositionTexture(Vector3.Down + Vector3.Right,   Vector2.One)
    };
    internal static int[] ib_data = { 0, 1, 2, 1, 3, 2 };
    
    public static void Initialize(Game game, ContentManager content, GraphicsDeviceManager graphics, GameWindow window) {
        State.game = game;
        
        game.Exiting += (sender, args) => {
            UpdateThread.Stop();
            gvars.write_gvars_to_disk();
        };
        
        State.content_manager = content;
        State.graphics = graphics;
        State.window =  window;
        State.sprite_batch = new SpriteBatch(State.graphics_device);

        game.IsFixedTimeStep = false;
        game.InactiveSleepTime = TimeSpan.Zero;
        graphics.SynchronizeWithVerticalRetrace = false;
        window.AllowUserResizing = false;
        
        gvars.add_gvar("resolution", gvar_data_type.VECTOR2I, FindDefaultResolution(), true);
        gvars.add_gvar("super_resolution_scale", gvar_data_type.FLOAT, 1f, true);
        gvars.add_gvar("frame_limit", gvar_data_type.INT, 180, true);
        gvars.add_gvar("vsync", gvar_data_type.BOOL, true, true);
        gvars.add_gvar("light_spot_resolution", gvar_data_type.INT, 1024, false);
        
        buffer = new GBuffer();
        buffer.CreateInPlace(graphics_device, resolution.X, resolution.Y);
        change_backbuffer_resolution();
        
        ChangeResolution(true);
        
        gvars.add_change_action("resolution", () => ChangeResolution());
        gvars.add_change_action("vsync", () => { graphics.SynchronizeWithVerticalRetrace = gvars.get_bool("vsync"); });
        gvars.add_change_action("frame_limit", () => ChangeFrameLimit());
        ChangeFrameLimit();
            
        bool read_gvars = gvars.read_gvars_from_disk();
        Debug.WriteLine($"{read_gvars} GVARS:\n{gvars.list_all()}");
                
        add_bindings(
            (bind_type.digital, controller_type.keyboard, Keys.W, "forward"),
            (bind_type.digital, controller_type.keyboard, Keys.A, "left"),
            (bind_type.digital, controller_type.keyboard, Keys.D, "right"),
            (bind_type.digital, controller_type.keyboard, Keys.S, "backward"),

            (bind_type.digital, controller_type.keyboard, Keys.Up, "t_forward"),
            (bind_type.digital, controller_type.keyboard, Keys.Left, "t_left"),
            (bind_type.digital, controller_type.keyboard, Keys.Right, "t_right"),
            (bind_type.digital, controller_type.keyboard, Keys.Down, "t_backward"),

            (bind_type.digital, controller_type.keyboard, Keys.PageUp, "t_up"),
            (bind_type.digital, controller_type.keyboard, Keys.PageDown, "t_down"),

            (bind_type.digital, controller_type.keyboard, Keys.Home, "t_upper"),
            (bind_type.digital, controller_type.keyboard, Keys.End, "t_downer"),

            (bind_type.digital, controller_type.keyboard, Keys.Space, "up"),
            (bind_type.digital, controller_type.keyboard, Keys.C, "down"),

            (bind_type.digital, controller_type.keyboard, Keys.T, "test"),
            (bind_type.digital, controller_type.keyboard, Keys.Y, "test_sweep"),
            (bind_type.digital, controller_type.keyboard, Keys.Q, "t_L"),
            (bind_type.digital, controller_type.keyboard, Keys.E, "t_R"),
            (bind_type.digital, controller_type.keyboard, Keys.R, "t_S"),


            (bind_type.digital, controller_type.keyboard, Keys.LeftShift, "shift"),
            (bind_type.digital, controller_type.keyboard, Keys.LeftControl, "ctrl"),

            (bind_type.digital, controller_type.mouse, Input.MouseButtons.Left, "click"), 
            (bind_type.digital, controller_type.mouse, Input.MouseButtons.Right, "click_right"),
            (bind_type.digital, controller_type.mouse, Input.MouseButtons.Middle, "click_middle"),
            (bind_type.digital, controller_type.mouse, Input.MouseButtons.ScrollUp, "scroll_up"),
            (bind_type.digital, controller_type.mouse, Input.MouseButtons.ScrollDown, "scroll_down"),

            (bind_type.digital, controller_type.keyboard, Keys.F, "t_supp" ),
            (bind_type.digital, controller_type.keyboard, Keys.D1, "speenL"),
            (bind_type.digital, controller_type.keyboard, Keys.D3, "speenR" ),
            (bind_type.digital, controller_type.keyboard, Keys.R,  "t_S"),
            (bind_type.digital, controller_type.keyboard, Keys.Q, "t_L" ),
            (bind_type.digital, controller_type.keyboard, Keys.E,"t_R" ),

            (bind_type.digital, controller_type.keyboard, Keys.OemTilde, "toggle_console" ),
            (bind_type.digital, controller_type.keyboard, Keys.F3, "toggle_full_info" ),
            (bind_type.digital, controller_type.keyboard, Keys.F4, "switch_buffer" )
            );
        
        force_enable("toggle_console");
        force_enable("screenshot");
        
        scene.update_thread.Start();
    }
    
    public static void Update(GameTime gt) {
        Clock.game_time = gt;
        Skybox.sun_moon.update();

        Vector3 movement = Vector3.Zero;
        
        if (pressed("forward")) movement += Vector3.Forward;
        if (pressed("backward")) movement += Vector3.Backward;
        if (pressed("left")) movement += Vector3.Left;
        if (pressed("right")) movement += Vector3.Right;
        if (pressed("up")) movement += Vector3.Up;
        if (pressed("down")) movement += Vector3.Down;

        camera.position += movement * (float)Clock.delta_time;
        
        camera.update();
        camera.update_projection(resolution);
    }
    
    public static void Load(ContentManager content) {
        Resources.LoadContentList(content);
        
        quad_vb = new VertexBuffer(graphics_device, VertexPositionTexture.VertexDeclaration, 4, BufferUsage.None);
        quad_vb.SetData(vb_data);
        
        quad_ib = new IndexBuffer(graphics_device, IndexElementSize.ThirtyTwoBits, 6, BufferUsage.None);
        quad_ib.SetData(ib_data);

        var ctestpos = (Vector3.Backward) * 2f;
        var dir = Vector3.Normalize(-ctestpos);
        var lookat = Matrix.CreateLookAt(ctestpos, Vector3.Forward, 
            Vector3.Up);
        test_camera = new Camera(ctestpos, Vector3.Forward);
        camera = test_camera;
        
        e_gbuffer = Resources.GetShader("fill_gbuffer");
        e_light_depth = Resources.GetShader("light_depth");
        e_exp_light_depth = Resources.GetShader("exp_light_depth");
        e_skybox = Resources.GetShader("skybox");
        e_compositor = Resources.GetShader("compositor");
        e_clear = Resources.GetShader("clear");
        e_directionallight = Resources.GetShader("directionallight");
        e_spotlight = Resources.GetShader("spotlight");
        e_pointlight = Resources.GetShader("pointlight");
        
        test_skull = Resources.GetModel("skull");
        
        Draw2D.load();
        Draw3D.load();
        
        Skybox.skybox_t.PrivateCreateSkyboxFromCrossImage(out Skybox.skybox_data, out Skybox.skybox_indices, 1, 0, 1, 2, 3, 5, 4);
        Skybox.skybox_t.Subdivide(Skybox.skybox_data, Skybox.skybox_indices, out Skybox.skybox_data, out Skybox.skybox_indices, 16, MathHelper.Pi);
        Skybox.skybox_cm = new RenderTarget2D(graphics_device, Skybox.skybox_face_res * 4, Skybox.skybox_face_res * 3, false, SurfaceFormat.Rgba64, DepthFormat.Depth16);
        Skybox.skybox_cm_e = new RenderTarget2D(graphics_device, Skybox.skybox_face_res * 4, Skybox.skybox_face_res * 3, false, SurfaceFormat.Rgba64, DepthFormat.Depth16);

        Skybox.sun_moon.time_stopped = false;
        Skybox.sun_moon.set_time_of_day(0.5);
        
        graphics_device.SetRenderTarget(Skybox.skybox_cm);
        graphics_device.Clear(Skybox.sun_moon.atmosphere_color);

        graphics_device.SetRenderTarget(null);

        skull_lamp = new light {
            type = LightType.SPOT,
            color = Color.Red,
            spot_info = new spot_info()
        };
        
        
        skull_lamp.position = Vector3.Backward * 2;
        skull_lamp.spot_info.orientation = Matrix.CreateRotationX(float.DegreesToRadians(90f));
        //skull_lamp.spot_info.

    }
    
    private static Vector2i FindDefaultResolution() {
        var cdm = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
        return new Vector2i(cdm.Width, cdm.Height);
    }

    private static void ChangeFrameLimit() {
        if (gvars.get_int("frame_limit") == -1) {
            game.IsFixedTimeStep = false;
            game.TargetElapsedTime = new TimeSpan((long)((1000.0 / 60.0) * 10000.0));
        } else {
            game.IsFixedTimeStep = true;
            game.TargetElapsedTime = new TimeSpan((long)((1000.0 / (double)gvars.get_int("frame_limit")) * 10000.0));
        }
    }
    static void change_backbuffer_resolution() {
        graphics.PreferredBackBufferWidth = resolution.X;
        graphics.PreferredBackBufferHeight = resolution.Y;
        graphics.ApplyChanges();
    }
    public static void ChangeResolution(bool update_backbuffer = true) {
        if (update_backbuffer) {
            graphics.PreferredBackBufferWidth = resolution.X;
            graphics.PreferredBackBufferHeight = resolution.Y;

            graphics.ApplyChanges();
        }

        buffer.change_resolution(graphics_device, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
    }

    private static float skull_rotate = 0f;
    private static light skull_lamp;
    public static void Compose() {
        //create visibility list here
        
        build_lighting(camera, buffer);
        
        clear_to_skybox(camera, buffer);
        
        //DRAW 3D
        graphics_device.SetRenderTargets(buffer.buffer_targets);
        graphics_device.RasterizerState = RasterizerState.CullCounterClockwise;
        graphics_device.BlendState = BlendState.AlphaBlend;
        if (Draw_3D != null) Draw_3D.Invoke();
        
        graphics_device.SetRenderTargets(buffer.buffer_targets);
        Draw3D.draw_buffers_diffuse_texture(
            test_skull.Meshes[0].MeshParts[0].VertexBuffer, 
            test_skull.Meshes[0].MeshParts[0].IndexBuffer, 
            Resources.GetTexture("texture_1001"), Color.White, 
            Matrix.CreateTranslation(Vector3.Zero) * Matrix.CreateRotationY(float.DegreesToRadians(skull_rotate)));
        
        skull_rotate += 120f * (float)Clock.delta_time;
        if (skull_rotate > 360f) {
            skull_rotate -= 360f;
        }

        draw_lighting(camera, buffer);
        graphics_device.SetRenderTarget(buffer.rt_2D);
        graphics_device.BlendState = BlendState.AlphaBlend;
        graphics_device.Clear(Color.Transparent);
        
        //DRAW 2D
        
        if (Draw_2D != null) Draw_2D.Invoke();
        //StaticControlBinds.draw_state(600, 0, 100, 10, 10);
        var dayper = Skybox.sun_moon.current_time_entire_day_percent;
        bool afternoon = dayper > 0.5f;
        var hour = afternoon ? ((dayper - 0.5f) * 2) * 12f : (dayper * 2f) * 12f;
        if ((int)hour == 0) hour = 12;

        string buffer_text = "";
        switch (draw_debug_buffer) {
            case 0: buffer_text = " <diffuse>";  break;
            case 1: buffer_text = " <normals>";  break;
            case 2: buffer_text = " <depth>";    break;  
            case 3: buffer_text = " <lighting>"; break;
            default: buffer_text = ""; break;
        }

        var debug_str =
            $"[Render] {Clock.frame_rate} FPS{buffer_text}\n[Update] {Clock.tick_rate} Ticks/s\n[Environment] {(int)hour} O'clock\n\n";
        
        if (show_all_debug_info) {
            debug_str += $"\n\nGVARS\n{gvars.list_all()}\n\nASSETS\n{Resources.ListAllContent()}\n\n";
        }
        
        Draw2D.text_shadow(debug_str, Vector2i.UnitX * 2, Color.White, Color.Black);
        
        //Draw2D.image(Resources.GetTexture("Missing"), Vector2i.One * 200, Vector2i.One * 50);
        Draw2D.image(Skybox.sun_moon.lerps.debug_band, Vector2i.Down * 40, Skybox.sun_moon.lerps.debug_band.Bounds.Size.ToVector2i() + (Vector2i.UnitY * 10));
        var tl = (Vector2i.Down * 40) + (Skybox.sun_moon.lerps.debug_band.Bounds.Size.ToVector2i() * (float)dayper);
        Draw2D.line(tl, tl + (Vector2i.UnitY * 11), Color.Red, 1f);
        
        
        //COMPOSE GBUFFER TO RT_FINAL
        graphics_device.SetRenderTarget(buffer.rt_final);
        graphics_device.Clear(Color.Transparent);
        
        graphics_device.SetVertexBuffer(quad_vb);
        graphics_device.Indices = quad_ib;
        
        graphics_device.BlendState = BlendState.AlphaBlend;
        
        e_compositor.Parameters["DiffuseLayer"].SetValue(buffer.rt_diffuse);
        e_compositor.Parameters["DepthLayer"].SetValue(buffer.rt_depth);
        e_compositor.Parameters["LightLayer"].SetValue(buffer.rt_lighting);
        e_compositor.Parameters["NormalLayer"].SetValue(buffer.rt_normal);
        //e_compositor.Parameters["sky_brightness"].SetValue(Skybox.sun_moon.sky_brightness);
        //e_compositor.Parameters["atmosphere_color"].SetValue(Skybox.sun_moon.atmosphere_color.ToVector3());
        e_compositor.Parameters["buffer"].SetValue(draw_debug_buffer);
        
        e_compositor.Techniques["draw"].Passes[0].Apply();
        graphics_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
        
        //DRAW RT_FINAL AND RT_2D TO SCREEN
        graphics_device.SetRenderTarget(null);
        Draw2D.end();
        
        Draw2D.begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.None);
        Draw2D.image(buffer.rt_final, Vector2i.Zero, resolution);
        Draw2D.end();
        
        Draw2D.begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None);
        Draw2D.image(buffer.rt_2D, Vector2i.Zero, resolution);
        Draw2D.end();
    }
    
    public static void clear_to_skybox(Camera camera, GBuffer buffer) {
        graphics_device.DepthStencilState = DepthStencilState.None;

        graphics_device.SetRenderTargets(buffer.buffer_targets);
        e_clear.Parameters["color"].SetValue(Skybox.sun_moon.atmosphere_color.ToVector4());
        e_clear.Techniques["Default"].Passes[0].Apply();

        graphics_device.SetVertexBuffer(quad_vb);
        graphics_device.Indices = quad_ib;

        graphics_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
        graphics_device.RasterizerState = RasterizerState.CullCounterClockwise;
        graphics_device.BlendState = BlendState.AlphaBlend;

        e_skybox.Parameters["atmosphere_color"].SetValue(Skybox.sun_moon.atmosphere_color.ToVector4());
        e_skybox.Parameters["sky_color"].SetValue(Skybox.sun_moon.sky_color.ToVector4());

        e_skybox.Parameters["World"].SetValue(Matrix.CreateScale(1f) * Matrix.Identity);
        e_skybox.Parameters["View"].SetValue(Matrix.CreateLookAt(Vector3.Zero, camera.direction, camera.up_direction));
        e_skybox.Parameters["Projection"].SetValue(camera.projection);

        e_skybox.Techniques["draw"].Passes[0].Apply();

        graphics_device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, Skybox.skybox_data, 0, 2, Skybox.skybox_indices, 0, Skybox.skybox_indices.Length / 3, VertexPositionNormalColorUv.VertexDeclaration);

        graphics_device.DepthStencilState = DepthStencilState.Default;
    }
    
    
    public static void build_lighting(Camera camera, GBuffer buffer) {
        graphics_device.SetRenderTarget(buffer.rt_lighting);
        graphics_device.BlendState = BlendState.Opaque;
        graphics_device.Clear(Skybox.sun_moon.atmosphere_color);
/*
        foreach (light light in visible_lights) {
            if (light.type == LightType.SPOT) {

                graphics_device.SetRenderTarget(light.spot_info.depth_map);

                graphics_device.BlendState = BlendState.Opaque;
                graphics_device.DepthStencilState = DepthStencilState.Default;

                graphics_device.Clear(Color.Transparent);

                e_exp_light_depth.Parameters["View"].SetValue(light.spot_info.view);
                e_exp_light_depth.Parameters["Projection"].SetValue(light.spot_info.projection);
                //create_spot_light_visibility_list(map, light);

                e_exp_light_depth.Parameters["LightPosition"].SetValue(light.position);
                e_exp_light_depth.Parameters["LightDirection"].SetValue(light.spot_info.orientation.Forward);
                e_exp_light_depth.Parameters["LightClip"].SetValue(light.spot_info.far_clip);
                e_exp_light_depth.Parameters["C"].SetValue(light.spot_info.C);

                foreach (int i in light.spot_info.visible) {
                    map.game_objects[i].draw_to_light(light);
                }


            } else if (light.type == LightType.POINT) {
            }
        }
        */
    }
    
    public static void draw_lighting(Camera camera, GBuffer buffer) {
        graphics_device.SetRenderTarget(buffer.rt_lighting);

        e_pointlight.Parameters["View"].SetValue(camera.view);
        e_pointlight.Parameters["Projection"].SetValue(camera.projection);
        e_pointlight.Parameters["InverseView"].SetValue(Matrix.Invert(camera.view));
        e_pointlight.Parameters["InverseViewProjection"].SetValue(Matrix.Invert(camera.view * camera.projection));

        e_spotlight.Parameters["View"].SetValue(camera.view);
        e_spotlight.Parameters["Projection"].SetValue(camera.projection);
        e_spotlight.Parameters["InverseView"].SetValue(Matrix.Invert(camera.view));
        e_spotlight.Parameters["InverseViewProjection"].SetValue(Matrix.Invert(camera.view * camera.projection));

        graphics_device.BlendState = BlendState.AlphaBlend;
        graphics_device.DepthStencilState = DepthStencilState.DepthRead;

        graphics_device.SetVertexBuffer(quad_vb);
        graphics_device.Indices = quad_ib;

        Skybox.sun_moon.configure_dlight_shader(e_directionallight);

        graphics_device.BlendState = DynamicLightRequirements.blend_state;
        graphics_device.DepthStencilState = DepthStencilState.DepthRead;
        var light = skull_lamp;
        /*
        foreach(light light in visible_lights) {
            if (light.type == LightType.SPOT) {
        
                e_spotlight.Parameters["World"].SetValue(light.world);

                e_spotlight.Parameters["NORMAL"].SetValue(buffer.rt_normal);
                e_spotlight.Parameters["DEPTH"].SetValue(buffer.rt_depth);
                e_spotlight.Parameters["COOKIE"].SetValue(light.spot_info.cookie);
                e_spotlight.Parameters["SHADOW"].SetValue(light.spot_info.depth_map);

                e_spotlight.Parameters["LightViewProjection"].SetValue(light.spot_info.view * light.spot_info.projection);
                e_spotlight.Parameters["LightColor"].SetValue(light.color.ToVector4());
                e_spotlight.Parameters["LightPosition"].SetValue(light.position);
                e_spotlight.Parameters["LightDirection"].SetValue(light.spot_info.orientation.Forward);
                e_spotlight.Parameters["LightAngleCos"].SetValue(light.spot_info.angle_cos);
                e_spotlight.Parameters["LightClip"].SetValue(light.spot_info.far_clip);
                e_spotlight.Parameters["DepthBias"].SetValue(light.spot_info.bias);
                e_spotlight.Parameters["C"].SetValue(light.spot_info.C);

                e_spotlight.Parameters["Shadows"].SetValue(light.spot_info.shadows);

                graphics_device.SetVertexBuffer(Resources.GetModel("cone").Meshes[0].MeshParts[0].VertexBuffer);
                graphics_device.Indices = Resources.GetModel("cone").Meshes[0].MeshParts[0].IndexBuffer;

                float SL = Math.Abs(Vector3.Dot(Vector3.Normalize(light.position - camera.position), light.spot_info.orientation.Forward));

                if (SL <= (light.spot_info.angle_cos)) {
                    graphics_device.RasterizerState = RasterizerState.CullCounterClockwise;
                } else {
                    graphics_device.RasterizerState = RasterizerState.CullClockwise;
                }

                e_spotlight.CurrentTechnique.Passes[0].Apply();
                graphics_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, Resources.GetModel("cone").Meshes[0].MeshParts[0].VertexBuffer.VertexCount);


            } else if (light.type == LightType.POINT) {
                e_pointlight.Parameters["World"].SetValue(
                Matrix.CreateScale(light.point_info.radius) * Matrix.CreateTranslation(light.point_info.position));

                e_pointlight.Parameters["NORMAL"].SetValue(EngineState.buffer.rt_normal);
                e_pointlight.Parameters["DEPTH"].SetValue(EngineState.buffer.rt_depth);

                e_pointlight.Parameters["LightColor"].SetValue(light.color.ToVector4());
                e_pointlight.Parameters["LightPosition"].SetValue(light.position);
                e_pointlight.Parameters["LightIntensity"].SetValue(1f);
                e_pointlight.Parameters["LightRadius"].SetValue(light.point_info.radius);

                e_pointlight.Parameters["Shadows"].SetValue(false);
                e_pointlight.Parameters["quantized"].SetValue(light.point_info.quantize);

                EngineState.graphics_device.SetVertexBuffer(ContentHandler.resources["sphere"].value_gfx.Meshes[0].MeshParts[0].VertexBuffer);
                EngineState.graphics_device.Indices = ContentHandler.resources["sphere"].value_gfx.Meshes[0].MeshParts[0].IndexBuffer;

                Vector3 sdiff = (camera.position) - light.position;
                float skyCameraToLight = (float)Math.Sqrt((float)Vector3.Dot(sdiff, sdiff)) / 100.0f;

                if (skyCameraToLight <= light.point_info.radius) {
                    EngineState.graphics_device.RasterizerState = RasterizerState.CullClockwise;
                } else {
                    EngineState.graphics_device.RasterizerState = RasterizerState.CullCounterClockwise;
                }

                e_pointlight.CurrentTechnique.Passes[0].Apply();
                EngineState.graphics_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, ContentHandler.resources["sphere"].value_gfx.Meshes[0].MeshParts[0].VertexBuffer.VertexCount);
            }
        }
*/
    }
}

public static class Clock {
    public static GameTime game_time;
    public static double delta_time => game_time.ElapsedGameTime.TotalSeconds;
    public static double delta_time_ms => game_time.ElapsedGameTime.TotalMilliseconds;

    public static double update_thread_tick_rate = 60.0;
    public static TimeSpan update_thread_goal_time = new TimeSpan((long)(10000 * (1000.0/update_thread_tick_rate)));
    public static double update_thread_delta => 10000.0 * (1000.0 / update_thread_tick_rate);
    public static int frame_rate { get; set; } = 0;
    private static double _frame_rate_timer = 0;
    private static int _frame_counter = 0;
    
    public static int tick_rate { get; set; } = 0;
    private static double _tick_rate_timer = 0;
    private static int _tick_counter = 0;

    public static ulong frame_count = 0;
    public static ulong tick_count = 0;
    

    internal static void FrameRateUpdate(double milliseconds) {
        _frame_rate_timer += milliseconds;
        _frame_counter++;
    
        if (_frame_rate_timer >= 1000.0) {
            frame_rate = _frame_counter;
            _frame_rate_timer -= 1000.0;
            _frame_counter = 0;
        }

        frame_count++;
    }
    
    internal static void TickRateUpdate(double milliseconds) {
        _tick_rate_timer += milliseconds;
        _tick_counter++;
        tick_count++;
        if (_tick_rate_timer >= 1000.0) {
            tick_rate = _tick_counter;
            _tick_rate_timer -= 1000.0;
            _tick_counter = 0;
        }
    }
    
}