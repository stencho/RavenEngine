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
        set_param("atmosphere_color", camera.environment.atmosphere_color.ToVector3());
        set_param("sky_color", camera.environment.sky_color.ToVector3());
            
        set_param("far_clip", camera.far_clip);
        set_param("camera_pos", camera.position);
            
        set_param("View", camera.view);
        set_param("Projection", camera.projection);
            
        set_param("fog", true);
        set_param("fog_start", 0.85f);
        set_param("fog_end", 0.98f);
        
        camera.gbuffer.rt_depth.use();
        camera.gbuffer.rt_depth.clear(Color.White);
    }
    
    public void render_batch_step(Camera camera, VertexBuffer vertex_buffer, IndexBuffer index_buffer, Texture2D texture, Matrix world, Color tint) {
        set_states();
        
        set_param("World", world);
        set_param("WVIT", Matrix.Transpose(Matrix.Invert(world * camera.view)));
            
        set_param("tint", tint.ToVector4());
            
        set_vertex_buffer(vertex_buffer, index_buffer);
        apply_passes();
        render_vertex_buffer();
            
        set_param("tint", Color.White.ToVector4());
    }
}
