using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Raven.Console;
using Raven.Engine.Audio;
using Raven.Engine.Collision;
using Raven.Engine.Components;
using Raven.Engine.Controls;
using Raven.Graphics;
using Raven.Graphics.Drawing2D;
using Raven.Graphics.Drawing3D;
using Raven.Graphics.InterpolatedTypes;
using Raven.Engine.Entities;
using Raven.Graphics.Skybox;
using Raven.UI;

using SoundFlow.Abstracts;
using KeyboardInput = Microsoft.Xna.Framework.Input.KeyboardInput;

    
namespace Raven.Engine;

public enum EngineThread { Render, Update } 

public static class State {
    

    public static BindWatcher engine_binds;
    internal static (string bind, object[] bind_data)[] 
        engine_bind_list = [
            ("test", [Keys.T]),
            ("test_extra", [Keys.Z]),
            
            ("shift", [Keys.LeftShift, Keys.RightShift]),
            ("ctrl", [Keys.LeftControl, Keys.RightControl]),
            ("alt", [Keys.LeftAlt, Keys.RightAlt]),
            
            ("click", [MouseWatcher.MouseButtons.Left]),
            ("click_right", [MouseWatcher.MouseButtons.Right]),
            ("click_middle", [MouseWatcher.MouseButtons.Middle]),
            ("scroll_up", [MouseWatcher.MouseButtons.ScrollUp]),
            ("scroll_down", [MouseWatcher.MouseButtons.ScrollDown]),
            
            ("toggle_console", [Keys.OemTilde]),
            ("toggle_inspector", [Keys.F1]),
            ("toggle_full_info", [Keys.F3]),
            ("switch_buffer", [Keys.F4]),
            
            ("screenshot", [Keys.Insert]),
            
            ("mode_windowed", [Keys.F9]),
            ("mode_borderless", [Keys.F10]),
            ("mode_borderless_fullscreen", [Keys.F11]),
            ("mode_fullscreen", [Keys.F12]),
            
            ("exit", [Keys.Escape])
    ];
    

    public static Game game;
    public static ContentManager content_manager;
    public static GameWindow window;
    public static GraphicsDeviceManager graphics;
    public static GraphicsDevice graphics_device => graphics.GraphicsDevice;
    public static SpriteBatch spritebatch;

    public static UIWindowManager UI;
    
    public static Vector2i resolution => gvars.get_Vector2i("r_resolution");
    public static float super_res_scale => gvars.get_float("r_resolution_scale");
    
    public static Viewport viewport => graphics_device.Viewport;
    public static Scene CurrentScene => Scene.Manager.ActiveScene;
    public static Clock.UpdateThread scene_update_thread => Scene.Manager.update_thread;
    public static bool EnableInterpolation { get; set; } = true;
    
    public static bool is_active => game.IsActive;
    
    public static bool running = true;
    
    [Flags]
   public enum SceneUseState : byte {
        NONE = 0,
        UPDATE = 1 << 0,
        UPDATE_GRAPHICS = 1 << 1,
        RENDER = 1 << 2,
        STABILIZING = 1 << 3,
        MOVING = 1 << 4
   }

    internal static SceneUseState using_scene = 0;
    public static SceneUseState SceneBeingUsedBy => (SceneUseState)using_scene;
    internal static void SetSceneState(SceneUseState state) {
        using_scene = state;
    }
    
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
        
        graphics.GraphicsProfile = GraphicsProfile.HiDef;
        graphics.ApplyChanges();
        
        Threads.Initialize();
        
        game.Exiting += (sender, args) => {
            Threads.cancellation_token_source.Cancel();
            gvars.write_gvars_to_disk();
        };
        
        State.content_manager = content;
        State.graphics = graphics;
        State.window = window;
        State.spritebatch = new SpriteBatch(State.graphics_device);
        
        game.IsFixedTimeStep = false;
        game.InactiveSleepTime = TimeSpan.Zero;
        graphics.SynchronizeWithVerticalRetrace = false;
        window.IsBorderless = false;
        //graphics.ToggleFullScreen();
        window.AllowUserResizing = true;
        
        engine_binds = new BindWatcher(engine_bind_list);
        engine_binds.binds["mode_windowed"].JustPressed += () => {
            gvars.set("r_display_mode", "windowed");
            ChangeWindowMode();
        };
        engine_binds.binds["mode_borderless"].JustPressed += () => {
            gvars.set("r_display_mode", "borderless");
            ChangeWindowMode();
        };
        engine_binds.binds["mode_borderless_fullscreen"].JustPressed += () => {
            gvars.set("r_display_mode", "borderless_fullscreen");
            ChangeWindowMode();
        };
        engine_binds.binds["mode_fullscreen"].JustPressed += () => {
            gvars.set("r_display_mode", "fullscreen");
            ChangeWindowMode();
        };
        
        ConsoleInputRunner.build_using_list();
        
        gvars.add_gvar("g_tick_rate", gvar_data_type.INT, 60, true, "Sets the update thread's tick rate.");
        gvars.add_gvar("g_time_scale", gvar_data_type.FLOAT, 1f, false);
        
        gvars.add_gvar("r_resolution", gvar_data_type.VECTOR2I, FindCurrentResolution(), true, "Resolution of both the game window and output buffer.");
        gvars.add_gvar("r_resolution_scale", gvar_data_type.FLOAT, 1f, true, "Set the 3D render output buffer resolution scale.\n0.5 will half the resolution, making things pixelated, 2.0 will double the resolution.\nThis will not affect the 2D layer or backbuffer.\nGoing above 1.0 will not really do anything due to how deferred rendering works.\n");
        
        gvars.add_gvar("r_vsync", gvar_data_type.BOOL, true, true, "Sync vertical retrace to display.");
        gvars.add_gvar("r_frame_limit", gvar_data_type.INT, 180, true, "Sets the render thread's frame rate limit.");
        gvars.add_gvar("r_interpolation", gvar_data_type.BOOL, true, false);
        gvars.add_gvar("r_field_of_view", gvar_data_type.FLOAT, 110f, true, "Main camera field of view.");
        
        gvars.add_gvar("r_light_spot_resolution", gvar_data_type.INT, 1024, false);
        
        gvars.add_gvar("r_display_mode", gvar_data_type.STRING, "borderless_fullscreen", true, "Sets the window style. Options are:\nfullscreen, borderless_fullscreen [default], window, borderless");

        var wind_pos_comment = "Last used window position.\nMay be locked at 0x0 or -1x-1 on platforms where this is not supported (notably Wayland).";
        
        if (window.Position == Point.Zero) {
            gvars.add_gvar("g_window_position", gvar_data_type.VECTOR2I, -Vector2i.One, true, wind_pos_comment); 
        } else {
            gvars.add_gvar("g_window_position", gvar_data_type.VECTOR2I, Vector2i.Zero, true, wind_pos_comment);
        }
        
        gvars.add_gvar("i_bind_tap_time", gvar_data_type.INT, 150, true, "Sets the tap time for digital inputs, in milliseconds.\nThis is how long it takes for a key to go from Pressed to Held,\nand if it is released before then, it will become Tapped for one frame.");
        gvars.add_gvar("i_mouse_sensitivity", gvar_data_type.VECTOR2, Vector2.One, true, "Sets the mouse sensitivity individually for each axis.");

        gvars.add_gvar("ui_focus_follows_mouse", gvar_data_type.BOOL, false, true, "Forces UI window focus to always be on the window under the mouse\n(as opposed to standard click-to-focus)");
        gvars.add_gvar("ui_mouse_follows_focus", gvar_data_type.BOOL, false, false, "Moves the mouse over UI windows when they're opened or given focus");
        gvars.add_gvar("ui_window_shadows", gvar_data_type.BOOL, false, true, "Adds a small drop shadow to UI windows");
        
        bool read_gvars = gvars.read_gvars_from_disk();
        Debug.WriteLine($"{read_gvars} GVARS:\n{gvars.list_all()}");
        
        ChangeFrameLimit();
        ChangeTickRate();
        ChangeWindowMode();
        change_backbuffer_resolution();
        ChangeResolution(true);
        
        gvars.add_change_action("r_resolution", () => ChangeResolution());
        gvars.add_change_action("r_resolution_scale", () => ChangeResolution(false));
        
        gvars.add_change_action("r_vsync", () => ChangeVSync());
        gvars.add_change_action("r_frame_limit", () => ChangeFrameLimit());
        gvars.add_change_action("g_tick_rate", () => ChangeTickRate());

        gvars.add_change_action("r_interpolation", () => EnableInterpolation = gvars.get_bool("r_interpolation"));

        gvars.add_change_action("r_display_mode", () => ChangeWindowMode());
        
        engine_binds.force_enable("toggle_console");
        engine_binds.force_enable("toggle_inspector");
        engine_binds.force_enable("toggle_full_info");
        engine_binds.force_enable("screenshot");
        engine_binds.force_enable("exit");
        
        SoundFlowState.Initialize();
    }

    public static void Destroy() {
        SoundFlowState.Destroy();
    }

    public static void Load(ContentManager content) {
        Resources.LoadEngineContent(content);
        Resources.LoadContentList(content);

        quad_vb = new VertexBuffer(graphics_device, VertexPositionTexture.VertexDeclaration, 4, BufferUsage.None);
        quad_vb.SetData(vb_data);

        quad_ib = new IndexBuffer(graphics_device, IndexElementSize.ThirtyTwoBits, 6, BufferUsage.None);
        quad_ib.SetData(ib_data);

        var ctestpos = (Vector3.Backward) * 2f;
        var dir = Vector3.Normalize(-ctestpos);
        var lookat = Matrix.CreateLookAt(ctestpos, Vector3.Forward,
            Vector3.Up);

        e_clear = Resources.GetShader("r_clear");
        e_compositor = Resources.GetShader("r_compositor");
        e_directionallight = Resources.GetShader("r_directional_light");
        e_exp_light_depth = Resources.GetShader("r_exp_light_depth");
        e_gbuffer = Resources.GetShader("r_fill_gbuffer");
        e_light_depth = Resources.GetShader("r_light_depth");
        e_pointlight = Resources.GetShader("r_point_light");
        e_skybox = Resources.GetShader("r_skybox");
        e_spotlight = Resources.GetShader("r_spot_light");

        Draw2D.load();
        Draw3D.load();

        SkyboxState.skybox_t.PrivateCreateSkyboxFromCrossImage(out SkyboxState.skybox_data, out SkyboxState.skybox_indices, 1, 0, 1, 2,
            3, 5, 4);
        SkyboxState.skybox_t.Subdivide(SkyboxState.skybox_data, SkyboxState.skybox_indices, out SkyboxState.skybox_data,
            out SkyboxState.skybox_indices, 16, MathHelper.Pi);
        SkyboxState.skybox_cm = new RenderTarget2D(graphics_device, SkyboxState.skybox_face_res * 4, SkyboxState.skybox_face_res * 3,
            false, SurfaceFormat.Rgba64, DepthFormat.Depth16);
        SkyboxState.skybox_cm_e = new RenderTarget2D(graphics_device, SkyboxState.skybox_face_res * 4, SkyboxState.skybox_face_res * 3,
            false, SurfaceFormat.Rgba64, DepthFormat.Depth16);

        SkyboxState.sun_moon.time_stopped = true;
        SkyboxState.sun_moon.set_time_of_day(0.5);

        graphics_device.SetRenderTarget(SkyboxState.skybox_cm);
        graphics_device.Clear(SkyboxState.sun_moon.atmosphere_color);

        graphics_device.SetRenderTarget(null);
       
        wait_for_init = true;
    }

    private static double time_of_last_window_size_change = 0;
    private static bool changing_window_size = false;
    
    public static void LoadFinished() {
        window.ClientSizeChanged += (sender, args) => {
            changing_window_size = true;
            time_of_last_window_size_change = Clock.game_time.TotalGameTime.TotalMilliseconds;
        };
        
        Scene.Manager.StartUpdateThread();
    }

    private static Vector2i FindCurrentResolution() {
        var cdm = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
        return new Vector2i(cdm.Width, cdm.Height);
    }
    private static Vector2i FindMonitorTopLeft() {
        return Vector2i.Zero;
    }

    public static string ListAdapters() {
        
        string mstr = "[Graphics Adapters]\n";
        var c = 0;
        
        foreach (var ga in GraphicsAdapter.Adapters) {
            
            mstr += $"[{c}] {ga.Description}\n";
            c++;
        }

        return mstr + "\n";
    }

    private static void ChangeFrameLimit() {
        if (gvars.get_int("r_frame_limit") == -1) {
            game.IsFixedTimeStep = false;
            game.TargetElapsedTime = new TimeSpan((long)((1000.0 / 60.0) * 10000.0));
        } else {
            game.IsFixedTimeStep = true;
            game.TargetElapsedTime = new TimeSpan((long)((1000.0 / (double)gvars.get_int("r_frame_limit")) * 10000.0));
        }
    }

    private static void ChangeVSync() {
        graphics.SynchronizeWithVerticalRetrace = gvars.get_bool("r_vsync");
        graphics.ApplyChanges();
    }

    private static void ChangeWindowMode() {
        var wm = gvars.get_string("r_display_mode");
        switch (wm) {
            case "fullscreen":
                gvars.set("r_resolution", FindCurrentResolution());
                graphics.IsFullScreen = true;
                window.IsBorderless = false;
                window.AllowUserResizing = false;
                break;
                
            case "borderless_fullscreen":
                gvars.set("r_resolution", FindCurrentResolution());
                graphics.IsFullScreen = false;
                window.IsBorderless = true;
                window.AllowUserResizing = false;
                break;
                
            case "window":
                graphics.IsFullScreen = false;
                window.IsBorderless = false;
                window.AllowUserResizing = true;
                break;
                
            case "borderless":
                graphics.IsFullScreen = false;
                window.IsBorderless = true;
                window.AllowUserResizing = true;    
                break;
                
            default:
                break;
        }
    }
    
    private static void ChangeTickRate() {
        var i = gvars.get_int("g_tick_rate");
        if (i < 1) {
            gvars.set("g_tick_rate", 1);
            return;
        } else {
            if (Scene.Manager.update_thread != null) Scene.Manager.update_thread.tick_rate = i;
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

    public static void screenshot() {
        foreach (GBuffer gb in GBuffer.Manager.gbuffers.Values) {
            if (gb.DrawToScreen && gb.screen_draw_info.fullscreen) {
                gb.TakeScreenshot();
            }
        }
    }

    internal static bool wait_for_init = false;


    public static void UpdateGraphics(GameTime gt) {
        Clock.game_time = gt;
        //Update all graphics stuff
        engine_binds.Update();
        UIWindowManager.Manager.update_all_UIs();
        SkyboxState.sun_moon.update();
        
        //Change game resolution to match window resolution
        if (Clock.game_time.TotalGameTime.TotalMilliseconds - time_of_last_window_size_change > 500 && changing_window_size) {
            gvars.set("r_resolution", window.ClientBounds.Size.ToVector2i());
            changing_window_size = false;
            time_of_last_window_size_change = 0;
        }
    }

    public static void UpdateEnd() {
        engine_binds.UpdateEnd();
    }
    
    private static bool currently_rendering;
    public static bool CurrentlyRendering => currently_rendering; 
    
    public static void Render() {
        while (true) {
            var current = using_scene;
            if (current == SceneUseState.STABILIZING) continue;
            
            if (Interlocked.CompareExchange(ref using_scene,SceneUseState.RENDER,current) == current) {
                break;
            } 
        }
        
        IAutoInterpolate.Manager.UpdateRenderThread(Clock.render_delta_time_ms);
        
        //fuck it, clear ALL gbuffers
        //probably worthwhile to implement a small system to allow keeping targets
        //TODO add a system to allow targets to skip being cleared
        GBuffer.Manager.ClearAll2DLayers();
        
        //update entity graphics within the current scene
        //Interlocked.Exchange(ref using_scene, (int)SceneUseState.UPDATE_GRAPHICS);
        Scene.Manager.UpdateGraphics();
        //Interlocked.Exchange(ref using_scene, (int)SceneUseState.NONE);
        
        UIWindowManager.Manager.render_all_UI_window_internals();
        
        //Interlocked.Exchange(ref using_scene, (int)SceneUseState.RENDER);
        //render out any cameras that have a GBuffer specified 
        Camera.Manager.BuildAllCameraGBuffers();
        //Interlocked.Exchange(ref using_scene, (int)SceneUseState.NONE);

        //draw all GBuffers which have draw_to_screen enabled, using their screen_draw_info
        GBuffer.Manager.DrawAllScreenBuffers();
        Interlocked.Exchange(ref using_scene, SceneUseState.NONE);
        
    }

    public static string engine_info() {
        string buffer_text = "";
        switch (State.draw_debug_buffer) {
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

        return $"[Render] {Clock.frame_rate} FPS{buffer_text}\n[Update] {Clock.tick_rate} TPS" +
               $"{(EnableInterpolation ? " (interpolated)," : ",")} ~{scene_update_thread.delta_ms_before_sleep:0.000}ms non-sleep" +
               $" ({Clock.total_tick_ms_last_update:0.000}/{scene_update_thread.goal_time:0.000}ms)\n" +
               //$"[Audio] ~{SoundFlowState.update_loop.last_ticks} Ticks per loop (always slightly over)/{SoundFlowState.update_loop.TPS} TPS ({Stopwatch.Frequency / 1000.0} Stopwatch ticks/ms)\n" +
               $"[Threads] {Threads.TaskCount}/{Threads.MaxTasks}\n";// +
              // $"[Draw Calls] {graphics_device.Metrics.DrawCount}\n[Sprites] {graphics_device.Metrics.SpriteCount}\n";
    }
}

public static class Clock {
    public static GameTime game_time;
    private static Stopwatch game_run_time_stopwatch = Stopwatch.StartNew();
    public static double game_run_time_ms => game_run_time_stopwatch.ElapsedMilliseconds;
    public static long game_run_time_ticks => game_run_time_stopwatch.ElapsedTicks;
    
    public static double render_delta_time => game_time.ElapsedGameTime.TotalSeconds;
    public static float render_delta_time_f => (float)game_time.ElapsedGameTime.TotalSeconds;
    public static double render_delta_time_ms => game_time.ElapsedGameTime.TotalMilliseconds;
    public static float render_delta_time_ms_f => (float)game_time.ElapsedGameTime.TotalMilliseconds;

    internal static double total_tick_ms_last_update = 0;

    public static double update_delta_time => Scene.Manager.update_thread.delta_s;
    public static float update_delta_time_f => (float)Scene.Manager.update_thread.delta_s;
    public static double update_delta_time_ms => Scene.Manager.update_thread.delta_ms;
    public static float update_delta_time_ms_f => (float)Scene.Manager.update_thread.delta_ms;
    
    public static double frame_rate { get; set; } = 0;
    private static double _frame_rate_timer = 0;
    private static double _frame_counter = 0;
    
    public static double tick_rate { get; set; } = 0;
    private static double _tick_rate_timer = 0;
    private static double _tick_counter = 0;
    
    public static ulong frame_count = 0;
    public static ulong tick_count = 0;

    public static double delta (EngineThread thread) {
        switch (thread) {
            case EngineThread.Render: return render_delta_time;
            case EngineThread.Update: return update_delta_time;
            default: throw new ArgumentOutOfRangeException(nameof(thread), thread, null);
        }
    }
    public static double delta_ms (EngineThread thread) {
        switch (thread) {
            case EngineThread.Render: return render_delta_time_ms;
            case EngineThread.Update: return update_delta_time_ms;
            default: throw new ArgumentOutOfRangeException(nameof(thread), thread, null);
        }
    }
    public static float delta_f (EngineThread thread) {
        switch (thread) {
            case EngineThread.Render: return render_delta_time_f;
            case EngineThread.Update: return update_delta_time_f;
            default: throw new ArgumentOutOfRangeException(nameof(thread), thread, null);
        }
    }
    public static float delta_ms_f (EngineThread thread) {
        switch (thread) {
            case EngineThread.Render: return render_delta_time_ms_f;
            case EngineThread.Update: return update_delta_time_ms_f;
            default: throw new ArgumentOutOfRangeException(nameof(thread), thread, null);
        }
    }
    
    public static float time_scale => gvars.get_float("g_time_scale");
    
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

    public class TickRateWatcher {
        private double poll_rate_timer = 0;
        private double poll_counter = 0;
        private double ticks_per_second;
        public double TicksPerSecond => ticks_per_second;

        public double read_time = 1000.0;
        
        public TickRateWatcher() {}
        
        public void PollRateUpdate(double milliseconds) {
            poll_rate_timer += milliseconds;
            poll_counter++;
            if (poll_rate_timer >= read_time) {
                ticks_per_second = poll_counter;
                poll_rate_timer -= read_time;
                poll_counter = 0;
            }
        }
    }
    
    
    public class UpdateThread {
        private string name = "";
        
        public Action update_action { get; set; }
        public Action post_update_action { get; set; }
        
        public double tick_rate { get; set; } = 60.0;
        public double goal_time => 1000.0 / tick_rate;
        
        private double _delta_ms_actual = 0.0;
        private double _delta_ms_before_sleep = 0.0;
        public double delta_ms_before_sleep => _delta_ms_before_sleep;

        public bool fixed_timestep { get; set; } = false;

        public double delta_ms => (!fixed_timestep ? _delta_ms_actual : (goal_time)) * Clock.time_scale;
        public double delta_s => (!fixed_timestep ? _delta_ms_actual / 1000 : (goal_time / 1000)) * Clock.time_scale;
        
        public UpdateThread(string name, Action update_action, Action post_update_action = null) {
            this.name = name;
            this.update_action = update_action;
            this.post_update_action = post_update_action;
        }
        
        public void Start() {
            Threads.StartTask($"Update{(name.Length > 0 ? " (" + name + ")" : "")}", Update);
        }

        readonly Stopwatch loop_stopwatch = Stopwatch.StartNew();
        
        private void Update() {
            long frame_start = 0;
            
            double elapsed_ms() => (loop_stopwatch.ElapsedTicks - frame_start) * 1000.0 / (double)Stopwatch.Frequency;
            double remaining_ms() => (goal_time - elapsed_ms());
            
            while (!State.wait_for_init) ;
            while (!Threads.IsCancellationRequested) {
                frame_start = loop_stopwatch.ElapsedTicks;

                //Interlocked.Exchange(ref State.using_scene, (int)State.SceneUseState.UPDATE);
                if (update_action != null) update_action();
                if (post_update_action != null) post_update_action();
                //Interlocked.Exchange(ref State.using_scene, (int)State.SceneUseState.NONE);
                
                _delta_ms_before_sleep = elapsed_ms();
                
                if (remaining_ms() > 1.0) {
                    Thread.Sleep((int)(remaining_ms() - 0.2));    
                }
                while (remaining_ms() > 0.0 && !Threads.IsCancellationRequested) {
                    //Thread.SpinWait(1);
                }
                
                total_tick_ms_last_update = elapsed_ms();
                TickRateUpdate(total_tick_ms_last_update);
                _delta_ms_actual = elapsed_ms();
            }
        }
    }
}