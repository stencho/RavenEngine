using System;
using Microsoft.Xna.Framework;
using RavenRPG.Graphics;
using RavenRPG.Graphics.Drawing3D;

namespace RavenRPG.Engine.Collision.Shapes3D {
    public class Polyhedron : Shape3D {
        public Vector3 start_point => verts[0];
        public Vector3 center => find_center();
        public shape_type shape { get; } = shape_type.polyhedron;

        public Vector3[] verts;
        Vector3 find_center() {
            return Vector3.Zero;
        }
        public BoundingBox sweep_bounding_box(Matrix world, Vector3 sweep) {
            if (sweep != Vector3.Zero) {
                return CollisionHelper.BoundingBox_around_BoundingBoxes(
                    find_bounding_box(world),
                    find_bounding_box(world * Matrix.CreateTranslation(sweep))
                );
            } else {
                return find_bounding_box(world);
            }
        }
        public BoundingBox find_bounding_box(Matrix world) {
            return CollisionHelper.BoundingBox_around_transformed_points(world, verts);
        }

        public Polyhedron(params Vector3[] points) {
            if (points.Length < 1) throw new Exception();

            verts = points;
        }

        public void draw(Matrix world) {
            foreach (Vector3 point in verts) {
                foreach (Vector3 point2 in verts) {
                    var a = Vector3.Transform(point, world);
                    var b = Vector3.Transform(point2, world);

                    Draw3D.line(a, b, Color.Red);
                }
                    
                Draw3D.xyz_cross(Vector3.Transform(point, world), 0.1f, Color.Red);
            }
        }
        public Vector3 support(Vector3 direction, Vector3 sweep) {
            return Supports.Polyhedron(direction, verts);
        }
    }
}
