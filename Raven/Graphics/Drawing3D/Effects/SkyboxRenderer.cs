using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Raven.Engine;
using Raven.Graphics.Drawing2D;
using Raven.Graphics.Effects;
using Raven.Graphics.Skybox;

namespace Raven.Graphics.Drawing3D.Effects;

public class SkyboxRenderer : ManagedEffect {
    private SceneEnvironment parent_environment;

    public SkyboxRenderer(SceneEnvironment environment) : base(Resources.GetShaderInstance("r_skybox")) =>
        this.parent_environment = environment;

    public void render(Camera camera) {
        State.graphics_device.RasterizerState = RasterizerState.CullCounterClockwise;
        State.graphics_device.DepthStencilState = DepthStencilState.None;
        //State.graphics_device.SamplerStates[0] = SamplerState.LinearWrap;

        camera.gbuffer.draw_to_bindings_for_skybox();

        var view = Matrix.CreateLookAt(Vector3.Zero, camera.direction, camera.up_direction);
        
        set_param("skybox_world", Matrix.Identity);
        set_param("skybox_view", view);
        set_param("skybox_projection", camera.projection);
        
        set_param("inverse_view", Matrix.Invert(view));
        
        set_param("SkyboxLerp", parent_environment.atmosphere_color_cycle.debug_band);
        set_param("skybox_height", SkyboxData.skybox_height);
        
        set_param("day_position", parent_environment.current_day_value);
        
        set_param("atmosphere_color", parent_environment.atmosphere_color);
        set_param("sky_color", parent_environment.sky_color);
        
        set_param("max_sky_darkness", parent_environment.sky_maximum_darkness);

        apply_passes();
        
        State.graphics_device.DrawUserIndexedPrimitives(
            PrimitiveType.TriangleList, 
            SkyboxData.skybox_data, 
            0,
            SkyboxData.skybox_data.Length, 
            SkyboxData.skybox_indices, 
            0,
            SkyboxData.skybox_indices.Length / 3, VertexPositionColorNormalTexture.VertexDeclaration);
    }
}