using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Raven.Engine;
using Raven.Engine.Collision;
using Raven.Engine.Collision.Shapes3D;
using Raven.Graphics.Drawing3D;
using Raven.Graphics.Effects;

namespace Raven.Engine.Geometry3D;

public class GeneratedTri : GeneratedGeometry {
    public Triangle shape => _collision as Triangle;
    
    private Shape3D _collision;
    public override Shape3D collision => _collision;

    private RenderPackage _render_info = new();
    public override RenderPackage render_info => _render_info;
    
    public GeneratedTri() => generate(
            Geometry.Collision.triangle.A, 
            Geometry.Collision.triangle.B, 
            Geometry.Collision.triangle.C);
    
    public GeneratedTri(Vector3 A, Vector3 B, Vector3 C) {
        generate(A, B, C);
    }
    public void generate(Vector3 A, Vector3 B, Vector3 C) {
        _collision = new Triangle(A,B,C);
        
        render_info.build_vertex_buffer(
            new VertexPositionColorTexture(shape.A, Color.White, (Vector2.UnitX * 0.5f)),
            new VertexPositionColorTexture(shape.B, Color.White, Vector2.UnitY),
            new VertexPositionColorTexture(shape.C, Color.White, Vector2.One));
        
        render_info.build_index_buffer(0,1,2);
        render_info.rasterizer_state = RasterizerState.CullNone;
    }
}

public static partial class Geometry {
    public static partial class Collision {
        public static Triangle triangle = new();
    }
} 