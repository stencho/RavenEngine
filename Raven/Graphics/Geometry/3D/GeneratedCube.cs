using Microsoft.Xna.Framework.Graphics;

namespace Raven.Graphics.Geometry3D;

public class GeneratdCube : GeneratedGeometry {
    internal override VertexBuffer vertex_buffer { get; }
    internal override VertexPositionColorTexture vertex_buffer_data { get; }
    
    public override void generate() {
        throw new System.NotImplementedException();
    }

    public override void draw_deferred() {
        throw new System.NotImplementedException();
    }

    public override void draw_forward() {
        throw new System.NotImplementedException();
    }
}