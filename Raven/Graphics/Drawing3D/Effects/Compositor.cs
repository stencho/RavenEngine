using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Raven.Engine;
using Raven.Graphics.Drawing2D;
using Raven.Graphics.Effects;
using Raven.Graphics.Skybox;

namespace Raven.Graphics.Drawing3D.Effects;

public class Compositor : ManagedEffect {
    public Compositor() : base(Resources.GetShader("r_compositor")) { }

    public void save_all_buffers(Camera camera) {
        var gbuffer = camera.gbuffer;
        
        save_buffer_as_png("diffuse", gbuffer.rt_diffuse);
        save_buffer_as_png("lighting", gbuffer.rt_lighting);
        save_buffer_as_png("depth", gbuffer.rt_depth);
        save_buffer_as_png("rt_2D", gbuffer.rt_2D);
        save_buffer_as_png("composed", gbuffer.rt_composed);
        save_buffer_as_png("final", gbuffer.rt_final.present);
    }

    void save_buffer_as_png(string name, RenderTarget2D target) {
        using (Stream stream = File.Create($"{name}.jpg"))
            target.SaveAsPng(stream, target.Width, target.Height);
    }
    
    public void clear_buffers(Camera camera) {
        var gbuffer = camera.gbuffer;
        
        gbuffer.draw_to_bindings_for_clear();
        
        foreach (var rtb in gbuffer.target_bindings) {
            ((RenderTarget2D)rtb.RenderTarget).use(); 
            ((RenderTarget2D)rtb.RenderTarget).clear();
        }

        change_technique("clear");
        render_fullscreen_quad();
    }
    
    public void compose(Camera camera) {
        State.graphics_device.DepthStencilState = DepthStencilState.None;
        State.graphics_device.RasterizerState = RasterizerState.CullNone;
        State.graphics_device.BlendState = BlendState.AlphaBlend;
        
        var gbuffer = camera.gbuffer;
        
        gbuffer.rt_composed.use();
        gbuffer.rt_composed.clear();
        
        set_param("Diffuse",  gbuffer.rt_diffuse);
        set_param("Lighting", gbuffer.rt_lighting);
        set_param("Depth",    gbuffer.rt_depth);
        set_param("Normal",   gbuffer.rt_normal);
        
        set_param("buffer", State.draw_debug_buffer);
        
        change_technique("compose");
        render_fullscreen_quad();
    }

    public void finalize(Camera camera) {
        State.graphics_device.DepthStencilState = DepthStencilState.None;
        State.graphics_device.BlendState = BlendState.AlphaBlend;
        
        var buffer = camera.gbuffer;
        buffer.rt_final.offscreen.use(); 
        buffer.rt_final.offscreen.clear();
        
        set_param("Composed", buffer.rt_composed);
        set_param("Overlay", buffer.rt_2D);
        
        change_technique("finalize");
        render_fullscreen_quad();
    }
    
    public void draw_buffer_to_screen(GBuffer buffer) {
        State.graphics_device.DepthStencilState = DepthStencilState.None;
        State.graphics_device.RasterizerState = RasterizerState.CullCounterClockwise;
        State.graphics_device.BlendState = BlendState.AlphaBlend;
        
        State.graphics_device.SetRenderTarget(null);
        
        buffer.rt_final.FlipTargets();
        
        set_param("Output", buffer.rt_final.present);
        
        if (buffer.screen_draw_info.fullscreen) {
            if (buffer.resolution_scale <= 1.0) State.graphics_device.SamplerStates[0] = SamplerState.PointWrap;
            else State.graphics_device.SamplerStates[0] = SamplerState.LinearWrap;
            
            change_technique("draw_fullscreen");
            render_fullscreen_quad();
            
        } else {
            change_technique("draw_to_screen");
            render_screen_quad(buffer.screen_draw_info.position, buffer.screen_draw_info.size);
        }
    }
    
    void render_fullscreen_quad() {
        Renderer.fullscreen_quad.UseBuffers();
        
        apply_passes();
        State.graphics_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
    }
    
    void render_screen_quad(Vector2i position, Vector2i size) {
        Renderer.fullscreen_quad.UseBuffers();
        
        set_param("screen_resolution", State.resolution.ToVector2());
        set_param("screen_draw_position", position);
        set_param("screen_draw_size", size);
        
        apply_passes();
        State.graphics_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
    }
}