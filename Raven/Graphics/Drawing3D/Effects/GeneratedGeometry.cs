using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Raven.Engine;
using Raven.Engine.Collision;
using Raven.Graphics.Drawing3D;
using Raven.Graphics.Effects;

namespace Raven.Engine.Geometry3D;

public abstract partial class GeneratedGeometry {
    public abstract Shape3D collision { get; }
    public abstract RenderPackage render_info { get; }
}

public class RenderPackage {
    public VertexBuffer vertex_buffer;
    public VertexPositionColorTexture[] vertex_buffer_data;
    
    public IndexBuffer index_buffer;
    public uint[] index_buffer_data;
    
    public RasterizerState rasterizer_state;
    public SamplerState diffuse_sampler_state;

    public RenderPackage() {
        rasterizer_state = RasterizerState.CullCounterClockwise;
        diffuse_sampler_state = SamplerState.PointWrap;
    }
    
    public RenderPackage(Texture2D texture, RasterizerState cull_mode, SamplerState sampler_mode) {
        rasterizer_state = cull_mode;
        diffuse_sampler_state = sampler_mode;
    }
    
    public void build_vertex_buffer(params VertexPositionColorTexture[] data) {
        vertex_buffer = new VertexBuffer(State.graphics_device, VertexPositionColorTexture.VertexDeclaration, data.Length,
            BufferUsage.None);
        vertex_buffer_data = data;
        vertex_buffer.SetData(vertex_buffer_data);
    }
    
    public void build_index_buffer(params uint[] indices) {
        index_buffer = new IndexBuffer(State.graphics_device, IndexElementSize.ThirtyTwoBits, indices.Length, BufferUsage.None);
        index_buffer_data = indices;
        index_buffer.SetData(index_buffer_data);
    }
}