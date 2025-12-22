using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using ProjectRaven.Graphics.Drawing3D;
using ProjectRaven.Graphics;

namespace ProjectRaven.Engine;

public class Player : Entity {
    public ChunkPosition chunk_position { get; set; }
    
    public List<DynamicLight> lights { get; set; }
    
    public void Update() {
    }

    public void AfterCollision() {
    }

    public void UpdateGraphics() {
    }

    //public render_info RenderInfo { get; set; } = null;

    public Action<Entity> Draw2D { get; set; }
}