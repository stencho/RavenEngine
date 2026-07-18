using Microsoft.Xna.Framework;
using Raven.Engine.Collision;
using Raven.Graphics.Drawing3D;

namespace Raven.Engine.Components;


[ComponentProperty("Light", typeof(DynamicLight))]
public partial class LightComponent : Component {
    public LightComponent(DynamicLight light) {
        add_data("Light", light);
    }
    
    public override ComponentFlags flags => ComponentFlags.Light;
    
    public override void Render(Camera camera) { }
    public override void RenderForward(Camera camera) { }
    public override void RenderZPrePass(Camera camera) {}

    public override Shape3D? GetShape() => null;
    public override BoundingBox? GetBounds() => null;
    public override collision_result? TestGJKAgainstShape(Shape3D test_shape, Matrix world) => null;

}