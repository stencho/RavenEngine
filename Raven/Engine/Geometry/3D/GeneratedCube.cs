using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Raven.Engine;
using Raven.Engine.Collision;
using Raven.Engine.Collision.Shapes3D;
using Raven.Engine.Geometry3D;
using Raven.Graphics.Effects;

namespace Raven.Engine.Geometry3D;

public class GeneratdCube {
    public Cube shape => _collision as Cube;
    
    private Shape3D _collision;
    public Shape3D collision => _collision;
    

    private RenderPackage _render_info;
    public RenderPackage render_info => _render_info;
}