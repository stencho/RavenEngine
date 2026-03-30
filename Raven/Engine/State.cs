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
using Raven.UI;

using SoundFlow.Abstracts;
using KeyboardInput = Microsoft.Xna.Framework.Input.KeyboardInput;

    
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
            ("screenshot", [Keys.F2]),
            ("toggle_full_info", [Keys.F3]),
            ("switch_buffer", [Keys.F4]),
            
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
    
    public static Vector2i resolution => gvars.get_Vector2i("resolution");
    public static float super_res_scale => gvars.get_float("super_resolution_scale");
    
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

    
    //TODO finish implementing display mode toggle hotkeys (F10,11,12)
    
    //TODO clean up gvar names and add categorization prefixes to default engine gvars
    // e_ for engine, g_ for graphics, i_ for input? 
    // also gvars.add_newline(); might be handy depending on how big the config gets
    
    //TODO modify window manager to take mouse input (and keyboard? should be doable through Watcher classes) through functions, to make it possible to instantiate them and
    // do like DOOM3 style in-game computer screen what you can click
    
    //TODO add current scene state saving/loading system (even just some JSON trash idk)
    // needs to have universal structure for Entity objects and their components, but variable layout depending on scene type for the ScenePositionInfo and general structure
    
    //TODO implement loading, reloading, and creating objects via CS-scripts. 
    // maybe each entity can get a .cs file of which a blank is autogenerated and saved to disk by the engine before being opened for editing
    
    //TODO add saving/loading binds based on a template and locked file layout a la how gvars do it 
    
    //TODO implement proper handling for keeping windows positioned on screen when the window size changes
    // it kinda happens if you interact with a window but it should probably also happen when the game size changes
    
    //TODO fix high freq audio loop now that I actually understand what's goin on
    //TODO function queue for the high freq audio loop
    // instead of doing multiple things per loop, do one thing from the top of a queue to try to keep performance as lockstep as possible
    // (ALAP)
    
    //TODO build new broad/narrow solvers for collision detection and resolution
    // Start with a broad phase solver which runs on a thread that constantly checks a ConcurrentQueue for entities to test for collisions,
    // this solver will be unique to each scene. It will run a method in the Scene object that adds Entities to a second ConcurrentQueue,
    // for the narrow phase to solve. The narrow phase will then call a callback method with information about the collisions or lack thereof.
    // Narrow phase also needs to be able to add objects back into itself if their movement is extreme enough that it changes their octree positions
    // In this engine, I want support for high speed projectiles, so swept collisions are used, and narrow phase will be multi-step
    // if there is a collision with an object, the object will be given the choice to either EPA directly out, EPA out then move tangential to
    // the impact normal by the distance that it penetrated the wall, or, if the entity requires it, run custom collision code 
    
    //TODO turn Raven into a basic scene editor with the ability to save and load 
    // use a combination of brush-based objects and imported mesh static objects
    
    //TODO build a UI development tool
    
    //TODO add basic physical interactions; impulse and transfer of force, angular momentum, basic friction (complex friction?? I like threading), etc. more fancy source-like stuff would be v nice
    // not like a spring is particularly hard to understand, but making 4 of them work on the same mass at the same time is uhhhh spooky
    // least I got a fixed deterministic timestep, but still like how da fuck does babby integrate
     
    
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
        
        ConsoleInputRunner.build_using_list();
        
        gvars.add_gvar("c_tick_rate", gvar_data_type.INT, 60, true, "Sets the update thread's tick rate.");
        gvars.add_gvar("c_timescale", gvar_data_type.FLOAT, 1f, false);
        
        gvars.add_gvar("resolution", gvar_data_type.VECTOR2I, FindCurrentResolution(), true, "Resolution of both the game window and output buffer.");
        gvars.add_gvar("super_resolution_scale", gvar_data_type.FLOAT, 1f, true, "Set the 3D render output buffer resolution scale.\n0.5 will half the resolution, making things pixelated, 2.0 will double the resolution and enable supersampling\nThis will not affect the 2D layer, which will always be at the main buffer resolution.");
        gvars.add_gvar("vsync", gvar_data_type.BOOL, true, true, "Sync vertical retrace to display.");
        gvars.add_gvar("frame_limit", gvar_data_type.INT, 180, true, "Sets the render thread's frame rate limit.");
        gvars.add_gvar("interpolation", gvar_data_type.BOOL, true, false);
        
        gvars.add_gvar("light_spot_resolution", gvar_data_type.INT, 1024, false);
        
        gvars.add_gvar("display_mode", gvar_data_type.STRING, "borderless_fullscreen", true, "Sets the window style. Options are:\nfullscreen, borderless_fullscreen [default], window, borderless");

        var wind_pos_comment = "Last used window position.\nMay be locked at 0x0 or -1x-1 on platforms where this is not supported (notably Wayland).";
        
        if (window.Position == Point.Zero) {
            gvars.add_gvar("window_position", gvar_data_type.VECTOR2I, -Vector2i.One, true, wind_pos_comment); 
        } else {
            gvars.add_gvar("window_position", gvar_data_type.VECTOR2I, Vector2i.Zero, true, wind_pos_comment);
        }
        
        gvars.add_gvar("bind_tap_time", gvar_data_type.INT, 150, true, "Sets the tap time for digital inputs, in milliseconds.\nThis is how long it takes for a key to go from Pressed to Held,\nand if it is released before then, it will become Tapped for one frame.");
        gvars.add_gvar("mouse_sensitivity", gvar_data_type.VECTOR2, Vector2.One, true, "Sets the mouse sensitivity individually for each axis.");
        
        bool read_gvars = gvars.read_gvars_from_disk();
        Debug.WriteLine($"{read_gvars} GVARS:\n{gvars.list_all()}");
        
        ChangeFrameLimit();
        ChangeTickRate();
        ChangeWindowMode();
        change_backbuffer_resolution();
        ChangeResolution(true);
        
        gvars.add_change_action("resolution", () => ChangeResolution());
        
        gvars.add_change_action("vsync", () => ChangeVSync());
        gvars.add_change_action("frame_limit", () => ChangeFrameLimit());
        gvars.add_change_action("c_tick_rate", () => ChangeTickRate());

        gvars.add_change_action("interpolation", () => EnableInterpolation = gvars.get_bool("interpolation"));

        gvars.add_change_action("display_mode", () => ChangeWindowMode());
        
        engine_binds.force_enable("toggle_console");
        engine_binds.force_enable("toggle_inspector");
        engine_binds.force_enable("toggle_full_info");
        engine_binds.force_enable("screenshot");
        
        SoundFlowState.Initialize();
    }

    public static void Destroy() {
        SoundFlowState.Destroy();
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

        e_gbuffer = Resources.GetShader("fill_gbuffer");
        e_light_depth = Resources.GetShader("light_depth");
        e_exp_light_depth = Resources.GetShader("exp_light_depth");
        e_skybox = Resources.GetShader("skybox");
        e_compositor = Resources.GetShader("compositor");
        e_clear = Resources.GetShader("clear");
        e_directionallight = Resources.GetShader("directionallight");
        e_spotlight = Resources.GetShader("spotlight");
        e_pointlight = Resources.GetShader("pointlight");

        Draw2D.load();
        Draw3D.load();

        UI = new UIWindowManager();

        Skybox.skybox_t.PrivateCreateSkyboxFromCrossImage(out Skybox.skybox_data, out Skybox.skybox_indices, 1, 0, 1, 2,
            3, 5, 4);
        Skybox.skybox_t.Subdivide(Skybox.skybox_data, Skybox.skybox_indices, out Skybox.skybox_data,
            out Skybox.skybox_indices, 16, MathHelper.Pi);
        Skybox.skybox_cm = new RenderTarget2D(graphics_device, Skybox.skybox_face_res * 4, Skybox.skybox_face_res * 3,
            false, SurfaceFormat.Rgba64, DepthFormat.Depth16);
        Skybox.skybox_cm_e = new RenderTarget2D(graphics_device, Skybox.skybox_face_res * 4, Skybox.skybox_face_res * 3,
            false, SurfaceFormat.Rgba64, DepthFormat.Depth16);

        Skybox.sun_moon.time_stopped = true;
        Skybox.sun_moon.set_time_of_day(0.5);

        graphics_device.SetRenderTarget(Skybox.skybox_cm);
        graphics_device.Clear(Skybox.sun_moon.atmosphere_color);

        graphics_device.SetRenderTarget(null);
       
        //camera.enable_gbuffer(1280,720);
        wait_for_init = true;
    }

    private static double time_of_last_window_size_change = 0;
    private static bool changing_window_size = false;
    
    public static void LoadFinished() {
        window.ClientSizeChanged += (sender, args) => {
            var g = false;
            
            if (window.ClientBounds.Size.X < 640) {
                graphics.PreferredBackBufferWidth = 640;
                g = true;
            }

            if (window.ClientBounds.Size.Y < 480) {
                graphics.PreferredBackBufferHeight = 480;
                g = true;
            }

            if (g) {
                graphics.ApplyChanges();
            }
            
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
        //var cdm = ;
        //return new Vector2i(cdm.Width, cdm.Height);
        //var dcount = SDL.SDL_GetNumVideoDisplays();
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
        if (gvars.get_int("frame_limit") == -1) {
            game.IsFixedTimeStep = false;
            game.TargetElapsedTime = new TimeSpan((long)((1000.0 / 60.0) * 10000.0));
        } else {
            game.IsFixedTimeStep = true;
            game.TargetElapsedTime = new TimeSpan((long)((1000.0 / (double)gvars.get_int("frame_limit")) * 10000.0));
        }
    }

    private static void ChangeVSync() {
        graphics.SynchronizeWithVerticalRetrace = gvars.get_bool("vsync");
        graphics.ApplyChanges();
    }

    private static void ChangeWindowMode() {
        var wm = gvars.get_string("window_mode");
        switch (wm) {
            case "fullscreen":
                gvars.set("resolution", FindCurrentResolution());
                graphics.IsFullScreen = true;
                window.IsBorderless = false;
                window.AllowUserResizing = false;
                break;
                
            case "borderless_fullscreen":
                gvars.set("resolution", FindCurrentResolution());
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
        var i = gvars.get_int("c_tick_rate");
        if (i < 1) {
            gvars.set("c_tick_rate", 1);
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
        engine_binds.Update();
        UI.update();
        Skybox.sun_moon.update();
        
        if (Clock.game_time.TotalGameTime.TotalMilliseconds - time_of_last_window_size_change > 500 && changing_window_size) {
            gvars.set("resolution", window.ClientBounds.Size.ToVector2i());
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
        
        IAutoInterpolate.Manager.UpdateRenderThread(Clock.delta_time_ms);
        
        //fuck it, clear ALL gbuffers
        //probably worthwhile to implement a small system to allow keeping targets
        //TODO add a system to allow targets to skip being cleared
        GBuffer.Manager.ClearAll2DLayers();
        
        //update entity graphics within the current scene
        //Interlocked.Exchange(ref using_scene, (int)SceneUseState.UPDATE_GRAPHICS);
        Scene.Manager.UpdateGraphics();
        //Interlocked.Exchange(ref using_scene, (int)SceneUseState.NONE);
        
        UI.render_window_internals();
        
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

        return $"[Render] {Clock.frame_rate} FPS{buffer_text}\n[Update] {Clock.tick_rate} TPS{(EnableInterpolation ? " (interpolated)," : ",")} ~{scene_update_thread.delta_ms_before_sleep:0.000}ms non-sleep ({Clock.total_tick_ms_last_update:0.000}/{scene_update_thread.goal_time:0.000}ms)\n[Audio] ~{SoundFlowState.update_loop.last_ticks} Ticks per loop (always slightly over)/{SoundFlowState.update_loop.TPS} TPS ({Stopwatch.Frequency / 1000.0} Stopwatch ticks/ms)\n[Threads] {Threads.TaskCount}/{Threads.MaxTasks}\n";
    }
}

public static class Clock {
    public static GameTime game_time;
    private static Stopwatch game_run_time_stopwatch = Stopwatch.StartNew();
    public static double game_run_time_ms => game_run_time_stopwatch.ElapsedMilliseconds;
    public static long game_run_time_ticks => game_run_time_stopwatch.ElapsedTicks;
    
    public static double delta_time => game_time.ElapsedGameTime.TotalSeconds;
    public static double delta_time_ms => game_time.ElapsedGameTime.TotalMilliseconds;

    public static double total_tick_ms_last_update = 0;
    
    public static double frame_rate { get; set; } = 0;
    private static double _frame_rate_timer = 0;
    private static double _frame_counter = 0;
    
    public static double tick_rate { get; set; } = 0;
    private static double _tick_rate_timer = 0;
    private static double _tick_counter = 0;
    
    public static ulong frame_count = 0;
    public static ulong tick_count = 0;

    public static float time_scale => gvars.get_float("c_timescale");
    
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
                    Thread.SpinWait(1);
                }
                
                Clock.total_tick_ms_last_update = elapsed_ms();
                Clock.TickRateUpdate(total_tick_ms_last_update);
                _delta_ms_actual = elapsed_ms();
            }
        }
    }
}