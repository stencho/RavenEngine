using Microsoft.Xna.Framework;
using Raven.Graphics;

namespace Raven.Engine.Collision.Shapes3D;

public class Circle : Shape3D {
    public shape_type type => shape_type.circle;
    public Vector3 start_point { get; }
    public Vector3 center => Vector3.Zero;

    public Vector3[] points;
    
    public Circle() {
        create(16, 0.5f, Vector3.Forward);    
    }
    
    public Circle(int subdivisions, float radius, Vector3 normal) {
        create(subdivisions, radius, normal);
    }

    void create(int sub, float r, Vector3 n) {
        points = new Vector3[sub + 1];
        points[0] = start_point;
        
        var normal = Vector3.Normalize(n);
        
        var cross = Vector3.Normalize(Vector3.Cross(normal, Vector3.Cross(normal, new Vector3(normal.X, normal.Y, -normal.Z))));
        if (cross.contains_nan()) cross = Vector3.Normalize(Vector3.Cross(normal, Vector3.Cross(normal, new Vector3(-normal.X, normal.Y, normal.Z))));
        if (cross.contains_nan()) cross = Vector3.Normalize(Vector3.Cross(normal, Vector3.Cross(normal, new Vector3(normal.X, -normal.Y, normal.Z))));
        if (cross.contains_nan()) cross = Vector3.Normalize(Vector3.Cross(normal, Vector3.Up));
        if (cross.contains_nan()) cross = Vector3.Normalize(Vector3.Cross(normal, Vector3.Forward));
        if (cross.contains_nan()) cross = Vector3.Normalize(Vector3.Cross(normal, Vector3.Right));

        for (int i = 1; i < sub+1; i++) {
            points[i] = start_point + (Vector3.Transform(cross, Matrix.CreateFromAxisAngle(normal,MathHelper.ToRadians(((float)(i-1) / (sub)) * 360f))) * (r));
        }
    }
    
    public BoundingBox find_bounding_box(Matrix world) {
        return CollisionHelper.BoundingBox_around_transformed_points(world, points);
    }
    
    public Vector3 support(Vector3 direction, Vector3 sweep) {
        return Supports.Polyhedron(direction, points[1..]);
    }

    public void draw(Camera camera, GBuffer gbuffer, Matrix world) { }

    public Vector3[] get_all_points() => points;
    
}