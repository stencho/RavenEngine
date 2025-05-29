using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using RavenRPG.Renderer;
using RavenRPG.Renderer.Drawing;

namespace RavenRPG.Engine;
public static class State {
    public static ContentManager content_manager;
    public static GameWindow window;
    public static GraphicsDeviceManager graphics;
    public static GraphicsDevice graphics_device => graphics.GraphicsDevice;
    public static SpriteBatch sprite_batch;
    public static Camera camera;
    
    public static Vector2i resolution => gvars.get_Vector2i("resolution");

    public static GBuffer buffer;
    
    public static void Initialize(ContentManager content,  GraphicsDeviceManager graphics, GameWindow window) {
        State.content_manager = content;
        State.graphics = graphics;
        State.window =  window;
        State.sprite_batch = new SpriteBatch(State.graphics_device);
        
        gvars.add_gvar("resolution", gvar_data_type.VECTOR2I, FindDefaultResolution(), true);

        buffer = new GBuffer();
        buffer.CreateInPlace(graphics_device, resolution.X, resolution.Y);
        change_backbuffer_resolution();
        
        ChangeResolution();
        
        gvars.add_change_action("resolution", ChangeResolution);
        
        bool read_gvars = gvars.read_gvars_from_disk();
        Debug.WriteLine($"{read_gvars} GVARS:\n{gvars.list_all()}");
    }

    public static void Load() {
        Draw2D.load(graphics_device, graphics, content_manager, resolution);
    }
    
    private static Vector2i FindDefaultResolution() {
        var cdm = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
        return new Vector2i(cdm.Width, cdm.Height);
    }

    static void change_backbuffer_resolution() {
        graphics.PreferredBackBufferWidth = resolution.X;
        graphics.PreferredBackBufferHeight = resolution.Y;
        graphics.ApplyChanges();
    }
    public static void ChangeResolution() {
        graphics.PreferredBackBufferWidth = resolution.X;
        graphics.PreferredBackBufferHeight = resolution.Y;
        
        graphics.ApplyChanges();
        
        buffer.change_resolution(graphics_device, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
    }

    public static void Compose() {
        graphics_device.SetRenderTargets(buffer.buffer_targets);
        
        
        graphics_device.SetRenderTarget(buffer.rt_2D);
        graphics_device.Clear(Color.Red);
        
        Draw2D.image("trumpmap", Vector2i.Zero, Vector2i.One * 200);
        
        graphics_device.SetRenderTarget(null);
        
        Draw2D.image(buffer.rt_2D, Vector2i.Zero, resolution);
    }
}