using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Raven.Graphics;
using Raven.Graphics.Drawing3D;

namespace Raven.Engine;

public class Player : Entity {
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


    //public render_info RenderInfo { get; set; } = null;

    //public Action<Entity> Draw2D { get; set; }
}