using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Raven.Graphics;
using Raven.Graphics.Drawing3D;
using Raven.Engine;
using Raven.Engine.Collision;

namespace Raven.Engine.Components;

//PROPERTIES
[ComponentProperty("ModelName", typeof(string))]

[ComponentProperty("Tint", typeof(Color))]
[ComponentProperty("Opacity", typeof(float))]

[ComponentProperty("Scale", typeof(float))]
[ComponentProperty("Orientation", typeof(Matrix))]
[ComponentProperty("Offset", typeof(Vector3))]

[ComponentProperty("RasterizerState", typeof(RasterizerState))]

[ComponentProperty("HasTransparentTexture", typeof(bool))]

public partial class ColliderModelComponent : Component {
    public override ComponentFlags flags => ComponentFlags.Render;
    
    public Matrix WorldMatrix => Matrix.CreateScale(Scale) * Orientation * Matrix.CreateTranslation(parent.position.position_interpolated + OffsetFromParent);

    private Shape3D shape;
    public override Shape3D? GetShape() => shape;
    public override BoundingBox? GetBounds() => shape.find_bounding_box(WorldMatrix);
    public override collision_result? TestGJKAgainstShape(Shape3D test_shape, Matrix world) => 
        GJK.gjk_intersects(test_shape, shape, world, WorldMatrix);
    
    public Model Model => Resources.GetModel(ModelName);
    
    public Vector3 OffsetFromParent = Vector3.Zero;
    
    public ColliderModelComponent(string model) {
        add_data("ModelName", model);
        
        add_data("Tint", Color.White);
        add_data("Opacity", 1.0f);
        
        add_data("Offset", Vector3.Zero);
        add_data("Scale", 1.0f);
        add_data("Orientation", Matrix.Identity);

        add_data("RasterizerState", RasterizerState.CullCounterClockwise);
        
        add_data("HasTransparentTexture", false);
    }

    public override void RenderZPrepass(Camera camera) {
        State.graphics_device.RasterizerState = RasterizerState;

        Model.ForAllMeshParts((mesh, part) => {
            if (Resources.GetTextureHasTransparency(get_texture_from_part(part).Name)) {
                HasTransparentTexture = true;
            }
        });
        
        Model.ForAllMeshParts((mesh, part) => {
            Renderer.z_prepass.render_step(
                camera, part.VertexBuffer, part.IndexBuffer,
                WorldMatrix);
        });
    }

    Texture2D get_texture_from_part(ModelMeshPart part) {
        Texture2D tex;
        
        if (part.Effect is BasicEffect) {
            tex = ((BasicEffect)part.Effect).Texture;
        } else {
            tex = Resources.GetTexture("OnePXWhite");
        }

        return tex;
    }
    
    public override void RenderDeferred(Camera camera) {
        Model.ForAllMeshParts((mesh, part) => {
            State.graphics_device.RasterizerState = RasterizerState;

            Renderer.deferred.setup_step(camera, get_texture_from_part(part), WorldMatrix, Tint);
            RunBeforeRenderDeferred?.Invoke();
            Renderer.deferred.render_step(part.VertexBuffer, part.IndexBuffer);
        });
    } 

    public override void RenderForward(Camera camera) {
        Model.ForAllMeshParts((mesh, part) => {
            State.graphics_device.RasterizerState = RasterizerState;

            Renderer.forward.setup_step(camera, get_texture_from_part(part), WorldMatrix, Tint, Opacity);
            RunBeforeRenderDeferred?.Invoke();
            Renderer.forward.render_step(part.VertexBuffer, part.IndexBuffer);
        });
    }
    
}