using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Raven.Engine.Collision;
using Raven.Engine.Collision.Shapes3D;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace Raven.Engine.Geometry3D;

public class GeneratedCone : GeneratedGeometry {
    public Cone shape => _collision as Cone; 
    
    private Shape3D _collision;
    public override Shape3D collision => _collision;
    
    private RenderPackage _render_info = null;
    public override RenderPackage render_info => _render_info;

    public GeneratedCone() {
        uint subdivs = 32;
        
        _collision = new Cone(1f, 0.5f, (int)subdivs, Vector3.Forward);
        _render_info = new RenderPackage();

        VertexPositionColorTexture[] data = new VertexPositionColorTexture[subdivs + 2];
        
        data[0] = new VertexPositionColorTexture(shape.A, Color.White, new Vector2(0.5f, 0.5f));
        data[1] = new VertexPositionColorTexture(shape.B, Color.White, new Vector2(0.5f, 0.5f));
        
        for (int i = 2; i < subdivs + 2; i++) {
            data[i] = new VertexPositionColorTexture(shape.points[i-1], Color.White, new Vector2((i-2 / (subdivs)), 0.5f));
        }
        
        uint[] indices = new uint[((subdivs) * 3) * 2];
        
        for (uint ind = 0; ind < subdivs-1; ind++) {
            indices[(ind * 3) + 0] = 0;
            indices[(ind * 3) + 1] = (ind+2 + 1);
            indices[(ind * 3) + 2] = (ind+2);
        }
        indices[((subdivs) * 3) - 3] = 0;
        indices[((subdivs) * 3) - 2] = 2;
        indices[((subdivs) * 3) - 1] = subdivs+1;
        
        for (uint ind = 0; ind < subdivs-1; ind++) {
            indices[((ind + subdivs) * 3) + 0] = 1;
            indices[((ind + subdivs) * 3) + 1] = (ind+2);
            indices[((ind + subdivs) * 3) + 2] = (ind+2 + 1);
        }
        indices[((subdivs * 2) * 3) - 3] = 1;
        indices[((subdivs * 2) * 3) - 2] = subdivs+1;
        indices[((subdivs * 2) * 3) - 1] = 2;
        
        _render_info.build_vertex_buffer(data);
        _render_info.build_index_buffer(indices);
    }
    
    
}