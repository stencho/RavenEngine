using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Raven.Engine.Collision;
using Raven.Engine.Collision.Shapes3D;
using Raven.Engine.Components;
using Raven.Engine.Geometry3D;
using Raven.Graphics.Drawing3D;

namespace Raven.Engine;

public partial class TestEntity : Entity {
    private ColliderMeshComponent component;
    private Quad b => component.GetShape() as Quad;
    
    public TestEntity() {
        component = Components.AddComponent(this, new ColliderMeshComponent( new GeneratedQuad(), "smugdean")) as ColliderMeshComponent;
        speed = 2 + RNG.rng_float * 2;
        funny = 10 + RNG.rng_float * 10;
    }
    public TestEntity(Vector3 position) {
        string tex = "smugdean";
        if (RNG.rng_bool) tex = "adam";
        
        component = Components.AddComponent(this, new ColliderMeshComponent( new GeneratedQuad(), tex)) as ColliderMeshComponent;
        speed = 2 + RNG.rng_float * 2;
        funny = 10 + RNG.rng_float * 10;

        if (RNG.rng_float > 0.5f)
            this.Components.ForAllComponentsWithFlag(ComponentFlags.Render, (c) => {
                c.SetData("Opacity", RNG.rng_float);
            });
        
        start = position;
    }

    public void Initialized() {
        position.XYZ = start;
    }
    
    private Vector3 start = Vector3.Zero;
    private float funny = 0f;
    private float speed = 0f;
    private bool boing = false;
    
    public void Update() { 
        if (Vector3.Distance(start, position.XYZ) > funny) {
            boing = !boing;
        }
            
        if (boing) MoveAndSlide(Vector3.Up * speed);
        else MoveAndSlide(Vector3.Down * speed);
        
        
    }

    public void AfterCollision() {
    }

    public void UpdateGraphics() {
    }

}