using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Raven.Engine;
using Raven.Engine.Collision;
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

    public Matrix world_transform;

    public Color color = Color.White;
    public Texture2D texture;

    public RasterizerState rasterizer_state;
    public SamplerState sampler_state;

    public RenderPackage() {
        texture = Resources.GetTexture("OnePXWhite");

        rasterizer_state = RasterizerState.CullCounterClockwise;
        sampler_state = SamplerState.PointClamp;
    }
    
    public RenderPackage(Texture2D texture, RasterizerState cull_mode, SamplerState sampler_mode) {
        this.texture = texture;

        rasterizer_state = cull_mode;
        sampler_state = sampler_mode;
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
    
    public void draw_forward() {
        
    }
}