using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Raven.Engine;

namespace Raven.Graphics.Effects.Forward;

public partial class DrawModelForward : ManagedEffect {
    private Matrix _world;
    private Vector3 _position;
    
    private string shader_name = "draw_forward";
    
}