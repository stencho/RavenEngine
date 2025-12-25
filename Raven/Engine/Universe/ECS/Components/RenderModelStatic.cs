using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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
    public override string name { get; set; } = "RenderModel";

    public Matrix WorldMatrix => get_value<Matrix>("Orientation") * Matrix.CreateScale(get_value<Vector3>("Scale"));
    
    public Model Model => Resources.GetModel(ModelName);
    public Texture2D Texture => Resources.GetTexture(TextureName);
    
    public RenderModelStatic(string model = "cube", string texture = "OnePXWhite") {
        add_data("ModelName", model);
        add_data("TextureName", texture);
        
        add_data("Offset", Vector3.Zero);
        add_data("Scale", 1.0f);
        add_data("Orientation", Matrix.Identity);

        add_data("BlendState", BlendState.AlphaBlend);
        add_data("RasterizerState", RasterizerState.CullCounterClockwise);
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
    public override string name { get; set; } = "RenderModel";

    public Matrix WorldMatrix => get_value<Matrix>("Orientation") * Matrix.CreateScale(get_value<Vector3>("Scale"));
    
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