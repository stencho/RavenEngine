using Microsoft.Xna.Framework;
using Raven.Graphics;
using Raven.Graphics.Drawing3D;

namespace Raven.Engine.Collision.Shapes3D {
    public class Line3D : Shape3D {
        public Vector3 start_point => A;
        public Vector3 center => (A + B) / 2f;
        public shape_type shape { get; } = shape_type.line;

        public Vector3 A;
        public Vector3 B;

        public BoundingBox sweep_bounding_box(Matrix world, Vector3 sweep) {
            if (sweep != Vector3.Zero) {
                return CollisionHelper.BoundingBox_around_points(A,B,A+sweep,B+sweep);
            } else {
                return find_bounding_box(world);
            }
        }
        public BoundingBox find_bounding_box(Matrix world) {
            return CollisionHelper.BoundingBox_around_points(A,B);
        }

        public Line3D() {
            A = Vector3.Zero;
            B = Vector3.Up * 1.8f;
        }

        public void create(float scale_x, float scale_y) {
            A = (Vector3.Left * 0.5f);
            B = (Vector3.Right * 0.5f);
        }

        public void draw(Matrix world) {
            Draw3D.line(
                Vector3.Transform(A, world),
                Vector3.Transform(B, world),
                Color.MonoGameOrange);
        }
        public Vector3 support(Vector3 direction, Vector3 sweep) {
            if (sweep != Vector3.Zero) {
                return Supports.Quad(direction, A,B, A+sweep,B+sweep);
            }
            return Supports.Line(direction, A,B);
        }
    }

}
