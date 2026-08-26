using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Raven.Engine;
using Raven.Graphics;
using Raven.Graphics.Effects;
using Raven.Graphics.Skybox;

namespace Raven.Engine.Geometry3D;

public class ZPrepass : ManagedEffect {
    public ZPrepass() : base(Resources.GetShader("r_depth")) {}
        
    public void set_states() {
        State.graphics_device.DepthStencilState = DepthStencilState.Default;
        State.graphics_device.BlendState = BlendState.Opaque;
    }
    
    // BATCH RENDERING
    public void batch_render_setup(Camera camera) {
        set_param("View", camera.view);
        set_param("Projection", camera.projection);
            
        camera.gbuffer.rt_depth.use();
        camera.gbuffer.rt_depth.clear(Color.White);
    }
    
    public void render_step(Camera camera, VertexBuffer vertex_buffer, IndexBuffer index_buffer, Matrix world) {
        set_states();
        
        set_param("World", world);
        set_param("WVIT", Matrix.Transpose(Matrix.Invert(world * camera.view)));
            
        set_vertex_buffer(vertex_buffer, index_buffer);
        apply_passes();
        render_vertex_buffer();
    }
}
