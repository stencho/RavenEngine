using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Raven.Engine.Collision;

namespace Raven.Engine.Components;

//PROPERTIES
[ComponentProperty("Health", typeof(float))]
public partial class HealthComponent : Component {

    public override ComponentFlags flags => ComponentFlags.None;
    
    public HealthComponent(float starting_health = 1.0f) {
        add_data("Health", starting_health);
    }

    public override void RenderZPrepass(Camera camera) { }
    public override void RenderDeferred(Camera camera) { }
    public override void RenderForward(Camera camera) {}
    
    public override Shape3D GetShape() => null;
    public override BoundingBox? GetBounds() => null;
    public override collision_result? TestGJKAgainstShape(Shape3D test_shape, Matrix world) => null;


}