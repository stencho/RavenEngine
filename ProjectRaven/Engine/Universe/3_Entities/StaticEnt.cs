using System;
using System.Collections.Generic;
using ProjectRaven.Graphics.Drawing2D;
using ProjectRaven.Graphics.Drawing3D;

namespace ProjectRaven.Engine;

public class StaticEnt : Entity {
    public ChunkPosition chunk_position { get; set; }
    public List<DynamicLight> lights { get; set; }
    
    public void Update() {
    }

    public void AfterCollision() {
    }

    public void UpdateGraphics() {
    }

    public Action<Entity> Draw2D { get; set; }
}