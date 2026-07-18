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

    private RenderPackage _render_info = new RenderPackage();
    public override RenderPackage render_info => _render_info;
    
    public GeneratedTri() {
        generate(
            GeneratedBases.Collision.triangle.A, 
            GeneratedBases.Collision.triangle.B, 
            GeneratedBases.Collision.triangle.C, 
            Color.White, null);
    }
    public GeneratedTri(Vector3 A, Vector3 B, Vector3 C) {
        generate(A, B, C, Color.White, null);
    }
    public GeneratedTri(Vector3 A, Vector3 B, Vector3 C, Color color, Texture2D texture = null, ManagedEffect effect = null) {
        generate(A, B, C,  color, texture);
    }
    
    public void generate(Vector3 A, Vector3 B, Vector3 C, Color color, Texture2D texture = null) {
        _collision = new Triangle(A,B,C);
        
        render_info.build_vertex_buffer(
            new VertexPositionColorTexture(shape.A, Color.White, (Vector2.UnitX * 0.5f)),
            new VertexPositionColorTexture(shape.B, Color.White, Vector2.UnitY),
            new VertexPositionColorTexture(shape.C, Color.White, Vector2.One)
            );
        
        render_info.build_index_buffer(0,1,2);
        
        render_info.color = color;
        render_info.rasterizer_state = RasterizerState.CullNone;
        
        if (texture != null) {
            render_info.texture = texture;
        } else {
            render_info.texture = Resources.GetTexture("OnePXWhite");
        }
    }
}

public static partial class GeneratedBases {
    public static partial class Collision {
        public static Triangle triangle = new Triangle(
            Vector3.Up * 0.5f,
            (Vector3.Down * 0.5f) + (Vector3.Right * 0.5f),
            (Vector3.Down * 0.5f) + (Vector3.Left * 0.5f)
        );
    }
} 