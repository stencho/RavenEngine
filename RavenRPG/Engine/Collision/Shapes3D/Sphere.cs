using Microsoft.Xna.Framework;
using RavenRPG.Renderer;

namespace RavenRPG.Engine.Collision.Shapes3D {
    public class Sphere : Shape3D {
        public Vector3 start_point => P;
        public Vector3 center => P;
        public shape_type shape { get; } = shape_type.sphere;

        public Vector3 P;

        public float radius { get; set; } = 0f;

        public BoundingBox sweep_bounding_box(Matrix world, Vector3 sweep) {
            if (sweep != Vector3.Zero) {
                return CollisionHelper.BoundingBox_around_capsule(
                    Vector3.Transform(P, world),
                    Vector3.Transform(P+sweep, world), 
                    radius);
            } else {
                return find_bounding_box(world);
            }
        }
        public BoundingBox find_bounding_box(Matrix world) {
            return CollisionHelper.BoundingBox_around_sphere(radius + Math3D.big_epsilon, world);
        }

        public Sphere(float radius) {
            P = Vector3.Zero;

            this.radius = radius;
        }

        public void draw(Matrix world) {
            Draw3D.sphere(Vector3.Transform(Vector3.Zero, world), radius, Color.MonoGameOrange);
            Draw3D.cube(find_bounding_box(world), Color.MonoGameOrange);
        }

        public Vector3 support(Vector3 direction, Vector3 sweep) {
            if (sweep != Vector3.Zero) {
                return Supports.Capsule(direction, P, P + sweep, radius);
            }
            return P + (Vector3.Normalize(direction) * radius) ;
        }
    }

}
