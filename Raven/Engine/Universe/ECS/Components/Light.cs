using Raven.Graphics.Drawing3D;

namespace Raven.Engine.Components;


[ComponentProperty("Light", typeof(DynamicLight))]
public partial class LightComponent : Component {
    public LightComponent(DynamicLight light) {
        add_data("Light", light);
    }
}