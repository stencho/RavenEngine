using Microsoft.Xna.Framework.Graphics;

namespace Raven.Graphics.Geometry3D;

public abstract partial class GeneratedGeometry {
    internal abstract VertexBuffer vertex_buffer { get; } 
    internal abstract VertexPositionColorTexture vertex_buffer_data { get; } 
    
    public abstract void generate();
    public abstract void draw_deferred();
    public abstract void draw_forward();
}