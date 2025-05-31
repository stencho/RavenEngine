using Microsoft.Xna.Framework;
using RavenRPG.Graphics;
using RavenRPG.Graphics.Drawing3D;

namespace RavenRPG.Engine.Collision.Shapes3D {
    public class Capsule : Shape3D {
        public Vector3 AB_normal => Vector3.Up;
        public float AB_length => Vector3.Distance(A, B);
        public float AB_full_length => Vector3.Distance(A - (AB_normal * radius), B + (AB_normal * radius));

        public Vector3 center => (A + B) / 2f;

        public Vector3 start_point => A;

        public shape_type shape { get; } = shape_type.capsule;

        public Vector3 A;
        public Vector3 B;

        public float radius { get; set; } = 0f;


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

            return CollisionHelper.BoundingBox_around_capsule(
                Vector3.Transform(A, world), Vector3.Transform(B, world), 
                radius, world);
        }

        public Capsule() {
            A = Vector3.Down * (1.8f / 2);
            B = Vector3.Up * (1.8f / 2);
            radius = 1f;
        }

        public Capsule(float height) {
            A = Vector3.Down * (height / 2);
            B = Vector3.Up * (height/2);
            radius = 1f;
        }

        public Capsule(float height, float radius) {
            A = Vector3.Down * (height / 2);
            B = Vector3.Up * (height / 2);
            this.radius = radius;
        }
        
        public Capsule(Vector3 A, Vector3 B, float radius) {
            this.A = A;
            this.B = B;
            this.radius = radius;
        }

        public void draw(Matrix world) {
            var aw = Vector3.Transform(A, world);
            var bw = Vector3.Transform(B, world);
            Draw3D.cylinder(aw, bw, radius, Color.MonoGameOrange);

            Draw3D.sphere(aw, radius, Color.MonoGameOrange);
            Draw3D.sphere(bw, radius, Color.MonoGameOrange);

            //Draw3D.cube(find_bounding_box(), Color.MonoGameOrange, State.camera.view, State.camera.projection);
            BoundingBox bb = find_bounding_box(world);

            //Draw3D.cube(bb, Color.Red);

        }
        public Vector3 support(Vector3 direction, Vector3 sweep) {
            if (sweep != Vector3.Zero) {
                return Supports.Sphere(direction, Supports.Quad(direction, A, B, A + sweep, B + sweep), radius);
            }

            return Supports.Capsule(direction, A, B, radius);
        }
    }


}
