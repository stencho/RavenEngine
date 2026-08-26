using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Raven.Engine.Collision;
using Raven.Engine.Geometry3D;
using Raven.Graphics;
using Raven.Graphics.Drawing3D;
using Raven.Graphics.Effects;

namespace Raven.Engine.Components;

//PROPERTIES

[ComponentProperty("Geometry", typeof(GeneratedGeometry))]
[ComponentProperty("TextureName", typeof(string))]

[ComponentProperty("Tint", typeof(Color))]
[ComponentProperty("Opacity", typeof(float))]

[ComponentProperty("Scale", typeof(float))]
[ComponentProperty("Orientation", typeof(Matrix))]
[ComponentProperty("Offset", typeof(Vector3))]

[ComponentProperty("RasterizerState", typeof(RasterizerState))]
[ComponentProperty("SamplerState", typeof(SamplerState))]

[ComponentProperty("HasTransparentTexture", typeof(bool))]

public partial class ColliderMeshComponent : Component {
    // COMPONENT
    public override ComponentFlags flags => ComponentFlags.Collide | ComponentFlags.Render;

    public Matrix WorldMatrix => Matrix.CreateScale(Scale) * Orientation * Matrix.CreateTranslation(parent.position.position_interpolated + OffsetFromParent);
    
    // COLLIDER
    Shape3D shape => Geometry.collision;
    public override Shape3D? GetShape() => shape;
    public override BoundingBox? GetBounds() => shape.find_bounding_box(WorldMatrix);
    public override collision_result? TestGJKAgainstShape(Shape3D test_shape, Matrix world) => 
        GJK.gjk_intersects(test_shape, shape, world, WorldMatrix);
    
    // RENDERING
    RenderPackage render_info => Geometry.render_info;
    public Texture2D Texture => Resources.GetTexture(TextureName);

    // GENERAL
    public Vector3 OffsetFromParent = Vector3.Zero;
    
    public ColliderMeshComponent(GeneratedGeometry geometry, string texture = "OnePXWhite") {
        add_data("Geometry", geometry);
        
        add_data("TextureName", texture);
        
        add_data("Tint", Color.White);
        add_data("Opacity", 1.0f);

        add_data("Scale", 1.0f);
        add_data("Orientation", Matrix.Identity);
        add_data("Offset", Vector3.Zero);
        
        add_data("RasterizerState",  RasterizerState.CullNone);
        add_data("SamplerState",  SamplerState.LinearWrap);
        
        add_data("HasTransparentTexture", Resources.GetTextureHasTransparency(TextureName));
    }

    public override void RenderZPrepass(Camera camera) {
        if (render_info == null) return;
        if (render_info.vertex_buffer == null) return;
        if (render_info.index_buffer == null) return;
        State.graphics_device.RasterizerState = render_info.rasterizer_state;
        HasTransparentTexture = Resources.GetTextureHasTransparency(TextureName);
        
        Renderer.z_prepass.render_step(camera, render_info.vertex_buffer, render_info.index_buffer, WorldMatrix);
    }

    public override void RenderDeferred(Camera camera) {
        if (render_info == null) return;
        if (render_info.vertex_buffer == null) return;
        if (render_info.index_buffer == null) return;
        
        State.graphics_device.RasterizerState = render_info.rasterizer_state;
        State.graphics_device.SamplerStates[0] = render_info.diffuse_sampler_state;

        Renderer.deferred.setup_step(camera, Texture, WorldMatrix, Tint);
        RunBeforeRenderDeferred?.Invoke();
        RunBeforeRenderBoth?.Invoke();
        Renderer.deferred.render_step(render_info.vertex_buffer, render_info.index_buffer);
    } 

    public override void RenderForward(Camera camera) {
        if (render_info == null) return;
        if (render_info.vertex_buffer == null) return;
        if (render_info.index_buffer == null) return;
        
        State.graphics_device.RasterizerState = render_info.rasterizer_state;
        State.graphics_device.SamplerStates[0] = render_info.diffuse_sampler_state;
        
        Renderer.forward.setup_step(camera, Texture, WorldMatrix, Tint, Opacity);
        RunBeforeRenderForward?.Invoke();
        RunBeforeRenderBoth?.Invoke();
        Renderer.forward.render_step(render_info.vertex_buffer, render_info.index_buffer);
    }

}