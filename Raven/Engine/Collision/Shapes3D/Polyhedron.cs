using System;
using Microsoft.Xna.Framework;
using Raven.Graphics;
using Raven.Graphics.Drawing3D;

namespace Raven.Engine.Collision.Shapes3D {
    public class Polyhedron : Shape3D {
        public Vector3 start_point => verts[0];
        public Vector3 center => find_center();
        public shape_type type { get; } = shape_type.polyhedron;

        public Vector3[] verts;

        public Vector3[] get_all_points() => verts;
        
        Vector3 find_center() {
            return Vector3.Zero;
        }
        
        public BoundingBox find_bounding_box(Matrix world) {
            return CollisionHelper.BoundingBox_around_transformed_points(world, verts);
        }

        public Polyhedron(params Vector3[] points) {
            if (points.Length < 1) throw new Exception();

            verts = points;
        }

        public void draw(Camera camera, GBuffer gbuffer, Matrix world) {
            foreach (Vector3 point in verts) {
                foreach (Vector3 point2 in verts) {
                    var a = Vector3.Transform(point, world);
                    var b = Vector3.Transform(point2, world);

                    Draw3D.line(camera, a, b, Color.Red);
                }
                    
                Draw3D.xyz_cross(camera, Vector3.Transform(point, world), 0.1f, Color.Red);
            }
        }
        public Vector3 support(Vector3 direction, Vector3 sweep) {
            return Supports.Polyhedron(direction, verts);
        }
    }
}
