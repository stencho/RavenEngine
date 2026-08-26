using System;
using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Framework.Utilities;
using Raven.Engine;
using Raven.Engine.Scene3D;
using Raven.Graphics.Drawing2D;
using Raven.UI;
using WaterTrans.GlyphLoader;

namespace RavenEdit;

public class RavenEditGame : Game {
    private GraphicsDeviceManager _graphics;
    private bool windows = true;

    private Typeface test_typeface;
    
    public static FullResolutionRenderTarget output_render_target;

    Canvas current_canvas;
    
    public RavenEditGame() {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        
        windows = OperatingSystem.IsWindows();
    }

    protected override void Initialize() {
        State.Initialize(this, Content, _graphics, Window);
        base.Initialize();
    }

    protected override void LoadContent() {
        State.Load(Content);
        
        Interface.Load();
        
        string fontPath = System.IO.Path.Combine(Environment.CurrentDirectory, "Content/BitstromWeraNerdFontMono-Regular.ttf");

        using (var fontStream = System.IO.File.OpenRead(fontPath)) {
            test_typeface = new Typeface(fontStream);
        }
        
        output_render_target = new FullResolutionRenderTarget();
        
        current_canvas = new Canvas();
        
        State.LoadFinishedNoUpdateThread();
    }

    protected override void Update(GameTime gameTime) {
        if (State.engine_binds.double_tapped("exit")) {
            Exit();
        }

        State.UpdateGraphics(gameTime);
        Interface.Update();
        State.UpdateEnd();
        
        base.Update(gameTime);
        
        Clock.TickRateUpdate(gameTime.ElapsedGameTime.TotalMilliseconds);
    }
    
    protected override void Draw(GameTime gameTime) {
        State.Render();
        
        // draw canvas and interface to their respective full resolution render targets
        current_canvas.Draw();
        Interface.Render();
        
        // compose layers
        State.graphics_device.SetRenderTarget(output_render_target.rt2D);
        
        Draw2D.image(current_canvas.render_target.rt2D, Vector2i.Zero, State.resolution);
        Draw2D.image(Interface.render_target.rt2D, Vector2i.Zero, State.resolution);
        
        // draw output to screen
        State.graphics_device.SetRenderTarget(null);
        Draw2D.image(output_render_target.rt2D, Vector2i.Zero, State.resolution);
        
        //update framerate counter
        Clock.FrameRateUpdate(gameTime.ElapsedGameTime.TotalMilliseconds);

        base.Draw(gameTime);
    }
    
    protected override void UnloadContent() {
        State.Destroy();
        base.UnloadContent();
    }
}