using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Raven.Engine.Components;
using Raven.Graphics.Drawing3D;

namespace Raven.Engine;

public partial class TestEntity : Entity {
    public TestEntity() {
        Components.AddComponent(this, new RenderModelStatic("cube", "smugdean"));
        speed = 2 + RNG.rng_float * 50;
        funny = 10 + RNG.rng_float * 50;
        
    }

    public void Initialized() {
        startY = position.offset.Y;
    }

    private float startY = 0;
    private float funny = 0f;
    private float speed = 0f;
    private bool boing = false;
    public void Update() {
        if (position.offset.Y >= startY + funny) {
            boing = false;
        }

        if (position.offset.Y <= startY - funny) {
            boing = true;
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