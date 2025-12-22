using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Raven.Graphics;
using Raven.Graphics.Drawing3D;

namespace Raven.Engine.Collision {
    public struct collision_result {
        public bool solved = false;
        public int id_A, id_B;

        public int closest_iteration;
        public Vector3 closest_A;
        public Vector3 closest_B;
        public Vector3 AB => closest_B - closest_A;

        public float distance = float.MaxValue;

        public float distance_to_zero_A = float.MaxValue;
        public float distance_to_zero_B = float.MaxValue;

        public float penetration;
        public Vector3 penetration_normal;
        public Vector3 penetration_tangent_A;
        public Vector3 penetration_tangent_B;
        public Vector3 penetration_scalar => penetration_normal * (penetration + Math3D.big_epsilon);
        public bool intersects;
        public bool hit => intersects;
        public gjk_simplex end_simplex;
        public List<gjk_simplex> simplex_list = new List<gjk_simplex>();

        public bool draw_all_supports = true;

        public bool save_simplices = true;
        public int draw_simplex = 0;
        public polytope polytope;
        public Vector3 contact;

        public void save_simplex(ref gjk_simplex simplex) {
            if (!save_simplices) return;

            simplex_list.Add(simplex.copy());
            simplex.early_exit_reason = "";
        }
        public void save_simplex(gjk_simplex simplex, string reason) {
            if (!save_simplices) return;

            var gs = simplex.copy();
            gs.early_exit_reason = reason;
            simplex_list.Add(gs);
        }

        public collision_result() {
            distance = float.MaxValue;
            penetration = 0;

            intersects = false;

            closest_iteration = 0;

            id_A = -1;
            id_B = -1;

            closest_A = Vector3.Zero;
            closest_B = Vector3.Zero;
        }
        public volatile List<Vector3> sweep_points = new List<Vector3>();
        public Vector3 sweep_end = Vector3.Zero;
        public Vector3 sweep_slide = Vector3.Zero;
        public Vector3 sweep_slide_dir = Vector3.Zero;

        public void draw(Vector3 world_pos) {
            if (simplex_list == null) return;
            if (simplex_list != null && draw_simplex > -1 && draw_simplex < simplex_list.Count) {
                gjk_simplex simplex = simplex_list[draw_simplex];

                Draw3D.text_3D(State.spritebatch,
                    $"iter {simplex.iteration} | {draw_simplex + 1}/{simplex_list.Count} [{simplex.stage.ToString()}] {(intersects ? "[hit]" : "")}\n" +
                    $"{simplex.early_exit_reason}\n" +
                    $"[dir] {simplex.direction.ToXString()}\n" +
                    $"[dist] {Vector3.Distance(simplex.closest_A, simplex.closest_B)} [{distance}]\n" +
                    $"[denom] {simplex.get_denom()}\n" +
                    $"{simplex.get_info()}",

                    "profont", world_pos + Vector3.Down * 3f, State.camera.direction, 1f, Color.Black);

                //Draw3D.line(world_pos, world_pos + (simplex.direction) * 0.5f, Color.HotPink);
                //Draw3D.arrow(world_pos + simplex.A, world_pos + simplex.B, 0.1f, Color.HotPink);

                //simplex.draw();

                Draw3D.xyz_cross(simplex.closest_A, 0.5f, Color.GreenYellow);
                Draw3D.xyz_cross(simplex.closest_B, 0.5f, Color.GreenYellow);

                //Draw3D.line(simplex.closest_A, simplex.closest_B, Color.Red);

                //if (polytope != null)
                    //polytope.draw();
            }


            Draw3D.line(closest_A, closest_B, Color.Pink);

            Draw3D.xyz_cross(closest_A, 10f, Color.Red);
            Draw3D.xyz_cross(closest_B, 10f, Color.HotPink);

            Draw3D.sprite_line(closest_A, closest_A + (penetration_normal * penetration) * 5f, 0.02f, Color.Red);
            Draw3D.sprite_line(closest_B, closest_B + (penetration_normal * penetration) * 5f, 0.02f, Color.Green);
            Draw3D.sprite_line(closest_A, closest_A + (penetration_tangent_A*0.5f), 0.04f, Color.Purple);
            Draw3D.sprite_line(closest_A, closest_A + (penetration_tangent_B * 0.5f), 0.04f, Color.HotPink);



            lock (sweep_points) {
                var a = 0;
                foreach (var v in sweep_points) {
                    //Draw3D.cube(v, Vector3.One, Color.Red, Matrix.Identity);

                    Draw3D.xyz_cross(v, 5f, Color.GreenYellow);
                    a++;
                }
                if (sweep_points.Count > 0) {
                    Draw3D.cube(Vector3.Zero, Vector3.One, Color.MonoGameOrange, 
                        end_simplex.A_transform_direction * Matrix.CreateTranslation(sweep_points[0]));

                    Draw3D.sprite_line(sweep_points[0],
                        sweep_points[sweep_points.Count - 1], 0.04f, Color.Purple);
                    Draw3D.sprite_line(end_simplex.A_transform.Translation,
                        sweep_points[0], 0.04f, Color.HotPink);


                    Draw3D.cube(Vector3.Zero, Vector3.One, Color.Red,
                        end_simplex.A_transform_direction * Matrix.CreateTranslation(sweep_points[sweep_points.Count - 2]));

                    Draw3D.cube(Vector3.Zero, Vector3.One, Color.Blue, 
                        end_simplex.A_transform_direction * Matrix.CreateTranslation(sweep_points[sweep_points.Count - 1]));


                }
            }
        }
    }
}
