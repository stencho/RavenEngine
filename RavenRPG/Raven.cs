using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RavenRPG.Engine;
using RavenRPG.Engine.Controls;
using RavenRPG.Engine.World;
using RavenRPG.Graphics.Drawing2D;


namespace RavenRPG;

public class Raven : Game {
    private GraphicsDeviceManager _graphics;

    public Raven() {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize() {
        State.Initialize(this, Content, _graphics, Window);
        State.Draw_2D = () => //Draw2D.image("trumpmap", Input.mouse_position - (Vector2i.One * 100) , Vector2i.One * 200);
        {
            Draw2D.fill_circle(State.main_thread_input.mouse_position, 5f, Color.White);
            Draw2D.circle(State.main_thread_input.mouse_position, 5f, 2f, Color.Black);
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