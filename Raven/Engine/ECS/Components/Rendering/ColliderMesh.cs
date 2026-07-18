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

[ComponentProperty("AlwaysRenderForward", typeof(bool))]

public partial class ColliderMesh : Component {
    public override ComponentFlags flags => ComponentFlags.Collide | ComponentFlags.Render;

    public Matrix WorldMatrix => Matrix.CreateScale(Scale) * Orientation *
                                 Matrix.CreateTranslation(parent.position.position_interpolated + OffsetFromParent);

    Shape3D shape => Geometry.collision;
    public override Shape3D? GetShape() => shape;
    
    RenderPackage render_info => Geometry.render_info;

    public Texture2D Texture => Resources.GetTexture(TextureName);

    public Vector3 OffsetFromParent = Vector3.Zero;

    public ColliderMesh(GeneratedGeometry geometry, string texture = "OnePXWhite") {
        add_data("Geometry", geometry);
        
        add_data("TextureName", texture);
        render_info.texture = Texture;
        
        add_data("Tint", Color.White);
        add_data("Opacity", 1.0f);

        add_data("Offset", Vector3.Zero);
        add_data("Scale", 1.0f);
        add_data("Orientation", Matrix.Identity);

        add_data("AlwaysRenderForward", false);
    }

    public override void RenderZPrePass(Camera camera) {
        State.graphics_device.RasterizerState = render_info.rasterizer_state;
        
        Renderer.z_prepass.render_batch_step(camera, 
            render_info.vertex_buffer, render_info.index_buffer,
            render_info.texture, WorldMatrix, Tint);
    }

    public override void Render(Camera camera) {
        State.graphics_device.RasterizerState = render_info.rasterizer_state;
        
        Renderer.deferred.render_step(camera,
            render_info.vertex_buffer, render_info.index_buffer,
            render_info.texture, WorldMatrix, Tint);
    }

    public override void RenderForward(Camera camera) {
        State.graphics_device.RasterizerState = render_info.rasterizer_state;
        
        Renderer.forward.render_step(camera,
            render_info.vertex_buffer, render_info.index_buffer,
            render_info.texture, WorldMatrix, Tint, Opacity);
    }
    
    public override collision_result? TestGJKAgainstShape(Shape3D test_shape, Matrix world) {
        return GJK.gjk_intersects(test_shape, shape, world, WorldMatrix);
    }

    public override BoundingBox? GetBounds() {
        return shape.find_bounding_box(WorldMatrix);
    }
}