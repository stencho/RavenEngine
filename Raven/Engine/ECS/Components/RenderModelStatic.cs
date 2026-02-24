using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Raven.Graphics;
using Raven.Graphics.Drawing3D;

namespace Raven.Engine.Components;

//PROPERTIES
[ComponentProperty("ModelName", typeof(string))]
[ComponentProperty("TextureName", typeof(string))]

[ComponentProperty("Scale", typeof(float))]
[ComponentProperty("Orientation", typeof(Matrix))]
[ComponentProperty("Offset", typeof(Vector3))]

[ComponentProperty("BlendState", typeof(BlendState))]
[ComponentProperty("RasterizerState", typeof(RasterizerState))]

public partial class RenderModelStatic : Component {

    public Matrix WorldMatrix => Matrix.CreateScale(Scale) * Orientation * Matrix.CreateTranslation(parent.position.offset_interpolated + OffsetFromParent);
    
    public Model Model => Resources.GetModel(ModelName);
    public Texture2D Texture => Resources.GetTexture(TextureName);

    public Vector3 OffsetFromParent = Vector3.Zero;
    
    
    public RenderModelStatic(string model = "cube", string texture = "OnePXWhite") {
        add_data("ModelName", model);
        add_data("TextureName", texture);
        
        add_data("Offset", Vector3.Zero);
        add_data("Scale", 1.0f);
        add_data("Orientation", Matrix.Identity);

        add_data("BlendState", BlendState.AlphaBlend);
        add_data("RasterizerState", RasterizerState.CullCounterClockwise);
    }

    public void ForAllMeshParts(Action<VertexBuffer, IndexBuffer> action) {
        for (int mesh_index = 0; mesh_index < Model.Meshes.Count; mesh_index++) {
            for (int part_index = 0; part_index < Model.Meshes[mesh_index].MeshParts.Count; part_index++) {
                action(Model.Meshes[mesh_index].MeshParts[part_index].VertexBuffer,
                    Model.Meshes[mesh_index].MeshParts[part_index].IndexBuffer);
            }
        }
    }

    public void DrawBasic(Camera camera, GBuffer buffer, Vector3 chunk_offset) {
        Draw3D.batch_draw_setup(camera,buffer);
        
        ForAllMeshParts((VertexBuffer VertexBuffer, IndexBuffer IndexBuffer) => {
            Draw3D.batch_draw_diffuse_texture(camera, buffer,  VertexBuffer, IndexBuffer, Texture, Color.White, WorldMatrix * Matrix.CreateTranslation(chunk_offset));
        });
    }
}

[ComponentProperty("ModelName", typeof(string))]
[ComponentProperty("TextureName", typeof(string))]

[ComponentProperty("Scale", typeof(float))]
[ComponentProperty("Orientation", typeof(Matrix))]
[ComponentProperty("Offset", typeof(Vector3))]

[ComponentProperty("BlendState", typeof(BlendState))]
[ComponentProperty("RasterizerState", typeof(RasterizerState))]
public partial class RenderModelStaticCollision : Component {

    public Matrix WorldMatrix => GetData<Matrix>("Orientation") * Matrix.CreateScale(GetData<Vector3>("Scale"));
    
    public Model Model => Resources.GetModel(ModelName);
    public Texture2D Texture => Resources.GetTexture(TextureName);
    
    public RenderModelStaticCollision(string model = "cube", string texture = "OnePXWhite") {
        add_data("ModelName", model);
        add_data("TextureName", texture);
        
        add_data("Offset", Vector3.Zero);
        add_data("Scale", 1.0f);
        add_data("Orientation", Matrix.Identity);

        add_data("BlendState", BlendState.AlphaBlend);
        add_data("RasterizerState", RasterizerState.CullCounterClockwise);
    }
}