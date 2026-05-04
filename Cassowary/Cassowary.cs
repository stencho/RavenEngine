using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Raven;
using Raven.Console;
using Raven.Engine;
using Raven.Engine.Audio;
using Raven.Engine.Audio.Generators;
using Raven.Engine.Components;
using Raven.Engine.Controls;
using Raven.Engine.Entities;
using Raven.Engine.Scene3D;
using Raven.Graphics;
using Raven.Graphics.Drawing2D;
using Raven.Graphics.Drawing3D;
using Raven.Graphics.Geometry2D;
using Raven.Graphics.InterpolatedTypes;
using Raven.Graphics.Skybox;
using Raven.UI;
using Raven.UI.Forms;
using Raven.UI.Forms.LayoutStrips;
using SoundFlow.Components;

namespace Cassowary;

//TODO thoguths lol
/*
    Try to get IK going and add a cool little guy what runs around creepily on two legs + a tail also w/ IK
  
 */

public class CassowaryGame : Game {
    private GraphicsDeviceManager _graphics;

    public static Scene scene;
    
    public static FreeCamEntity free_cam;
    
    private static float skull_rotate = 0f;
    private static light skull_lamp;

    public static bool show_all_debug_info = false;

    private Sine sine;
    
    UIWindow inspector;
    
    private LerpedMatrix l_mat = new LerpedMatrix(Matrix.Identity * Matrix.CreateFromAxisAngle(Vector3.UnitX, float.DegreesToRadians(-90)),
        Matrix.Identity * Matrix.CreateFromAxisAngle(Vector3.UnitX, float.DegreesToRadians(90)), 2000,
        InterpolationType.Bounce, EngineThread.Render);
    
    public CassowaryGame() {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        
        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;
        
        IsMouseVisible = true;
        Window.AllowUserResizing = true;
    }

    protected override void Initialize() {
        State.Initialize(this, Content, _graphics, Window);
        
        gvars.set("g_time_scale", 0f);
        
        base.Initialize();
    }

    protected override void LoadContent() {
        State.Load(Content);
        
        scene = new BasicScene();
        Scene.SetActiveScene(scene);
        
        skull_lamp = new light {
            type = LightType.SPOT,
            color = Color.Red,
            spot_info = new spot_info()
        };
        
        //test_ent = new TestEntity();
        //test_ent2 = new TestEntity();
        free_cam = new FreeCamEntity();
        
        //test_ent.Components.GetComponent<RenderModel>("RenderModel").Texture
        //universe.SpawnEntity(test_ent, Vector3ui128.Zero, Vector3.Zero);
        //universe.SpawnEntity(test_ent2, Vector3ui128.Right, Vector3.Right * 5);
        State.CurrentScene.Spawn(free_cam, Vector3.Zero);

        var cam = free_cam.Components.GetFirst<GBufferCamera>().camera;
        cam.use_gvar_field_of_view = true;
        cam.gbuffer.enable_screen_draw_fullscreen(-1);
        State.UI = new UIWindowManager(cam.gbuffer);
        
        for (int i = 0; i < 10; i++) {
            var pos = (Vector3.UnitX * (RNG.rng_float_neg_one_to_one * 50)) + (Vector3.UnitZ * (RNG.rng_float_neg_one_to_one * 50));
            var ent = new TestEntity(pos);
            
            State.CurrentScene.Spawn(ent);
        }
        
        //SkyboxState.sun_moon.set_time_of_day(0.9f);
        Vector2i pointer_tip = (Vector2i.One * 5) + (Vector2i.Right * 5);
        
        cam.gbuffer.Draw2DOverGame += (DrawShapesToSurface draw_shapes) => {
            //StaticControlBinds.draw_state(600, 0, 100, 10, 10);
            var dayper = SkyboxState.sun_moon.current_time_entire_day_percent;
            bool afternoon = dayper > 0.5f;
            var hour = afternoon ? ((dayper - 0.5f) * 2) * 12f : (dayper * 2f) * 12f;
            if ((int)hour == 0) hour = 12;

            var debug_str = "";
            debug_str += State.engine_info();
            
            if (show_all_debug_info) {
                debug_str += $"[SceneType] {State.CurrentScene.scene_type}\n\n";
                debug_str += $"\n[GVars]\n{gvars.list_all()}\n\n[Loaded Assets]\n{Resources.ListAllContent()}\n";
                Draw2D.text_shadow($"{Camera.Manager.ListAllCameras}\n{GBuffer.Manager.ListAllBuffers}\n{ManagedRT2D.Manager.ListAllBuffers}\n[Windows] {State.UI.list_windows()}\n{Renderer.VisibilityString}\n{State.CurrentScene.VisibilityString}",
                (Vector2i.UnitX * 250) + (Vector2i.UnitY * 100), Color.White, Color.Black);
                debug_str += State.ListAdapters();
                debug_str += State.engine_binds.Mouse.state_info();
                debug_str += State.engine_binds.Keyboard.state_info();
                debug_str += State.engine_binds.state_info();
                
            }
            Draw2D.text_shadow(debug_str, Vector2i.One * 4, Color.White, Color.Black);

            Draw2D.image(SkyboxState.sun_moon.lerps.debug_band, Vector2i.Down * 24 + (Vector2i.Right * (State.resolution.X - SkyboxState.sun_moon.lerps.debug_band.Bounds.Size.X)),
                SkyboxState.sun_moon.lerps.debug_band.Bounds.Size.ToVector2i() + (Vector2i.UnitY * 10));
            var tl = (Vector2i.Down * 24) + (Vector2i.Right * (State.resolution.X - SkyboxState.sun_moon.lerps.debug_band.Bounds.Size.X)) +
                                            (SkyboxState.sun_moon.lerps.debug_band.Bounds.Size.ToVector2i() * (float)dayper);
            Draw2D.line(tl, tl + (Vector2i.UnitY * 11), Color.Red, 1f);
            Draw2D.text_shadow($"[Environment] {(int)hour} O'clock", Vector2i.Down * 4 + (Vector2i.Right * (State.resolution.X - SkyboxState.sun_moon.lerps.debug_band.Bounds.Size.X)), Color.White, Color.Black);
            
            //Draw2D.fill_circle(new Vector2(200 + (50 * (sine.Phase / (MathF.PI * 2))), 10), 6f, Color.IndianRed);
            //Draw2D.fill_circle(new Vector2(200 + (50 * ), 18), 6f, Color.DarkOliveGreen);
            //Draw2D.fill_circle(new Vector2(200 + (50 * ), 24), 6f, Color.MediumPurple);
            
            //Draw2D.image(cursor.white_fill, Vector2i.One * 50, Vector2i.One * 100, 0f);
            //Draw2D.image(cursor.signed_distance_negative, Vector2i.One * 50, Vector2i.One * 100, 0f);
            //Draw2D.rect(Vector2i.One * 50, Vector2i.One * 32 + 50, Color.Black, 1f);
            
            //Draw2D.rect((Vector2i.One * 100), (Vector2i.One * 100) + shapes.get("cursor").bordered_bounds.size, Color.Black, 1f);
            
            //test_shape.render(Vector2i.One * 50);
            
            //Draw2D.rect((Vector2i.One * 100)+ shapes.get("cursor").shape_top_left_within_border, (Vector2i.One * 100)  + shapes.get("cursor").shape_top_left_within_border + shapes.get("cursor").bounds.size, Color.Black, 1f);
            Draw2D.end();
            
        };
        
        //Oscillator test = new Oscillator(SoundFlowState.Engine, SoundFlowState.PlaybackDevice.Format) { Frequency = 220, Type = Oscillator.WaveformType.Sine};
        
        
        inspector = new UIWindow(new Vector2i(0, State.resolution.Y - 700), new Vector2i(400, 320));
        //inspector.hide();
        /*
        var b = new UIButton(5, 5, "fart really hard");


        var c = new UIButton(b.bottom_right.X + 2, 5, "fart even harder");

        c.set_action(() => Log.log("yee"));
        inspector.add_subform(c);
        inspector.add_subform(b);

        var d = new UIButton(5, 5, "adam sander");

        var test_panel = new Panel(Vector2i.One * 25, Vector2i.One * 350);
        var test_panel_2 = new Panel(Vector2i.One * 25, Vector2i.One * 250);

        b.set_action(() => test_panel.toggle_visibility());
        d.set_action(() => test_panel_2.toggle_visibility());

        test_panel.add_subform(d);
        test_panel.add_subform(test_panel_2);

        test_panel_2.foreground_draw = p => {
            Draw2D.image(Resources.GetTexture("adam"), Vector2i.Zero, p.client_size);
            Draw2D.text(p.root_client_size.ToXString(), Vector2i.One * 5, Color.Black);
        };
        test_panel.hide();
        test_panel_2.hide();

        inspector.add_subform(test_panel);

        */

        LayoutManager lm = new LayoutManager(inspector);
        
        lm.add_strip(new UIButton(0, 0, "test button"));
        lm.add_strip(new UIButton(0,0, "test button"), new UIButton(0,0, "test button"));
        lm.add_strip(new UIButton(0,0, "test button"), new UIButton(0,0, "test button"), new UIButton(0,0, "test button"));
        
        inspector.add_subform(lm);
        
        State.UI.add_window(inspector);
        
        State.LoadFinished();
    }

    ~CassowaryGame() {
        State.Destroy();
    }

    protected override void Update(GameTime gameTime) {
        if (State.engine_binds.double_tapped("exit")) {
            Exit();
        }
        
        State.UpdateGraphics(gameTime);
        
        skull_rotate += 0.001f;
        if (skull_rotate > MathF.PI * 2) {
            skull_rotate -= MathF.PI * 2;
        }
        
        var time = SkyboxState.sun_moon.current_time_entire_day_percent;
        
        if (State.engine_binds.double_tapped("test")) {
            SkyboxState.sun_moon.set_time_of_day(0.5f);    
            
        } else if (State.engine_binds.pressed("test")) {
            if (State.engine_binds.just_pressed("scroll_up")) {
                time += 0.005;
                if (time > 1.0) time -= 1.0;
                SkyboxState.sun_moon.set_time_of_day(time);    
            }
            if (State.engine_binds.just_pressed("scroll_down")) {
                time -= 0.005;
                if (time < 0) time += 1.0;
                SkyboxState.sun_moon.set_time_of_day(time);    
            }
        }
        
        if (State.engine_binds.just_pressed("toggle_inspector")) {
            State.UI.toggle_window(inspector);
        }
        
        if (State.engine_binds.just_pressed("test_extra")) {
            Threads.Request(new Threads.ThreadRequestPacket(() => Log.log("fart")));
        }
        
        if (State.engine_binds.just_pressed("screenshot")) {
            State.screenshot();
        }
        
        if (State.engine_binds.just_pressed("switch_buffer")) {
            State.draw_debug_buffer += 1;
            if (State.draw_debug_buffer > 3) State.draw_debug_buffer = -1;
        }

        if (State.engine_binds.just_pressed("toggle_full_info")) {
            show_all_debug_info = !show_all_debug_info;
        }
        
        State.UpdateEnd();
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime) {
        //GraphicsDevice.Clear(Color.CornflowerBlue);
        
        State.Render();
        
        Clock.FrameRateUpdate(gameTime.ElapsedGameTime.TotalMilliseconds);
        base.Draw(gameTime);
    }
}