using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Raven.Engine;
using Raven.Graphics.Effects;
using Raven.Graphics.Skybox;

namespace Raven.Graphics.Drawing3D.Effects;

public class DirectionalLight : ManagedEffect {
    private SceneEnvironment parent_environment;
    
    public DirectionalLight(SceneEnvironment environment) : base(Resources.GetShader("r_directional_light")) =>
        this.parent_environment = environment;

    /* MAYBE? big ol ortho light?????
    public void build_shadows() {}
    */
    
    public void draw_lighting(Camera camera) {
        State.graphics_device.DepthStencilState = DepthStencilState.None;
        State.graphics_device.BlendState = Renderer.light_accumulation_blend_state;
        
        var gbuffer = camera.gbuffer;
        
        gbuffer.rt_lighting.use();
        Renderer.fullscreen_quad.UseBuffers();
        
        set_param("NORMAL", gbuffer.rt_normal);
        set_param("DEPTH", gbuffer.rt_depth);

        set_param("inverse_view", Matrix.Invert(camera.view));
        
        set_param("fullbright", false);
        
        set_param("light_direction", camera.environment.sun_direction);
        set_param("light_color", camera.environment.atmosphere_color);
        
        apply_passes();
        State.graphics_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
    }
}