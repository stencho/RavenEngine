using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Raven.Engine;
using Raven.Graphics.Drawing3D;

namespace Raven.Graphics.Particles {
    public class PointCloud {
        public class point_in_cloud {
            public Vector3 point_previous;
            public Vector3 point;

            public bool lerp = false;
            public Vector3 lerp_to;
            public float lerp_speed;

            public Matrix orientation;

            public bool alive = true;
            public SpriteEffects flipmode = SpriteEffects.None;
            public Color tint = Color.White;
            public float scale = 1f;
            //public float opacity = 1f;

            public point_in_cloud(Vector3 p) { 
                point = p;
                point_previous = p;
                orientation = Matrix.Identity; }

            public Action<point_in_cloud, PointCloud> behaviour;

            public void update(PointCloud parent) {
                if (!alive) return;
                                
                if (lerp) {
                    point = Vector3.LerpPrecise(point, lerp_to, lerp_speed);
                }

                behaviour(this, parent);
            }

        }

        public point_in_cloud[] points;
        public InstanceData[] vert_positions;
        float[] distances;
        
        int _point_count;
        public int point_count => _point_count;

        public PointCloud(int points) {
            this.points = new point_in_cloud[points];
            vert_positions = new InstanceData[points];
            distances = new float[points];
            _point_count = points;

            create_random_on_cube(Vector3.Up * 65f + (Vector3.Forward * 70), 25f);

            //create_uniform_on_line(Vector3.Up * 65f + (Vector3.Forward * 70), Vector3.Up * 65f);
        }
        public PointCloud(int points, Action<point_in_cloud, PointCloud> point_brain) {
            this.points = new point_in_cloud[points];
            vert_positions = new InstanceData[points];
            distances = new float[points];
            _point_count = points;

            create_random_within_cube(Vector3.Up * 65f + (Vector3.Forward * 70), 25f);

            for (int i = 0; i < this.points.Length; i++) {
                this.points[i].scale = RNG.rng_float;
                this.points[i].behaviour = point_brain;
            }
            //create_uniform_on_line(Vector3.Up * 65f + (Vector3.Forward * 70), Vector3.Up * 65f);
        }

        public void update() {
            for (int i = 0; i < points.Length; i++) {
                points[i].update(this);
            }
        }

        public void draw_debug() {
            for (int i = 0; i < points.Length; i++) {
               // Draw3D.xyz_cross(points[i].point, 0.1f, Color.Red);
               // Draw3D.line(points[i].point, points[i].point_previous, Color.DeepPink);
            }
        }

        void create_random_within_cube(Vector3 center, float size) {
            for (int i = 0; i < points.Length; i++) {
                points[i] = new point_in_cloud(center + new Vector3(
                    RNG.rng_float_neg_one_to_one * size,
                    RNG.rng_float_neg_one_to_one * size,
                    RNG.rng_float_neg_one_to_one * size));
            }
        }
        void create_random_on_cube(Vector3 center, float size) {
            for (int i = 0; i < points.Length; i++) {
                var v = Vector3.Zero;

                if (RNG.rng_bool) { //on X
                    if (RNG.rng_bool) { // pos
                        v.X += size;
                    } else {
                        v.X -= size;
                    }

                    v.Y = RNG.rng_float_neg_one_to_one * size;
                    v.Z = RNG.rng_float_neg_one_to_one * size;

                } else if (RNG.rng_bool) { //on y
                    if (RNG.rng_bool) { // pos
                        v.Y += size;
                    } else {
                        v.Y -= size;
                    }

                    v.X = RNG.rng_float_neg_one_to_one * size;
                    v.Z = RNG.rng_float_neg_one_to_one * size;
                } else { //on z
                    if (RNG.rng_bool) { // pos
                        v.Z += size;
                    } else {
                        v.Z -= size;
                    }

                    v.Y = RNG.rng_float_neg_one_to_one * size;
                    v.X = RNG.rng_float_neg_one_to_one * size;
                }

                points[i] = new point_in_cloud(v + center);

            }
        }


        void create_random_on_sphere(Vector3 center, float radius) {
            for (int i = 0; i < points.Length; i++) {
                points[i] = new point_in_cloud(center + (Vector3.Normalize(RNG.rng_v3_neg_one_to_one) * radius));
            }
        }
        void create_random_within_sphere(Vector3 center, float radius) {
            for (int i = 0; i < points.Length; i++) {
                var r = center + (RNG.rng_v3_neg_one_to_one * radius);
                while (Vector3.Distance(center, r) > radius) {
                    r = center + (RNG.rng_v3_neg_one_to_one * radius);
                }

                points[i] = new point_in_cloud(r);
            }
        }

        void create_random_on_line(Vector3 A, Vector3 B) {
            var AB = B-A;
            for (int i = 0; i < points.Length; i++) {
                points[i] = new point_in_cloud(A + (Vector3.Normalize(AB) * (AB.Length() * RNG.rng_float))) ;
            }
        }
        void create_uniform_on_line(Vector3 A, Vector3 B) {
            var AB = B-A;
            var len = AB.Length() / points.Length; 

            for (int i = 0; i < points.Length; i++) {
                points[i] = new point_in_cloud(A + (Vector3.Normalize(AB) * (len * i)));
            }
        }

        public DynamicVertexBuffer instance_buffer;

        public void GenerateWorldMatrixBuffer(Camera camera) {
            //for (int i = 0; i < _point_count; i++) {
            int i = 0;
            
            
            foreach(point_in_cloud p in points.OrderByDescending(a => Vector3.Distance(camera.position, a.point))) { 
                p.orientation = Matrix.CreateScale(p.scale) * Matrix.CreateBillboard(p.point, camera.position, camera.orientation.Up, camera.orientation.Forward);

                p.orientation.r1(out vert_positions[i].r1.X, out vert_positions[i].r1.Y, out vert_positions[i].r1.Z, out vert_positions[i].r1.W);
                p.orientation.r2(out vert_positions[i].r2.X, out vert_positions[i].r2.Y, out vert_positions[i].r2.Z, out vert_positions[i].r2.W);
                p.orientation.r3(out vert_positions[i].r3.X, out vert_positions[i].r3.Y, out vert_positions[i].r3.Z, out vert_positions[i].r3.W);
                p.orientation.r4(out vert_positions[i].r4.X, out vert_positions[i].r4.Y, out vert_positions[i].r4.Z, out vert_positions[i].r4.W);

                //mtmp = Matrix.Transpose(Matrix.Invert(mtmp));
                //mtmp.r1(out vert_positions[i].r1_IT.X, out vert_positions[i].r1_IT.Y, out vert_positions[i].r1_IT.Z, out vert_positions[i].r1_IT.W);
                //mtmp.r2(out vert_positions[i].r2_IT.X, out vert_positions[i].r2_IT.Y, out vert_positions[i].r2_IT.Z, out vert_positions[i].r2_IT.W);
                //mtmp.r3(out vert_positions[i].r3_IT.X, out vert_positions[i].r3_IT.Y, out vert_positions[i].r3_IT.Z, out vert_positions[i].r3_IT.W);
                //mtmp.r4(out vert_positions[i].r4_IT.X, out vert_positions[i].r4_IT.Y, out vert_positions[i].r4_IT.Z, out vert_positions[i].r4_IT.W);


                vert_positions[i].tint = Color.White; // Color.FromNonPremultiplied((int)p.point.X, (int)p.point.Y, (int)p.point.Z, 255);
                vert_positions[i].normal = Vector3.Normalize(p.orientation.Forward);

                i++;
            }            

            instance_buffer = new DynamicVertexBuffer(State.graphics_device, InstanceDataDec.VertexDeclaration, vert_positions.Length, BufferUsage.WriteOnly);
            instance_buffer.SetData(vert_positions);
            
        }

    }
}
