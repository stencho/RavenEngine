using System.Collections.Generic;
using Raven.Engine.Components;
using Raven.Graphics.Drawing3D;

namespace Raven.Engine;

public partial class TestEntity : Entity {
    public TestEntity() {
        Components.AddComponent(this, new RenderModelStatic("cube", "smugdean"));
    }

    public void Initialized() {
    }

    public void Update() {
    }

    public void AfterCollision() {
    }

    public void UpdateGraphics() {
    }

}