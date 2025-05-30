using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using RavenRPG.Renderer;

namespace RavenRPG.Engine.World;

public class Player : Entity {
    public Vector2i64 chunk_index { get; set; }
    public Vector3 chunk_offset { get; set; }
    
    public List<DynamicLight> lights { get; set; }

    public void Update() {
    }

    public void AfterCollision() {
    }

    public void UpdateGraphics() {
    }

    public Action<Entity> Draw3D { get; set; }
    public Action<Entity> Draw2D { get; set; }
}