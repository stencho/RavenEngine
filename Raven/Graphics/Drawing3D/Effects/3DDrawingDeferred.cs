using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Raven.Graphics.Drawing2D;
using Raven.Engine;
using Raven.Graphics.Drawing3D;
using Raven.Graphics.Skybox;

namespace Raven.Graphics.Effects {
    public class DrawBuffersDeferred : ManagedEffect {
        public DrawBuffersDeferred() : base(Resources.GetShaderInstance("draw_deferred")) {}
        
        public void set_states() {
            State.graphics_device.DepthStencilState = DepthStencilState.None;
            State.graphics_device.BlendState = BlendState.Opaque; 
            State.graphics_device.SamplerStates[0] = SamplerState.LinearWrap;
            State.graphics_device.SamplerStates[1] = SamplerState.PointClamp;
        }

        // BATCH RENDERING
        public void batch_render_setup(Camera camera) {
            set_param("atmosphere_color", camera.environment.atmosphere_color.ToVector3());
            set_param("sky_color", camera.environment.sky_color.ToVector3());
            
            set_param("far_clip", camera.far_clip);
            set_param("camera_pos", camera.position);
            
            set_param("View", camera.view);
            set_param("Projection", camera.projection);
            
            set_param("resolution", State.resolution.ToVector2());
            
            set_param("DEPTH", camera.gbuffer.rt_depth);
            
            camera.gbuffer.draw_to_bindings();
        }
        
        public void render_step(Camera camera, VertexBuffer vertex_buffer, IndexBuffer index_buffer, Texture2D texture, Matrix world, Color tint) {
            
            
            set_param("DIFFUSE", texture);
            
            set_param("World", world);
            set_param("WVIT", Matrix.Transpose(Matrix.Invert(world * camera.view)));
            
            set_param("tint", tint.ToVector4());
            
            set_vertex_buffer(vertex_buffer, index_buffer);
            apply_passes();
            set_states();
            render_vertex_buffer();
            
            set_param("tint", Color.White.ToVector4());
        }
        
        // FULL RENDER, SET ALL PARAMS
        public void render(Camera camera, VertexBuffer vertex_buffer, IndexBuffer index_buffer, Texture2D texture, Matrix world, Color tint) {
            set_states();
            
            set_param("texture_map", texture);

            set_param("World", world);
            set_param("View", camera.view);
            set_param("Projection", camera.projection);
            
            set_param("far_clip", camera.far_clip);
            
            set_param("atmosphere_color", camera.environment.atmosphere_color.ToVector3());
            set_param("sky_color", camera.environment.sky_color.ToVector3());
            
            set_param("camera_pos", camera.position);
            set_param("clip_trans", true);
            set_param("tint", tint.ToVector4());
            
            set_param("WVIT", Matrix.Transpose(Matrix.Invert(world * camera.view)));
            
            set_vertex_buffer(vertex_buffer, index_buffer);
            apply_passes();
            render_vertex_buffer();
            
            State.graphics_device.RasterizerState = RasterizerState.CullCounterClockwise;
        }
        
    }
}
