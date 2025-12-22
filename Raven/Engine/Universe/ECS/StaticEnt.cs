using System;
using System.Collections.Generic;
using Raven.Graphics.Drawing2D;
using Raven.Graphics.Drawing3D;

namespace Raven.Engine;

public class StaticEnt : Entity {
    public ChunkPosition chunk_position { get; set; }
    public ChunkPosition chunk_position_stable { get; set; }
    
    public List<DynamicLight> lights { get; set; }
    
    public ComponentManager Components { get; set; } = new();
    
    public void Update() {
    }

    public void AfterCollision() {
    }

    public void UpdateGraphics() {
    }

    //public Action<Entity> Draw2D { get; set; }
}