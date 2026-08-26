using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Raven.Graphics;

namespace Raven.Engine.Collision.Shapes3D;

public class Cone : Shape3D {
    public shape_type type => shape_type.cone;
    
    public Vector3 start_point => A;
    public Vector3 center => (B - A) * 0.5f;

    public Vector3 A, B;
    public Vector3[] points;

    public float length = 1f;
    public float radius = 1f;
    
    public Cone() {
        A = Vector3.Zero;
        B = A + (length * Vector3.Up);

        create((B - A).Length(), 16);
    }
    public Cone(float length, float radius, int subdivisions, Vector3 direction) {
        if (subdivisions < 3) subdivisions = 3;
        this.length = length;
        this.radius = radius;
        
        direction.Normalize();
        
        A = Vector3.Zero;
        B = A + (length * direction);

        create((B - A).Length(), subdivisions);
    }

    public void create(float length, int subdivisions = 3) {
        if (subdivisions < 3) subdivisions = 3;
        points = new Vector3[subdivisions + 1];
        points[0] = A;
        
        var normal = Vector3.Normalize(A-B);
        
        var cross = Vector3.Normalize(Vector3.Cross(normal, Vector3.Cross(normal, new Vector3(normal.X, normal.Y, -normal.Z))));
        if (cross.contains_nan()) cross = Vector3.Normalize(Vector3.Cross(normal, Vector3.Cross(normal, new Vector3(-normal.X, normal.Y, normal.Z))));
        if (cross.contains_nan()) cross = Vector3.Normalize(Vector3.Cross(normal, Vector3.Cross(normal, new Vector3(normal.X, -normal.Y, normal.Z))));
        if (cross.contains_nan()) cross = Vector3.Normalize(Vector3.Cross(normal, Vector3.Up));
        if (cross.contains_nan()) cross = Vector3.Normalize(Vector3.Cross(normal, Vector3.Forward));
        if (cross.contains_nan()) cross = Vector3.Normalize(Vector3.Cross(normal, Vector3.Right));

        for (int i = 1; i < subdivisions+1; i++) {
            points[i] = B + (Vector3.Transform(cross, Matrix.CreateFromAxisAngle(normal,MathHelper.ToRadians(((float)(i-1) / (subdivisions)) * 360f))) * (radius));
        }

        //points[subdivisions - 1] = points[1];
    }
    
    public BoundingBox find_bounding_box(Matrix world) {
        return CollisionHelper.BoundingBox_around_transformed_points(world, points);
    }

    public Vector3 support(Vector3 direction, Vector3 sweep) {
        if (sweep != Vector3.Zero) {
            return Supports.ConeSweep(direction, this, sweep);
        }
        return Supports.Cone(direction, this);
    }

    public void draw(Camera camera, GBuffer gbuffer, Matrix world) {
        
    }

    public Vector3[] get_all_points() {
        return points;
    }
}