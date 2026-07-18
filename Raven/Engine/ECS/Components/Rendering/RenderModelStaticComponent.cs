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
[ComponentProperty("TextureName", typeof(string))]

[ComponentProperty("Tint", typeof(Color))]
[ComponentProperty("Opacity", typeof(float))]

[ComponentProperty("Scale", typeof(float))]
[ComponentProperty("Orientation", typeof(Matrix))]
[ComponentProperty("Offset", typeof(Vector3))]

[ComponentProperty("BlendState", typeof(BlendState))]
[ComponentProperty("RasterizerState", typeof(RasterizerState))]

[ComponentProperty("HasTransparentTexture", typeof(bool))]

public partial class RenderModelStatic : Component {

    public override ComponentFlags flags => ComponentFlags.Render;
    
    public Matrix WorldMatrix => Matrix.CreateScale(Scale) * Orientation * Matrix.CreateTranslation(parent.position.position_interpolated + OffsetFromParent);
    
    public Model Model => Resources.GetModel(ModelName);
    public Texture2D Texture => Resources.GetTexture(TextureName);

    public Vector3 OffsetFromParent = Vector3.Zero;
    
    public RenderModelStatic(string model = "cube", string texture = "OnePXWhite") {
        add_data("ModelName", model);
        add_data("TextureName", texture);
        
        add_data("Tint", Color.White);
        add_data("Opacity", 1.0f);
        
        add_data("Offset", Vector3.Zero);
        add_data("Scale", 1.0f);
        add_data("Orientation", Matrix.Identity);

        add_data("RasterizerState", RasterizerState.CullCounterClockwise);
        
        add_data("HasTransparentTexture", false);
        if (Resources.GetTextureHasTransparency(texture)) 
            HasTransparentTexture = true;
    }

    public override void RenderZPrePass(Camera camera) {
        Model.ForAllMeshParts((VertexBuffer VertexBuffer, IndexBuffer IndexBuffer) => {
            Renderer.z_prepass.render_batch_step(
                camera,
                VertexBuffer, IndexBuffer,
                Texture, WorldMatrix, Tint);
        });
    }
    
    public override void Render(Camera camera) {
        Model.ForAllMeshParts((VertexBuffer VertexBuffer, IndexBuffer IndexBuffer) => {
            Renderer.deferred.render_step(
                camera,
                VertexBuffer, IndexBuffer, 
                Texture, WorldMatrix, Tint);
        });
    }
    public override void RenderForward(Camera camera) {
        Model.ForAllMeshParts((VertexBuffer VertexBuffer, IndexBuffer IndexBuffer) => {
            Renderer.forward.render_step(
                camera,
                VertexBuffer, IndexBuffer, 
                Texture, WorldMatrix, Tint, Opacity);
        });
    }
    
    public override Shape3D? GetShape() => null;
    public override collision_result? TestGJKAgainstShape(Shape3D test_shape, Matrix world) => null;

    public override BoundingBox? GetBounds() {
        return new BoundingBox();
    }
}