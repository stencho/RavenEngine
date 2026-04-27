using System;
using Microsoft.Xna.Framework;
using Raven.Engine.Collision;
using Raven.Engine.Collision.Shapes3D;

namespace Raven.Engine.Entities;
public partial class Brush : Entity {
    shape_type shape;
    Shape3D collision;
    
    public Brush(shape_type shape) {
        this.shape = shape;
        switch (shape) {
            case shape_type.cube:
                break;
            case shape_type.polyhedron:
                break;
            case shape_type.quad:
                break;
            case shape_type.tri:
                break;
            case shape_type.capsule:
                break;
            case shape_type.cylinder:
                break;
            case shape_type.line:
                break;
            case shape_type.sphere:
                break;
            case shape_type.dummy:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(shape), shape, null);
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
