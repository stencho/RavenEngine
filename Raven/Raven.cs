using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Raven.Engine.Controls;
using Raven.Engine.Universes;
using Raven.Engine;
using Raven.Graphics.Drawing2D;


namespace Raven;

public class RavenGame : Game {
    private GraphicsDeviceManager _graphics;

    public RavenGame() {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        
        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;
        
        IsMouseVisible = true;
        Window.AllowUserResizing = true;
        
    }

    protected override void Initialize() {
        State.Initialize(this, Content, _graphics, Window);
        State.Draw_2D = () => 
        {
            Draw2D.fill_circle(State.input_main_thread.mouse_position, 5f, Color.White);
            Draw2D.circle(State.input_main_thread.mouse_position, 5f, 2f, Color.Black);
        };
        base.Initialize();
    }

    protected override void LoadContent() {
        State.Load(Content);
    }
    
    protected override void Update(GameTime gameTime) {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape)) {
            Exit();
        }
        
        State.Update(gameTime);
        
        base.Update(gameTime);
    }
    
    protected override void Draw(GameTime gameTime) {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        
        State.Render();
        
        Clock.FrameRateUpdate(gameTime.ElapsedGameTime.TotalMilliseconds);
        base.Draw(gameTime);
    }
}