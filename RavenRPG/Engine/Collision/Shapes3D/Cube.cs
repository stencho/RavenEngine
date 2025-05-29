using Microsoft.Xna.Framework;
using RavenRPG.Renderer;

namespace RavenRPG.Engine.Collision.Shapes3D {
    public class Cube : Shape3D {
        public Vector3 start_point => A;
        public Vector3 center => (A+B+C+D+E+F+G+H) / 8f;
        public shape_type shape { get; } = shape_type.cube;

        public Vector3 A => obb.A;
        public Vector3 B => obb.B;
        public Vector3 C => obb.C;
        public Vector3 D => obb.D;
        public Vector3 E => obb.E;
        public Vector3 F => obb.F;
        public Vector3 G => obb.G;
        public Vector3 H => obb.H;
                
        OBB obb;
        public Vector3 half_scale;

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
            return CollisionHelper.BoundingBox_around_OBB(obb, world);
        }

        public Cube() {
            obb = new OBB(Vector3.Zero, Vector3.One * 0.5f);
        }
        public Cube(float half_scale) {
            obb = new OBB(Vector3.Zero, Vector3.One * half_scale);
            this.half_scale = Vector3.One * half_scale;
        }
        public Cube(Vector3 half_scale) {
            this.half_scale = half_scale;
            obb = new OBB(Vector3.Zero, half_scale);
        }
        
        public static Vector3 farthest_point(Cube cube, Vector3 direction, out int i) {
            Vector3 pos = Vector3.Zero;

            pos += Vector3.UnitX * (Vector3.Dot(direction, Vector3.UnitX) >= 0f ? cube.half_scale.X : -cube.half_scale.X);
            pos += Vector3.UnitY * (Vector3.Dot(direction, Vector3.UnitY) >= 0f ? cube.half_scale.Y : -cube.half_scale.Y);
            pos += Vector3.UnitZ * (Vector3.Dot(direction, Vector3.UnitZ) >= 0f ? cube.half_scale.Z : -cube.half_scale.Z);

            i = 0;
            if (pos.Z > 0) {
                if (pos.Y <= 0) {
                    if (pos.X <= 0) {
                        i = 6;
                    } else {
                        i = 7;
                    }

                } else if (pos.Y > 0) {
                    if (pos.X <= 0) {
                        i = 4;
                    } else {
                        i = 5;
                    }
                }
            } else if (pos.Z < 0) {
                if (pos.Y <= 0) {
                    if (pos.X <= 0) {
                        i = 2;
                    } else {
                        i = 3;
                    }
                } else {
                    if (pos.X <= 0) {
                        i = 0;
                    } else {
                        i = 1;
                    }
                }
            }

            return pos;
        }
        
        public void draw(Matrix world) {
            Draw3D.cube(
                Vector3.Transform(A, world),
                Vector3.Transform(B, world),
                Vector3.Transform(C, world),
                Vector3.Transform(D, world),
                Vector3.Transform(E, world),
                Vector3.Transform(F, world),
                Vector3.Transform(G, world),
                Vector3.Transform(H, world),
                Color.MonoGameOrange);
        }
        public Vector3 support(Vector3 direction, Vector3 sweep) {
            if (sweep != Vector3.Zero) {
               return Supports.CubeSweep(direction, this, sweep);
            }
            return Supports.Cube(direction, this);
        }

    }
}
