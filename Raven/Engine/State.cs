using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Raven.Engine.Universes;
using Raven.Console;
using Raven.Engine.Components;
using Raven.Engine.Controls;
using Raven.Graphics;
using Raven.Graphics.Drawing2D;
using Raven.Graphics.Drawing3D;
using Raven.RPG.Entities;
using Raven.UI;

namespace Raven.Engine;
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


        public void configure_dlight_shader(Camera camera, GBuffer gbuffer, Effect e_directionallight) {

            //e_directionallight.Parameters["fog"].SetValue(true);
            //e_directionallight.Parameters["fog_start"].SetValue(0.5f);

            //e_directionallight.Parameters["camera_pos"].SetValue(State.camera.position);
            //e_directionallight.Parameters["FarClip"].SetValue(State.camera.far_clip);

            e_directionallight.Parameters["NORMAL"].SetValue(gbuffer.rt_normal);
            //e_directionallight.Parameters["DEPTH"].SetValue(State.buffer.rt_depth);

            e_directionallight.Parameters["InverseView"].SetValue(Matrix.Invert(camera.view));

            e_directionallight.Parameters["AtmosphereColor"].SetValue(lerps.get_color_at((float)Skybox.sun_moon.current_day_value).ToVector3());
            e_directionallight.Parameters["AtmosphereIntensity"].SetValue(0.5f);

            e_directionallight.Parameters["LightColor"].SetValue(lerps.get_color_at((float)Skybox.sun_moon.current_day_value).ToVector3());
            e_directionallight.Parameters["LightIntensity"].SetValue(1f);

            e_directionallight.Parameters["LightDirection"].SetValue(sun_direction);
            //e_directionallight.Parameters["camera_pos"].SetValue(State.camera.position);

            e_directionallight.CurrentTechnique.Passes[0].Apply();
            graphics_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, quad_vb.VertexCount);           

        }

    }

    internal static (bind_type type, object bind_type_type, object bind_data, string bind)[] 
        engine_bind_list = [
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
            (bind_type.digital, controller_type.keyboard, Keys.Z, "test_extra"),
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
            
            (bind_type.digital, controller_type.keyboard, Keys.F, "t_supp"),
            (bind_type.digital, controller_type.keyboard, Keys.D1, "speenL"),
            (bind_type.digital, controller_type.keyboard, Keys.D3, "speenR"),
            (bind_type.digital, controller_type.keyboard, Keys.R, "t_S"),
            (bind_type.digital, controller_type.keyboard, Keys.Q, "t_L"),
            (bind_type.digital, controller_type.keyboard, Keys.E, "t_R"),

            (bind_type.digital, controller_type.keyboard, Keys.OemTilde, "toggle_console"),
            (bind_type.digital, controller_type.keyboard, Keys.F3, "toggle_full_info"),
            (bind_type.digital, controller_type.keyboard, Keys.F4, "switch_buffer")
    ];
    
    public static Game game;
    public static ContentManager content_manager;
    public static GameWindow window;
    public static GraphicsDeviceManager graphics;
    public static GraphicsDevice graphics_device => graphics.GraphicsDevice;
    public static SpriteBatch spritebatch;

    public static UIWindowManager UI;
    
    public static Vector2i resolution => gvars.get_Vector2i("resolution");
    public static float super_res_scale => gvars.get_float("super_resolution_scale");

    //public static GBuffer gbuffer;
    //public static Camera camera;

    public static Model test_skull; 
    
    public static Viewport viewport => graphics_device.Viewport;
    
    public static Universe universe = new();

    public static Action Draw_2D;
    public static Action Draw_3D;
    
    public static bool is_active => game.IsActive;
    public static bool show_all_debug_info = false;
    
    public static bool running = true;
    
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

    public static ControlBinds engine_binds = new ControlBinds(engine_bind_list);
    public static Input input_main_thread => engine_binds.input;
    
    internal static VertexPositionTexture[] vb_data = {
        new VertexPositionTexture(Vector3.Up + Vector3.Left,      Vector2.Zero),
        new VertexPositionTexture(Vector3.Up + Vector3.Right,     Vector2.UnitX),
        new VertexPositionTexture(Vector3.Down + Vector3.Left,    Vector2.UnitY),
        new VertexPositionTexture(Vector3.Down + Vector3.Right,   Vector2.One)
    };
    internal static int[] ib_data = { 0, 1, 2, 1, 3, 2 };
    
    public static void Initialize(Game game, ContentManager content, GraphicsDeviceManager graphics, GameWindow window) {
        State.game = game;
        
        Threads.Initialize();
        
        game.Exiting += (sender, args) => {
            Threads.cancellation_token_source.Cancel();
            gvars.write_gvars_to_disk();
        };
        
        State.content_manager = content;
        State.graphics = graphics;
        State.window =  window;
        State.spritebatch = new SpriteBatch(State.graphics_device);

        game.IsFixedTimeStep = true;
        game.InactiveSleepTime = TimeSpan.Zero;
        graphics.SynchronizeWithVerticalRetrace = false;
        window.IsBorderless = false;
        
        window.AllowUserResizing = false;
        
        ConsoleInputRunner.build_using_list();
        
        gvars.add_gvar("resolution", gvar_data_type.VECTOR2I, FindDefaultResolution(), true);
        gvars.add_gvar("super_resolution_scale", gvar_data_type.FLOAT, 1f, true);
        gvars.add_gvar("frame_limit", gvar_data_type.INT, 180, true);
        gvars.add_gvar("vsync", gvar_data_type.BOOL, true, true);
        gvars.add_gvar("light_spot_resolution", gvar_data_type.INT, 1024, false);
        
        change_backbuffer_resolution();
        
        ChangeResolution(true);
        
        gvars.add_change_action("resolution", () => ChangeResolution());
        gvars.add_change_action("super_resolution_scale", () => ChangeResolution());
        gvars.add_change_action("vsync", () => { graphics.SynchronizeWithVerticalRetrace = gvars.get_bool("vsync"); });
        gvars.add_change_action("frame_limit", () => ChangeFrameLimit());
        ChangeFrameLimit();
            
        bool read_gvars = gvars.read_gvars_from_disk();
        Debug.WriteLine($"{read_gvars} GVARS:\n{gvars.list_all()}");
                
        engine_binds.force_enable("toggle_console");
        engine_binds.force_enable("screenshot");
        
        universe.StartUpdating();
    }

    public static TestEntity test_ent;
    public static FreeCamEntity free_cam;
    
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
        
        e_gbuffer = Resources.GetShader("fill_gbuffer");
        e_light_depth = Resources.GetShader("light_depth");
        e_exp_light_depth = Resources.GetShader("exp_light_depth");
        e_skybox = Resources.GetShader("skybox");
        e_compositor = Resources.GetShader("compositor");
        e_clear = Resources.GetShader("clear");
        e_directionallight = Resources.GetShader("directionallight");
        e_spotlight = Resources.GetShader("spotlight");
        e_pointlight = Resources.GetShader("pointlight");
        
        test_skull = Resources.GetModel("fourth");
        
        Draw2D.load();
        Draw3D.load();
        
        UI = new UIWindowManager();
        
        Skybox.skybox_t.PrivateCreateSkyboxFromCrossImage(out Skybox.skybox_data, out Skybox.skybox_indices, 1, 0, 1, 2, 3, 5, 4);
        Skybox.skybox_t.Subdivide(Skybox.skybox_data, Skybox.skybox_indices, out Skybox.skybox_data, out Skybox.skybox_indices, 16, MathHelper.Pi);
        Skybox.skybox_cm = new RenderTarget2D(graphics_device, Skybox.skybox_face_res * 4, Skybox.skybox_face_res * 3, false, SurfaceFormat.Rgba64, DepthFormat.Depth16);
        Skybox.skybox_cm_e = new RenderTarget2D(graphics_device, Skybox.skybox_face_res * 4, Skybox.skybox_face_res * 3, false, SurfaceFormat.Rgba64, DepthFormat.Depth16);

        Skybox.sun_moon.time_stopped = true;
        Skybox.sun_moon.set_time_of_day(0.5);
        
        graphics_device.SetRenderTarget(Skybox.skybox_cm);
        graphics_device.Clear(Skybox.sun_moon.atmosphere_color);

        graphics_device.SetRenderTarget(null);

        skull_lamp = new light {
            type = LightType.SPOT,
            color = Color.Red,
            spot_info = new spot_info()
        };
        
        test_ent = new TestEntity();
        free_cam = new FreeCamEntity();
        
        //test_ent.Components.GetComponent<RenderModel>("RenderModel").Texture
        universe.SpawnEntity(test_ent, Vector3ui128.Zero, Vector3.Zero);
        universe.SpawnEntity(free_cam, Vector3ui128.Zero, Vector3.Zero);

        var cam = free_cam.Components.Get<GBufferCamera>("Camera").camera;
        
        cam.gbuffer.EnableScreenDrawFullscreen(-1);
        
        cam.gbuffer.Draw3DLayer = () => {
            //drawing the world should go here
            Draw3D.draw_buffers_diffuse_texture(cam, cam.gbuffer,
                State.test_skull.Meshes[0].MeshParts[0].VertexBuffer,
                State.test_skull.Meshes[0].MeshParts[0].IndexBuffer,
                Resources.GetTexture("XboxenDiffuse"), Color.White,
                Matrix.CreateRotationY(0f) * Matrix.CreateScale(0.5f) * Matrix.CreateTranslation(Vector3.Down * 2));

        };
        
        cam.gbuffer.Draw2DLayer =
            () => {
                graphics_device.SetRenderTarget(cam.gbuffer.rt_2D);
                graphics_device.BlendState = BlendState.AlphaBlend;
                graphics_device.Clear(Color.Transparent);
                
                //StaticControlBinds.draw_state(600, 0, 100, 10, 10);
                var dayper = Skybox.sun_moon.current_time_entire_day_percent;
                bool afternoon = dayper > 0.5f;
                var hour = afternoon ? ((dayper - 0.5f) * 2) * 12f : (dayper * 2f) * 12f;
                if ((int)hour == 0) hour = 12;

                string buffer_text = "";
                switch (draw_debug_buffer) {
                    case 0:
                        buffer_text = " <diffuse>";
                        break;
                    case 1:
                        buffer_text = " <normals>";
                        break;
                    case 2:
                        buffer_text = " <depth>";
                        break;
                    case 3:
                        buffer_text = " <lighting>";
                        break;
                    default:
                        buffer_text = "";
                        break;
                }

                var debug_str = "";
                debug_str += $"[Render] {Clock.frame_rate} FPS{buffer_text}\n[Update] {Clock.tick_rate} Ticks/s ({Clock.total_ms_last_update:0.000}/{Clock.update_thread_goal_time.TotalMilliseconds:0.000})\n";
                if (show_all_debug_info) {
                    debug_str += $"\n[GVars]\n{gvars.list_all()}\n\n[Loaded Assets]\n{Resources.ListAllContent()}\n";
                }
                debug_str += $"\n[Environment] {(int)hour} O'clock\n\n[Threads] {Threads.TaskCount}/{Threads.MaxTasks}\n{Threads.list_all_active_threads}\n{universe.universe_info}\n{Camera.Manager.ListAllCameras}\n{ManagedRT2D.Manager.ListAllBuffers}\n{GBuffer.Manager.ListAllBuffers}\n[Windows] {UI.list_windows()}\n";


                Draw2D.text_shadow(debug_str, Vector2i.One * 4, Color.White, Color.Black);

                //Draw2D.image(Resources.GetTexture("Missing"), Vector2i.One * 200, Vector2i.One * 50);
                Draw2D.image(Skybox.sun_moon.lerps.debug_band, Vector2i.Down * 5 + (Vector2i.Right * 250),
                    Skybox.sun_moon.lerps.debug_band.Bounds.Size.ToVector2i() + (Vector2i.UnitY * 10));
                var tl = (Vector2i.Down * 5) + (Vector2i.Right * 250) +
                         (Skybox.sun_moon.lerps.debug_band.Bounds.Size.ToVector2i() * (float)dayper);
                Draw2D.line(tl, tl + (Vector2i.UnitY * 11), Color.Red, 1f);
                UI.draw();

                if (Draw_2D != null) Draw_2D.Invoke();

                universe.DrawChunkMapAroundEntity(free_cam, new Vector2i(resolution.X - 250, 0), Vector2i.One * 250);
            };
        //camera.enable_gbuffer(1280,720);
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

        GBuffer.Manager.UpdateFullscreenBufferResolutions();
    }


    
    public static void Update(GameTime gt) {
        //Threads.Prune();
        Clock.game_time = gt;
        engine_binds.Update();
        
        UI.update();
        
        Skybox.sun_moon.update();

        
        if (engine_binds.tapped("test")) {
            Skybox.sun_moon.time_stopped = !Skybox.sun_moon.time_stopped;
        }

        if (engine_binds.pressed("test")) {
            var newtime = Skybox.sun_moon.current_time_entire_day_percent;
            if (engine_binds.just_pressed("scroll_up")) {
                newtime += 0.005;
                if (newtime > 1.0) newtime -= 1.0;
                Skybox.sun_moon.set_time_of_day(newtime);    
            }
            if (engine_binds.just_pressed("scroll_down")) {
                newtime -= 0.005;
                if (newtime < 0) newtime += 1.0;
                Skybox.sun_moon.set_time_of_day(newtime);    
            }
            
        }
        
        if (engine_binds.just_pressed("test_extra")) {
            Threads.Request(new Threads.ThreadRequestPacket(() => Log.log("fart")));
        }
        
        
        Camera.Manager.UpdateAllCameras();
        //skull_lamp.spot_info.orientation = camera.orientation * (Matrix.CreateRotationY(MathHelper.ToRadians(-8f)));
    }

    private static float skull_rotate = 0f;
    private static light skull_lamp;
    
    public static void Render() {
        universe.UpdateGraphics();
        
        UI.render_window_internals();
        
        Camera.Manager.BuildAllCameraGBuffers();
        GBuffer.Manager.DrawAllScreenBuffers();
    }
    
}

public static class Clock {
    public static GameTime game_time;
    public static double delta_time => game_time.ElapsedGameTime.TotalSeconds;
    public static double delta_time_ms => game_time.ElapsedGameTime.TotalMilliseconds;

    private static double _update_tick_rate = 60.0;
    public static double update_thread_tick_rate {
        get {
            return _update_tick_rate;
        }
        set {
            _update_tick_rate = value;
            update_thread_goal_time = new TimeSpan((long)(10000 * (1000.0/update_thread_tick_rate)));
        }
    }

    public static TimeSpan update_thread_goal_time = new TimeSpan((long)(10000 * (1000.0/update_thread_tick_rate)));
    public static double update_thread_delta => (1000.0 / update_thread_tick_rate);
    
    public static double frame_rate { get; set; } = 0;
    private static double _frame_rate_timer = 0;
    private static double _frame_counter = 0;
    
    public static double tick_rate { get; set; } = 0;
    private static double _tick_rate_timer = 0;
    private static double _tick_counter = 0;

    public static ulong frame_count = 0;
    public static ulong tick_count = 0;
    

    public static void FrameRateUpdate(double milliseconds) {
        _frame_rate_timer += milliseconds;
        _frame_counter++;
    
        if (_frame_rate_timer >= 1000.0) {
            frame_rate = _frame_counter;
            _frame_rate_timer -= 1000.0;
            _frame_counter = 0;
        }

        frame_count++;
    }

    public static double total_ms_last_update = 0;
    
    public static void TickRateUpdate(double milliseconds) {
        _tick_rate_timer += milliseconds;
        _tick_counter++;
        tick_count++;
        if (_tick_rate_timer >= 1000.0) {
            tick_rate = _tick_counter;
            _tick_rate_timer -= 1000.0;
            _tick_counter = 0;
        }
    }
    
    
    public class UpdateThread {
        private string name = "";
        
        public Action update_action { get; set; }
        
        public ControlBinds binds = new ControlBinds(State.engine_bind_list);
        public Input input_update_thread => binds.input;

        internal bool currently_updating = false;
        public bool CurrentlyUpdating => currently_updating;
        
        public UpdateThread(string name, Action update_action) {
            this.name = name;
            this.update_action = update_action;
        }
        
        public void Start() {
            Threads.StartTask($"Update{(name.Length > 0 ? " (" + name + ")" : "")}", Update);
        }
    
        Stopwatch loop_stopwatch = Stopwatch.StartNew();
        
        private void Update() {
            while (!Threads.IsCancellationRequested) {
                //currently_updating = true;

                long frame_start = loop_stopwatch.ElapsedTicks;

                //var start_dt = DateTime.Now;

                //UPDATE 
                binds.Update();

                if (update_action != null) update_action();

                if (binds.just_pressed("switch_buffer")) {
                    State.draw_debug_buffer += 1;
                    if (State.draw_debug_buffer > 3) State.draw_debug_buffer = -1;
                }

                if (binds.just_pressed("toggle_full_info")) {
                    State.show_all_debug_info = !State.show_all_debug_info;
                }
                
                //currently_updating = false;
                State.universe.StabilizeChunkPositions();


                double elapsed_ms() {
                    return (loop_stopwatch.ElapsedTicks - frame_start) * 1000.0 / (double)Stopwatch.Frequency;
                }

                double remaining_ms() {
                    return (Clock.update_thread_goal_time.TotalMilliseconds - elapsed_ms()) * 1000.0 / (double)Stopwatch.Frequency;
                }

                if (remaining_ms() > 1.0) {
                    Thread.Sleep((int)(remaining_ms() - 1));    
                }
                while (remaining_ms() > 0.0 && !Threads.IsCancellationRequested) {
                    Thread.SpinWait(1);
                }
                
                Clock.total_ms_last_update = elapsed_ms();
                Clock.TickRateUpdate(total_ms_last_update);
            }
        }
    }
}