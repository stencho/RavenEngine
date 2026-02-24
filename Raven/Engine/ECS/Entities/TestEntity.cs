using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Raven.Engine.Components;
using Raven.Engine.Worlds;
using Raven.Graphics.Drawing3D;

namespace Raven.Engine;

public partial class TestEntity : Entity {
    public TestEntity() {
        Components.AddComponent(this, new RenderModelStatic("cube", "smugdean"));
        speed = 2 + RNG.rng_float * 5;
        funny = 10 + RNG.rng_float * 40;
        
    }

    public void Initialized() {
        start = position.offset;
        chunk_start = position.index;
    }

    private Vector3ui128 chunk_start;
    private Vector3 start = Vector3.Zero;
    private float funny = 0f;
    private float speed = 0f;
    private bool boing = false;
    
    public void Update() {
        if (ChunkPosition.MeasureAbsoluteDistance(chunk_start, start, position.index, position.offset, out _) > funny) {
            boing = !boing;
        }
            
        if (boing) 
            MoveAndSlide(Vector3.Up * speed);
        else 
            MoveAndSlide(Vector3.Down * speed);
    }

    public void AfterCollision() {
    }

    public void UpdateGraphics() {
    }

}