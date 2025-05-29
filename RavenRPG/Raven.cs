using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RavenRPG.Engine;

namespace RavenRPG;

public class Raven : Game {
    private GraphicsDeviceManager _graphics;

    public Raven() {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize() {
        State.Initialize(Content, _graphics, Window);
        base.Initialize();
    }

    protected override void LoadContent() {
        Resources.LoadContentList(Content);
        // TODO: use this.Content to load your game content here
    }

    private ulong c = 0;
    protected override void Update(GameTime gameTime) {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape)) {
            gvars.write_gvars_to_disk();
            Exit();
        }

        Window.Title = c.ToString();
        c++;
        // TODO: Add your update logic here

        base.Update(gameTime);
    }

    ~Raven() {
        gvars.write_gvars_to_disk();
    }
    
    protected override void Draw(GameTime gameTime) {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        
        State.Compose();

        base.Draw(gameTime);
    }
}