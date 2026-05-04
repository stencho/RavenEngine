using System;
using Microsoft.Xna.Framework;
using Raven.Engine.Collision;
using Raven.Engine.Collision.Shapes3D;

namespace Raven.Engine.Entities;

public partial class Brush : Entity {
    shape_type shape => collision.shape;
    Shape3D collision;

    public Brush(Shape3D shape) { collision = shape; }
    public Brush(shape_type shape) {
        switch (shape) {
            case shape_type.cube: collision = new Cube(); break;
            case shape_type.polyhedron: collision = new Polyhedron(); break;
            case shape_type.quad: collision = new Quad(); break;
            case shape_type.tri: collision = new Triangle(); break;
            case shape_type.capsule: collision = new Capsule(); break;
            case shape_type.cylinder: collision = new Cylinder(); break;
            case shape_type.line: collision = new Line3D(); break;
            case shape_type.sphere: collision = new Sphere(); break;
            case shape_type.dummy: collision = new DummySupport(); break;
            default: throw new ArgumentOutOfRangeException(nameof(shape), shape, null);
        }
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
