using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Raven.Graphics;
using Raven.Graphics.Drawing3D;

namespace Raven.Engine.Collision.Shapes3D {
    public class Quad : Shape3D {
        public Vector3 start_point => A;
        public Vector3 center => (A + B+C+D) / 4f;
        public shape_type shape { get; } = shape_type.quad;

        public Vector3 A;
        public Vector3 B;
        public Vector3 C;
        public Vector3 D;

        public Vector3 origin => (A + B + C + D) / 4f;

        public VertexBuffer debug_vertex_buffer => null;
        public IndexBuffer debug_index_buffer => null;

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
            return CollisionHelper.BoundingBox_around_points(
                Vector3.Transform(A, world), 
                Vector3.Transform(B, world), 
                Vector3.Transform(C, world), 
                Vector3.Transform(D, world)
                );
        }

        public Quad() {
            create(1, 1);
        }
        public Quad(float scale) {
            create(scale, scale);
        }
        public Quad(float scale_x, float scale_y) {
            create(scale_x, scale_y);
        }
        public Quad(Vector3 A, Vector3 B, Vector3 C, Vector3 D) {
            create(A, B, C, D);
        }

        public void create(float scale_x, float scale_y) {
            A = (Vector3.Left * 0.5f * scale_x) + (Vector3.Forward * 0.5f * scale_y);
            B = (Vector3.Right * 0.5f * scale_x) + (Vector3.Forward * 0.5f * scale_y);
            C = (Vector3.Right * 0.5f * scale_x) + (Vector3.Backward* 0.5f * scale_y);
            D = (Vector3.Left * 0.5f * scale_x) + (Vector3.Backward * 0.5f * scale_y);
        }

        public void create(Vector3 A, Vector3 B, Vector3 C, Vector3 D) {
            this.A = A;
            this.B = B;
            this.C = C;
            this.D = D;
        }

        public void draw(Camera camera, GBuffer buffer, Matrix world) {
            Draw3D.cube(camera,find_bounding_box(world), Color.Magenta);

            Draw3D.fill_quad(camera, buffer, world, A, B, C, D, Color.White, "zerocool_sharper");
            Draw3D.line(camera,Vector3.Transform(A, world), Vector3.Transform(C, world), Color.Pink);
        }
        public Vector3 support(Vector3 direction, Vector3 sweep) {
            if (sweep != Vector3.Zero) {
                return Supports.Polyhedron(direction, A, B, C, D,
                    A + sweep, B + sweep, C + sweep, D + sweep);
            }
            return Supports.Quad(direction, A, B, C, D);

            return Supports.Polyhedron(direction, A, B, C, D,
                A + (Vector3.Down * 0.1f), B + (Vector3.Down * 0.1f), C + (Vector3.Down * 0.1f), D + (Vector3.Down * 0.1f));
        }

    }
}
