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
    
    public GeneratedQuad() {
        generate(
            GeneratedBases.Collision.quad.A, 
            GeneratedBases.Collision.quad.B, 
            GeneratedBases.Collision.quad.C, 
            GeneratedBases.Collision.quad.D, 
            Color.White, null, null);
    }
    public GeneratedQuad(Vector3 A, Vector3 B, Vector3 C, Vector3 D) {
        generate(A, B, C, D, Color.White, null, null);
    }
    public GeneratedQuad(Vector3 A, Vector3 B, Vector3 C, Vector3 D, Color color, Texture2D texture = null, ManagedEffect effect = null) {
        generate(A, B, C,  D, color, texture, effect);
    }
    
    public void generate(Vector3 A, Vector3 B, Vector3 C, Vector3 D, Color color, Texture2D texture = null, ManagedEffect effect = null) {
        _collision = new Quad(A,B,C,D);
        
        render_info.build_vertex_buffer(
            new VertexPositionColorTexture(shape.A, Color.White, Vector2.Zero),
            new VertexPositionColorTexture(shape.B, Color.White, Vector2.UnitX),
            new VertexPositionColorTexture(shape.C, Color.White, Vector2.One),
            new VertexPositionColorTexture(shape.D, Color.White, Vector2.UnitY)
        );
        
        render_info.build_index_buffer(0,1,2, 0,2,3);
        
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
        public static Quad quad = new Quad(
            (Vector3.Up + Vector3.Left) * 0.5f,
            (Vector3.Up + Vector3.Right) * 0.5f,
            (Vector3.Down + Vector3.Right) * 0.5f,
            (Vector3.Down + Vector3.Left) * 0.5f
        );
    }
} 