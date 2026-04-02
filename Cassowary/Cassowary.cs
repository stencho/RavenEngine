using System;
using Cassowary.UI;
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
using Raven.Graphics.InterpolatedTypes;
using Raven.UI;
using Raven.UI.Forms;
using SoundFlow.Components;

namespace Cassowary;

//TODO thoguths lol
/*
    Try to get IK going and add a cool little guy what runs around creepily on two legs + a tail also w/ IK
  
 */

public class CassowaryGame : Game {
    private GraphicsDeviceManager _graphics;

    public static Scene scene;
    
    public static Model test_box; 
    public static Model test_skull; 
    
    public static TestEntity test_ent;
    public static TestEntity test_ent2;
    public static FreeCamEntity free_cam;
    
    private static float skull_rotate = 0f;
    private static light skull_lamp;

    public static bool show_all_debug_info = true;

    private Sine sine;
    
    InspectorWindow inspector;
    
    private LerpedMatrix l_mat = new LerpedMatrix(Matrix.Identity * Matrix.CreateFromAxisAngle(Vector3.UnitX, float.DegreesToRadians(-90)),
        Matrix.Identity * Matrix.CreateFromAxisAngle(Vector3.UnitX, float.DegreesToRadians(90)), 2000,
        InterpolationType.Bounce, InterpolationThread.Render);
    
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
        
        cam.gbuffer.enable_screen_draw_fullscreen(-1);
        cam.gbuffer.draw_UI_to_this_buffer();
        
        for (int i = 0; i < 50; i++) {
            var pos = (Vector3.UnitX * (RNG.rng_float_neg_one_to_one * 500)) + (Vector3.UnitZ * (RNG.rng_float_neg_one_to_one * 500));
            var ent = new TestEntity(pos);
            
            State.CurrentScene.Spawn(ent);
            
        }
        
        cam.gbuffer.Draw3DLayer = () => {
            //drawing the world should go here
            Draw3D.draw_buffers_diffuse_texture(cam, cam.gbuffer,
                test_box.Meshes[0].MeshParts[0].VertexBuffer,
                test_box.Meshes[0].MeshParts[0].IndexBuffer,
                Resources.GetTexture("XboxenDiffuse"), Color.White,
                Matrix.CreateScale(2f) * Matrix.CreateRotationY(skull_rotate) *
                Matrix.CreateTranslation((Vector3.Down * 5f) + Vector3.Forward * 5f));
            Draw3D.draw_buffers_diffuse_texture(cam, cam.gbuffer,
                test_skull.Meshes[0].MeshParts[0].VertexBuffer,
                test_skull.Meshes[0].MeshParts[0].IndexBuffer,
                Resources.GetTexture("texture_1001"), Color.White,
                Matrix.CreateScale(2f) * l_mat.tween_value *
                Matrix.CreateTranslation((Vector3.Down * 1f) + Vector3.Forward * 5f));
        };
        
        cam.gbuffer.Draw2DLayer = () => {
            //StaticControlBinds.draw_state(600, 0, 100, 10, 10);
            var dayper = State.Skybox.sun_moon.current_time_entire_day_percent;
            bool afternoon = dayper > 0.5f;
            var hour = afternoon ? ((dayper - 0.5f) * 2) * 12f : (dayper * 2f) * 12f;
            if ((int)hour == 0) hour = 12;

            var debug_str = "";
            debug_str += State.engine_info();
            debug_str += $"[Freecam] {free_cam.position.XYZ.ToXString()} {free_cam.position.XYZ.ToXString()} \n";
            
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

            Draw2D.image(State.Skybox.sun_moon.lerps.debug_band, Vector2i.Down * 24 + (Vector2i.Right * (State.resolution.X - State.Skybox.sun_moon.lerps.debug_band.Bounds.Size.X)),
                State.Skybox.sun_moon.lerps.debug_band.Bounds.Size.ToVector2i() + (Vector2i.UnitY * 10));
            var tl = (Vector2i.Down * 24) + (Vector2i.Right * (State.resolution.X - State.Skybox.sun_moon.lerps.debug_band.Bounds.Size.X)) +
                                            (State.Skybox.sun_moon.lerps.debug_band.Bounds.Size.ToVector2i() * (float)dayper);
            Draw2D.line(tl, tl + (Vector2i.UnitY * 11), Color.Red, 1f);
            Draw2D.text_shadow($"[Environment] {(int)hour} O'clock", Vector2i.Down * 4 + (Vector2i.Right * (State.resolution.X - State.Skybox.sun_moon.lerps.debug_band.Bounds.Size.X)), Color.White, Color.Black);
            
            Draw2D.line(new Vector2(100, 10), new Vector2(150, 10), Color.BurlyWood, 1f);
            
            //Draw2D.fill_circle(new Vector2(200 + (50 * (sine.Phase / (MathF.PI * 2))), 10), 6f, Color.IndianRed);
            //Draw2D.fill_circle(new Vector2(200 + (50 * ), 18), 6f, Color.DarkOliveGreen);
            //Draw2D.fill_circle(new Vector2(200 + (50 * ), 24), 6f, Color.MediumPurple);
        };
        
        test_box = Resources.GetModel("fourth");
        test_skull = Resources.GetModel("skull");

        //Oscillator test = new Oscillator(SoundFlowState.Engine, SoundFlowState.PlaybackDevice.Format) { Frequency = 220, Type = Oscillator.WaveformType.Sine};
        
        
        inspector = new InspectorWindow(new Vector2i(0, State.resolution.Y - 700), new Vector2i(400, 320));
        inspector.hide();
        
        var b = new UIButton(5, 5, "fart really hard");
        b.set_action(() => Log.log("ye"));
        
        var c = new UIButton(55, 15, "fart really hard");
        c.set_action(() => Log.log("yee"));
        inspector.add_subform(c);
        inspector.add_subform(b);

        var d = new UIButton(5, 15, "kabuki be like");
        d.set_action(() => Log.log("yooooo *klonk*"));
        

        var test_panel = new Panel(Vector2i.One * 25, Vector2i.One * 350);
        var test_panel_2 = new Panel(Vector2i.One * 25, Vector2i.One * 250);
        
        test_panel.add_subform(d);
        test_panel.add_subform(test_panel_2);

        test_panel_2.foreground_draw = p => {
            Draw2D.image(Resources.GetTexture("adam"), Vector2i.Zero, p.client_size);
        };

        inspector.add_subform(test_panel);
        
        State.UI.add_window(inspector);
        
        var tester = new UIWindow(new Vector2i(20,20), new Vector2i(400, 320));

        var subtest = new UIButton(5, 5, "mmm im a subby lil button :3c");
        subtest.set_action(() => Log.log("ahn!!"));
        var subtest2 = new UIButton(5, 55, "im a subby lil button too hehe");
        subtest2.set_action(() => Log.log("uhnghn!!"));
        var panel = new TabbedPanel(Vector2i.Zero, tester.client_size);
        
        
        panel.add_subform(subtest);
        panel.add_subform(subtest2);
        tester.add_subform(panel);
        
        //tester.internal_draw_action = () => {
        //};
        
        State.UI.add_window(tester);
        
        
        //SoundFlowState.Master.AddComponent(test);
        //test.Enabled = true;
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
        
        var time = State.Skybox.sun_moon.current_time_entire_day_percent;
        
        if (State.engine_binds.double_tapped("test")) {
            State.Skybox.sun_moon.set_time_of_day(0.5f);    
            
        } else if (State.engine_binds.pressed("test")) {
            if (State.engine_binds.just_pressed("scroll_up")) {
                time += 0.005;
                if (time > 1.0) time -= 1.0;
                State.Skybox.sun_moon.set_time_of_day(time);    
            }
            if (State.engine_binds.just_pressed("scroll_down")) {
                time -= 0.005;
                if (time < 0) time += 1.0;
                State.Skybox.sun_moon.set_time_of_day(time);    
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