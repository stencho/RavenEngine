using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Raven.Engine.Collision;
using Raven.Engine.Collision.Shapes3D;
using Raven.Engine.Components;
using Raven.Engine.Geometry3D;
using Raven.Graphics.Drawing3D;

namespace Raven.Engine.Entities;

public partial class Brush : Entity {
    public ColliderMeshComponent collider_mesh;
    public Shape3D shape => collider_mesh.GetShape();
    
    public Brush(shape_type shape, string texture_name) {
        add_shape_component(shape, texture_name);
    }

    void add_shape_component(shape_type shape, string texture_name) {
        switch (shape) {
            case shape_type.cube: 
                collider_mesh = Components.AddComponent(this, new ColliderMeshComponent(new GeneratedCube(), texture_name)) as ColliderMeshComponent;
                return;
            
            case shape_type.polyhedron: break;
            
            case shape_type.quad:
                collider_mesh = Components.AddComponent(this, new ColliderMeshComponent(new GeneratedQuad(), texture_name)) as ColliderMeshComponent; 
                return;
            
            case shape_type.tri: 
                collider_mesh = Components.AddComponent(this, new ColliderMeshComponent(new GeneratedTri(), texture_name)) as ColliderMeshComponent; 
                return;
            
            
            case shape_type.cone: 
                collider_mesh = Components.AddComponent(this, new ColliderMeshComponent(new GeneratedCone(), texture_name)) as ColliderMeshComponent; 
                return;
            
            case shape_type.circle: 
                collider_mesh = Components.AddComponent(this, new ColliderMeshComponent(new GeneratedCircle(), texture_name)) as ColliderMeshComponent; 
                return;
                
            case shape_type.capsule: break;
            case shape_type.cylinder: break;
            case shape_type.line: break;
            case shape_type.sphere: break;
            case shape_type.dummy: break;
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
        var b = collider_mesh.GetBounds().Value;
        NewGenDraw3D.Cube(Color.Green, collider_mesh.WorldMatrix, b.A(), b.B(), b.C(), b.D(), b.E(), b.F(), b.G(), b.H());
        
        switch (shape.type) {
            case shape_type.cube:
                var c = shape as Cube;
                NewGenDraw3D.Cube(Color.MonoGameOrange, collider_mesh.WorldMatrix, c.A, c.B, c.C, c.D, c.E, c.F, c.G, c.H);
                return;
            
            case shape_type.polyhedron: break;
            
            case shape_type.quad:
                var q = shape as Quad;
                NewGenDraw3D.QuadTriangles(Color.MonoGameOrange, collider_mesh.WorldMatrix, false, q.A, q.B, q.C, q.D);
                return;
            
            case shape_type.tri: 
                var tri = shape as Triangle;
                NewGenDraw3D.Triangle(Color.MonoGameOrange, collider_mesh.WorldMatrix, tri.A, tri.B, tri.C);
                return;
            
            case shape_type.cone:
                var cone = shape as Cone;
                NewGenDraw3D.Cone(cone, Color.MonoGameOrange, collider_mesh.WorldMatrix);
                return;

            case shape_type.circle:
                var circle = shape as Circle;
                NewGenDraw3D.Line(Color.MonoGameOrange, collider_mesh.WorldMatrix, true, circle.points[1..]);
                return;
            
            case shape_type.capsule: break;
            case shape_type.cylinder: break;
            case shape_type.line: break;
            case shape_type.sphere: break;
            case shape_type.dummy: break;
            default: throw new ArgumentOutOfRangeException(nameof(shape), shape, null);
        }

    }
}
