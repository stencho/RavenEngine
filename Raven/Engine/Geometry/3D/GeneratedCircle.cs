using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Raven.Engine.Collision;
using Raven.Engine.Collision.Shapes3D;

namespace Raven.Engine.Geometry3D;

public class GeneratedCircle : GeneratedGeometry {
    public Circle shape => _collision as Circle;
    
    private Shape3D _collision;
    public override Shape3D collision => _collision;

    private RenderPackage _render_info;
    public override RenderPackage render_info => _render_info;

    public GeneratedCircle() {
        uint subdivs = 32;
        
        _render_info = new RenderPackage();
        _collision = new Circle((int)subdivs, 0.5f, Vector3.Forward);
        
        VertexPositionColorTexture[] verts = new  VertexPositionColorTexture[subdivs + 1];
        verts[0] = new VertexPositionColorTexture(Vector3.Zero, Color.White, new Vector2(0.5f, 0.5f));
        
        for (int i = 1; i < subdivs + 1; i++) {
            verts[i] = new VertexPositionColorTexture(shape.points[i], Color.White, Vector2.One * 0.5f - shape.points[i].XY());
        }
        
        uint[] indices = new uint[(subdivs + 1) * 3];
        for (uint i = 0; i < subdivs-1; i++) {
            indices[(i * 3) + 0] = 0;
            indices[(i * 3) + 1] = i + 2;
            indices[(i * 3) + 2] = i + 1;
        }
        indices[((subdivs) * 3) + 0] = 0;
        indices[((subdivs) * 3) + 1] = 1;
        indices[((subdivs) * 3) + 2] = subdivs;
        

        render_info.build_vertex_buffer(verts);
        render_info.build_index_buffer(indices);
        
        render_info.rasterizer_state =  RasterizerState.CullNone;

    }
}