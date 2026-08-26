using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Raven.Engine;
using Raven.Graphics.Drawing3D;
using Raven.Graphics.Skybox;

namespace Raven.Graphics.Effects;

public class DrawBuffersForward : ManagedEffect {
    public DrawBuffersForward() : base(Resources.GetShaderInstance("draw_forward")) {}
    
    public void set_states() {
        State.graphics_device.DepthStencilState = DepthStencilState.None;
        State.graphics_device.BlendState = BlendState.AlphaBlend;
    }
    
    // BATCH RENDERING
    public void batch_render_setup(Camera camera) {
        set_param("inverse_view", Matrix.Invert(camera.view));
        
        set_param("directional_light_dir", camera.environment.sun_direction);
        set_param("directional_light_color", camera.environment.atmosphere_color.ToVector3());
        
        set_param("far_clip", camera.far_clip);
        set_param("camera_pos", camera.position);
        
        set_param("fullbright", false);
        set_param("ignore_depth", false);
        
        set_param("View", camera.view);
        set_param("Projection", camera.projection);
        
        set_param("DEPTH", camera.gbuffer.rt_depth);
        set_param("resolution", State.resolution.ToVector2());
        
        camera.gbuffer.rt_composed.use();
    }
    public void line_render_setup(Camera camera) {
        set_param("inverse_view", Matrix.Invert(camera.view));
        
        set_param("directional_light_dir", camera.environment.sun_direction);
        set_param("directional_light_color", camera.environment.atmosphere_color.ToVector3());
        
        set_param("far_clip", camera.far_clip);
        set_param("camera_pos", camera.position);
        
        set_param("fullbright", true);
        set_param("ignore_depth", false);
        
        set_param("View", camera.view);
        set_param("Projection", camera.projection);
        
        set_param("opacity", 1f);
        
        set_param("resolution", State.resolution.ToVector2());
        
        set_param("DIFFUSE", Resources.GetTexture("OnePXWhite"));
        set_param("DEPTH", camera.gbuffer.rt_depth);
        
        camera.gbuffer.rt_composed.use();
    }

    public void setup_step(Camera camera,Texture2D texture, Matrix world, Color tint, float opacity) {
        set_param("DIFFUSE", texture);
        set_param("opacity", opacity);
        
        set_param("World", world);
        set_param("WVIT", Matrix.Transpose(Matrix.Invert(world * camera.view)));
        
        set_param("tint", tint.ToVector4());
    }
    
    public void render_step( VertexBuffer vertex_buffer, IndexBuffer index_buffer) {
        set_states();
        set_vertex_buffer(vertex_buffer, index_buffer);
        apply_passes();
        render_vertex_buffer();
    }
    
    public void render_lines_step(Camera camera, Color color, Matrix world, bool close_loop, bool ignore_depth, params Vector3[] points) {
        set_param("fullbright", true);
        
        set_param("ignore_depth", ignore_depth);
        
        set_param("World", world);
        set_param("WVIT", Matrix.Transpose(Matrix.Invert(world * camera.view)));
        
        set_param("tint", color.ToVector4());
        
        VertexPositionColor[] verts = new VertexPositionColor[points.Length + 1];

        for (int i = 0; i < points.Length; i++) {
            verts[i].Position = points[i];
        }

        verts[verts.Length-1].Position = points[0];
        
        Renderer.forward.apply_passes();
        State.graphics_device.DrawUserPrimitives(PrimitiveType.LineStrip, verts, 0, verts.Length - 1);
        
    } 
    
    // FULL RENDER, SET ALL PARAMS
    public void render(Camera camera, VertexBuffer vertex_buffer, IndexBuffer index_buffer, Texture texture, Matrix world, Color tint) {
        camera.gbuffer.rt_composed.use();
        
        set_param("texture_map", texture);

        set_param("World", world);
        set_param("View", camera.view);
        set_param("Projection", camera.projection);
        
        set_param("far_clip", camera.far_clip);
        
        set_param("atmosphere_color", camera.environment.atmosphere_color.ToVector3());
        set_param("sky_color", camera.environment.sky_color.ToVector3());
        
        set_param("camera_pos", camera.view.Translation);
        
        set_param("clip_trans", true);
        set_param("tint", tint.ToVector4());
        
        set_param("WVIT", Matrix.Transpose(Matrix.Invert(world * camera.view)));
        
        set_vertex_buffer(vertex_buffer, index_buffer);
        apply_passes();
        render_vertex_buffer();
    }
    
}