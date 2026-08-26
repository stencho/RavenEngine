using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Raven.Engine;
using Raven.Engine.Collision;
using Raven.Engine.Collision.Shapes3D;
using Raven.Engine.Geometry3D;
using Raven.Graphics.Effects;

namespace Raven.Engine.Geometry3D;

public class GeneratedQuad : GeneratedGeometry {
    public Quad shape => _collision as Quad;
    
    private Shape3D _collision;
    public override Shape3D collision => _collision;
    
    private RenderPackage _render_info = new RenderPackage();
    public override RenderPackage render_info => _render_info;
    
    public GeneratedQuad(Vector3 A, Vector3 B, Vector3 C, Vector3 D) => generate(A, B, C,  D);
    public GeneratedQuad() => generate(
            Geometry.Collision.quad.A, 
            Geometry.Collision.quad.B, 
            Geometry.Collision.quad.C, 
            Geometry.Collision.quad.D);
    
    public void generate(Vector3 A, Vector3 B, Vector3 C, Vector3 D) {
        _collision = new Quad(A,B,C,D);
        
        render_info.build_vertex_buffer(
            new VertexPositionColorTexture(shape.A, Color.White, Vector2.Zero),
            new VertexPositionColorTexture(shape.B, Color.White, Vector2.UnitX),
            new VertexPositionColorTexture(shape.C, Color.White, Vector2.One),
            new VertexPositionColorTexture(shape.D, Color.White, Vector2.UnitY)
        );
        
        render_info.build_index_buffer(0,1,2, 0,2,3);
        render_info.rasterizer_state = RasterizerState.CullNone;
    }
}

public static partial class Geometry {
    public static partial class Collision {
        public static Quad quad = new();
    }
} 