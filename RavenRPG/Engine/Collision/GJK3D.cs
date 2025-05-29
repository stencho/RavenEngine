using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using RavenRPG.Renderer;

namespace RavenRPG.Engine.Collision {

    public enum spoint { A=0, B=1, C=2, D=3 }

    public enum simplex_stage {
        empty = -1,
        point=0,
        line=1,
        triangle=2,
        tetrahedron=3
    }

    public struct gjk_support {
        public Vector3 A_support;
        public Vector3 B_support;

        public Vector3 P;

        public float barycentric;

        public gjk_support() {
            A_support = Vector3.Zero;
            B_support = Vector3.Zero;
            P = Vector3.Zero;
            barycentric = 0f;
        }

        public gjk_support(Vector3 a, Vector3 b, Vector3 p) {
            A_support = a;
            B_support = b;
            P = p;
            barycentric = 0f;
        }
    }

    public struct gjk_simplex {

        public Vector3 direction;

        public Vector3 closest_A = Vector3.Zero;
        public Vector3 closest_B = Vector3.Zero;

        public Vector3 sweep_A = Vector3.Zero;
        public Vector3 sweep_B = Vector3.Zero;

        public simplex_stage stage = simplex_stage.empty;

        public Matrix A_transform;
        public Matrix B_transform;

        public Matrix A_transform_direction;
        public Matrix B_transform_direction;

        public gjk_support[] supports = new gjk_support[4];

        public bool hit;

        public int iteration = 0;

        public string early_exit_reason;

        public Vector3 A => supports[A_index].P;
        public Vector3 B => supports[B_index].P;
        public Vector3 C => supports[C_index].P;
        public Vector3 D => supports[D_index].P;

        internal int A_index => (int)stage;
        internal int B_index => (int)stage - 1;
        internal int C_index => (int)stage - 2;
        internal int D_index => (int)stage - 3;

        public float A_bary => supports[A_index].barycentric;
        public float B_bary => supports[B_index].barycentric;
        public float C_bary => supports[C_index].barycentric;
        public float D_bary => supports[D_index].barycentric;

        public gjk_support A_support => supports[A_index];
        public gjk_support B_support => supports[B_index];
        public gjk_support C_support => supports[C_index];
        public gjk_support D_support => supports[D_index];


        public Vector3 AO => -A; public Vector3 BO => -B;
        public Vector3 CO => -C; public Vector3 DO => -D;

        public Vector3 AB => B - A; public Vector3 AC => C - A; public Vector3 AD => D - A;
        public Vector3 BA => A - B; public Vector3 BC => C - B; public Vector3 BD => D - B;
        public Vector3 CD => D - C; public Vector3 DB => B - D;

        public Vector3 ABC => Vector3.Cross(AB, AC);
        public Vector3 ADB => Vector3.Cross(AD, AB);
        public Vector3 ACD => Vector3.Cross(AC, AD);
        public Vector3 BCD => Vector3.Cross(BC, BD);

        public void add_new_point(Vector3 A_sup, Vector3 B_sup) {

            if ((int)stage < (int)simplex_stage.tetrahedron)
                stage = (simplex_stage)((int)stage + 1);
            else
                return;

            var P = A_sup - B_sup;
            supports[(int)stage] = new gjk_support(A_sup, B_sup, P);

        }

        int spoint_index(spoint p) {
            switch (p) {
                case spoint.A: return A_index;
                case spoint.B: return B_index;
                case spoint.C: return C_index;
                case spoint.D: return D_index;
            }
            return -1;
        }

        Vector3 spoint_value(spoint p) {
            switch (p) {
                case spoint.A: return supports[A_index].P;
                case spoint.B: return supports[B_index].P;
                case spoint.C: return supports[C_index].P;
                case spoint.D: return supports[D_index].P;
            }
            return Vector3.Zero;
        }

        float spoint_bary(spoint P) {
            switch (P) {
                case spoint.A: return supports[A_index].barycentric;
                case spoint.B: return supports[B_index].barycentric;
                case spoint.C: return supports[C_index].barycentric;
                case spoint.D: return supports[D_index].barycentric;
            }
            return 0f;
        }

        public void set_bary(spoint P, float bary) {
            switch (P) {
                case spoint.A:
                    supports[A_index].barycentric = bary;
                    break;
                case spoint.B:
                    supports[B_index].barycentric = bary;
                    break;
                case spoint.C:
                    supports[C_index].barycentric = bary;
                    break;
                case spoint.D:
                    supports[D_index].barycentric = bary;
                    break;
            }
        }
        public void set_bary(float U) {
            supports[A_index].barycentric = U;
        }
        public void set_bary_line(float U, float V) {
            supports[A_index].barycentric = U;
            supports[B_index].barycentric = V;
        }
        public void set_bary_tri(float U, float V, float W) {
            supports[A_index].barycentric = U;
            supports[B_index].barycentric = V;
            supports[C_index].barycentric = W;
        }
        public void set_bary_line((float U, float V) uv) {
            supports[A_index].barycentric = uv.U;
            supports[B_index].barycentric = uv.V;
        }


        public void set_bary_line() {
            var bc = CollisionHelper.line_barycentric(Vector3.Zero, A, B);
            supports[A_index].barycentric = bc.U;
            supports[B_index].barycentric = bc.V;
        }
        public void set_bary_tri() {
            var bc = CollisionHelper.triangle_barycentric(Vector3.Zero, A, B, C);
            supports[A_index].barycentric = bc.u;
            supports[B_index].barycentric = bc.v;
            supports[C_index].barycentric = bc.w;
        }

        public float get_denom() {
            float denom = 0f;
            switch (stage) {
                case simplex_stage.point:
                    denom = 1f;
                    break;
                case simplex_stage.line:
                    denom = A_support.barycentric + B_support.barycentric;
                    break;
                case simplex_stage.triangle:
                    denom = A_support.barycentric + B_support.barycentric + C_support.barycentric;
                    break;
                case simplex_stage.tetrahedron:
                    denom = A_support.barycentric + B_support.barycentric + C_support.barycentric + D_support.barycentric;
                    break;
            }
            denom = 1f / denom;
            return denom;
        }

        public void move_to_stage(spoint A) {
            var s = new gjk_support[4];
            s[0] = supports[spoint_index(A)];
            supports = s;
            stage = simplex_stage.point;
            s[0].barycentric = 1f;
        }
        public void move_to_stage(spoint A, spoint B) {
            var s = new gjk_support[4];
            s[1] = supports[spoint_index(A)];
            s[0] = supports[spoint_index(B)];

            supports = s;
            stage = simplex_stage.line;

        }
        public void move_to_stage(spoint A, spoint B, spoint C) {
            var s = new gjk_support[4];
            s[2] = supports[spoint_index(A)];
            s[1] = supports[spoint_index(B)];
            s[0] = supports[spoint_index(C)];

            supports = s;
            stage = simplex_stage.triangle;
        }

        public bool same_dir_as_AO(Vector3 P) {
            return Math3D.same_dir(P, AO);
        }

        public void set_dir_to_inverse_closest() {
            switch (stage) {
                case simplex_stage.point:
                    direction = AO;
                    break;
                case simplex_stage.line:
                    if (Vector3.Distance(Vector3.Zero, A) < Vector3.Distance(Vector3.Zero, B))
                        direction = AO;
                    else
                        direction = BO;
                    break;
                case simplex_stage.triangle:
                    if (Vector3.Distance(Vector3.Zero, A) < Vector3.Distance(Vector3.Zero, B))
                        if (Vector3.Distance(Vector3.Zero, A) < Vector3.Distance(Vector3.Zero, C))
                            direction = AO;
                        else
                            direction = CO;
                    else
                        direction = BO;
                    break;
                case simplex_stage.tetrahedron:
                    var a = Vector3.Distance(Vector3.Zero, A);

                    if (a < Vector3.Distance(Vector3.Zero, B))
                        if (a < Vector3.Distance(Vector3.Zero, C))
                            if (a < Vector3.Distance(Vector3.Zero, D))
                                direction = AO;
                            else
                                direction = DO;
                        else
                            direction = CO;
                    else
                        direction = BO;
                    break;
            }
        }


        public gjk_simplex() {
            A_transform_direction = Matrix.Identity;
            B_transform_direction = Matrix.Identity;

            direction = Vector3.Zero;

            early_exit_reason = "";
            hit = false;
        }

        public gjk_simplex copy() {
            return new gjk_simplex() {
                stage = stage,

                A_transform = A_transform,
                B_transform = B_transform,

                A_transform_direction = A_transform_direction,
                B_transform_direction = B_transform_direction,

                closest_A = closest_A,
                closest_B = closest_B,

                direction = direction,

                early_exit_reason = early_exit_reason,
                hit = hit,
                iteration = iteration,
                supports = supports
            };
        }

        public bool farthest_tet(spoint P) {
            if (stage != simplex_stage.tetrahedron) return false;
            var pos = spoint_value(P);

            for (int i = 0; i < 4; i++) {
                if (i == spoint_index(P))
                    continue;

                if (!Math3D.same_dir(pos - supports[i].P, pos)) {
                    return false;
                }

            }

            return true;
        }

        public string get_info() {
            StringBuilder sb = new StringBuilder();

            if ((int)stage >= 0) sb.Append($"[A] {A.ToXString()} [b] {supports[A_index].barycentric}\n");
            if ((int)stage >= 1) sb.Append($"[B] {B.ToXString()} [b] {supports[B_index].barycentric}\n");
            if ((int)stage >= 2) sb.Append($"[C] {C.ToXString()} [b] {supports[C_index].barycentric}\n");
            if ((int)stage >= 3) sb.Append($"[D] {D.ToXString()} [b] {supports[D_index].barycentric}\n");

            return sb.ToString();

        }

        public void draw() {
            if (supports == null)
                return;

            Draw3D.xyz_cross(A, 0.2f, Color.Red);
            if ((int)stage > 0) Draw3D.xyz_cross(B, 0.2f, Color.Green);
            if ((int)stage > 1) Draw3D.xyz_cross(C, 0.2f, Color.Blue);
            if ((int)stage > 2) Draw3D.xyz_cross(D, 0.2f, Color.Yellow);

            var mid = A;

            switch (stage) {
                case simplex_stage.point: break;

                case simplex_stage.line:
                    mid = (A + B) / 2f;

                    Draw3D.line(A, B, Color.HotPink);

                    Draw3D.line(
                        supports[(int)spoint.A].A_support,
                        supports[(int)spoint.B].A_support,
                        Color.HotPink);
                    Draw3D.line(
                        supports[(int)spoint.A].B_support,
                        supports[(int)spoint.B].B_support,
                        Color.HotPink);

                    break;

                case simplex_stage.triangle:
                    mid = (A + B + C) / 3f;

                    Draw3D.lines(Color.HotPink, A, B, C, A);

                    Draw3D.lines(Color.HotPink,
                        supports[(int)spoint.A].A_support,
                        supports[(int)spoint.B].A_support,
                        supports[(int)spoint.C].A_support,
                        supports[(int)spoint.A].A_support);

                    Draw3D.lines(Color.HotPink,
                        supports[(int)spoint.A].B_support,
                        supports[(int)spoint.B].B_support,
                        supports[(int)spoint.C].B_support,
                        supports[(int)spoint.A].B_support);
                    break;
                case simplex_stage.tetrahedron:
                    mid = (B + C + D) / 3f;

                    Draw3D.lines(Color.HotPink, C, B, D, C);

                    Draw3D.line(B, A, Color.Purple);
                    Draw3D.line(C, A, Color.Purple);
                    Draw3D.line(D, A, Color.Purple);


                    Draw3D.lines(Color.HotPink,
                        supports[(int)spoint.C].A_support,
                        supports[(int)spoint.B].A_support,
                        supports[(int)spoint.D].A_support,
                        supports[(int)spoint.C].A_support);

                    Draw3D.lines(Color.HotPink,
                        supports[(int)spoint.C].B_support,
                        supports[(int)spoint.B].B_support,
                        supports[(int)spoint.D].B_support,
                        supports[(int)spoint.C].B_support);


                    Draw3D.line(supports[(int)spoint.B].A_support, supports[(int)spoint.A].A_support, Color.Purple);
                    Draw3D.line(supports[(int)spoint.C].A_support, supports[(int)spoint.A].A_support, Color.Purple);
                    Draw3D.line(supports[(int)spoint.D].A_support, supports[(int)spoint.A].A_support, Color.Purple);

                    Draw3D.line(supports[(int)spoint.B].B_support, supports[(int)spoint.A].B_support, Color.Purple);
                    Draw3D.line(supports[(int)spoint.C].B_support, supports[(int)spoint.A].B_support, Color.Purple);
                    Draw3D.line(supports[(int)spoint.D].B_support, supports[(int)spoint.A].B_support, Color.Purple);


                    var m = (A + B + C) / 3f;
                    Draw3D.line(m, m + Vector3.Normalize(ABC), Color.Red);

                    m = (A + D + B) / 3f;
                    Draw3D.line(m, m + Vector3.Normalize(ADB), Color.Green);

                    m = (A + C + D) / 3f;
                    Draw3D.line(m, m + Vector3.Normalize(ACD), Color.Blue);


                    break;
            }

            Draw3D.arrow(mid, mid + direction, 0.2f, Color.HotPink);

        }
    }
    public class GJK {
        const int max_iterations = 35;
        public static collision_result swept_gjk_intersects_with_halving(Shape3D shape_A, Shape3D shape_B, Matrix w_a, Matrix w_b, Vector3 sweep_a, Vector3 sweep_b) {
            collision_result result = gjk_intersects(shape_A, shape_B, w_a, w_b, sweep_a, sweep_b);



            
            result.sweep_end = sweep_a;
            if (result.hit && result.penetration > Math3D.epsilon) {
                result.sweep_end += (result.penetration_normal * result.penetration);
            }

            result.sweep_slide = Vector3.Zero;
            result.sweep_slide_dir = Vector3.Zero;

            if (!result.hit || result.penetration <= Math3D.epsilon) {

                return result;
            }
            
            var sweep_points = new List<Vector3>();
            sweep_points.Add(w_a.Translation + sweep_a);
            var sweep_dir_a = Vector3.Normalize(sweep_a);
            var sweep_dist_a = sweep_a.Length();
            var last_a = 0f;
            var a_diff = Math.Abs(last_a - sweep_dist_a);
            var iterations = 0;


            bool hit = true;

            while (iterations < 12 || !hit) {
                if (hit) {
                    sweep_dist_a -= a_diff / 2f;
                } else {
                    sweep_dist_a += a_diff / 2f;
                }

                a_diff = Math.Abs(last_a - sweep_dist_a);
                hit = gjk_intersects_bool_only(shape_A, shape_B, w_a, w_b, sweep_dir_a * sweep_dist_a, sweep_b);

                sweep_points.Add(w_a.Translation + (sweep_dir_a * sweep_dist_a));

                if (a_diff / 2f <= Math3D.epsilon) { break; }

                last_a = sweep_dist_a;

                iterations++;
            }
            //result.penetration += sweep_dist_a;

            result = gjk_intersects(shape_A, shape_B, w_a, w_b, sweep_dir_a * sweep_dist_a, sweep_b);

            var st = 1-(sweep_dist_a / sweep_a.Length());
            var pd = CollisionHelper.project_direction_onto_plane(sweep_a, 
               result.penetration_tangent_A, 
               result.penetration_tangent_B);
            var cs = (sweep_dir_a * sweep_dist_a);

            if (result.intersects) {
                result.sweep_end = cs;
                result.sweep_slide = (w_a.Translation + (sweep_a - (pd * st))) - (w_a.Translation + result.sweep_end);
                if (result.sweep_slide.contains_nan()) {
                    result.sweep_slide = Vector3.Zero;
                }
                result.sweep_slide_dir = Vector3.Normalize(result.sweep_slide - result.sweep_end);

            } else {
                result.sweep_end = sweep_a;
                result.sweep_slide = Vector3.Zero;
                result.sweep_slide_dir = Vector3.Zero;
            }
            result.sweep_points = sweep_points;
            return result;
        }


        public static collision_result gjk_intersects(Shape3D shape_A, Shape3D shape_B, Matrix w_a, Matrix w_b) =>
            gjk_intersects(shape_A, shape_B, w_a, w_b, Vector3.Zero, Vector3.Zero);
        
        public static collision_result gjk_intersects(Shape3D shape_A, Shape3D shape_B, Matrix w_a, Matrix w_b, Vector3 sweep_a, Vector3 sweep_b) {
            collision_result result = new collision_result();

            gjk_simplex simplex = new gjk_simplex();

            Vector3 scale_a; Quaternion rot_a;
            Vector3 scale_b; Quaternion rot_b;

            w_a.Decompose(out scale_a, out rot_a, out _);
            w_b.Decompose(out scale_b, out rot_b, out _);

            simplex.A_transform = w_a;
            simplex.B_transform = w_b;

            if (sweep_a != Vector3.Zero)
                simplex.sweep_A = sweep_a;

            simplex.sweep_B = sweep_b;

            simplex.A_transform_direction = Matrix.CreateFromQuaternion(rot_a);
            simplex.B_transform_direction = Matrix.CreateFromQuaternion(rot_b);

            simplex.direction = w_b.Translation - w_a.Translation;
            //simplex.direction = Vector3.Up + Vector3.Right + Vector3.Forward;
            //simplex.direction = Vector3.Up;

            simplex.add_new_point(
                Vector3.Transform(
                    shape_A.support(
                        Vector3.Transform(simplex.direction, Matrix.Invert(simplex.A_transform_direction)), 
                        Vector3.Transform(sweep_a, Matrix.Invert(simplex.A_transform_direction))), 
                    w_a),
                Vector3.Transform(shape_B.support(Vector3.Transform(-simplex.direction, Matrix.Invert(simplex.B_transform_direction)), sweep_b), w_b));

            simplex.supports[0].barycentric = 1f;

            gjk_closest_point_calc(ref simplex, ref result, w_a, w_b);

            simplex.direction = simplex.AO;

            int iteration = 1;

            while (iteration < max_iterations) {
                simplex.add_new_point(
                    Vector3.Transform(
                        shape_A.support(
                            Vector3.Transform(simplex.direction, Matrix.Invert(simplex.A_transform_direction)),
                            Vector3.Transform(sweep_a, Matrix.Invert(simplex.A_transform_direction))),
                        w_a),
                        Vector3.Transform(shape_B.support(Vector3.Transform(-simplex.direction, Matrix.Invert(simplex.B_transform_direction)), sweep_b), w_b));

                simplex.iteration = iteration;

                ///////////////////////////////////////////////////////////
                if (simplex.stage == simplex_stage.line) { // *** LINE ***

                    result.save_simplex(ref simplex);

                    //gjk_closest_point_calc(ref simplex, ref result, w_a, w_b);

                    if (Vector3.Distance(simplex.A, simplex.B) <= Math3D.big_epsilon) {
                        simplex.move_to_stage(spoint.B);
                        gjk_closest_point_calc(ref simplex, ref result, w_a, w_b);
                        break;
                    }

                    if (CollisionHelper.line_closest_point(simplex.A, simplex.B, Vector3.Zero).Length() <= Math3D.big_epsilon) {
                        result.intersects = true;
                        simplex.set_bary_line();
                        break;
                    }

                    //origin between A and B
                    if (simplex.same_dir_as_AO(simplex.AB)) {
                        simplex.early_exit_reason = "Origin betweeen A and B";
                        simplex.direction = Vector3.Cross(Vector3.Cross(simplex.AB, simplex.AO), simplex.AB);

                    } else {
                        simplex.direction = simplex.AO;
                        simplex.move_to_stage(spoint.A);
                    }

                    ///////////////////////////////////////////////////////////
                } else if (simplex.stage == simplex_stage.triangle) { // *** TRIANGLE ***

                    result.save_simplex(ref simplex);

                    if (Vector3.Distance(simplex.A, simplex.B) <= Math3D.big_epsilon 
                        || Vector3.Distance(simplex.A, simplex.C) <= Math3D.big_epsilon) {
                        //result.save_simplex(simplex, "fart");
                        simplex.move_to_stage(spoint.B, spoint.C);
                        gjk_closest_point_calc(ref simplex, ref result, w_a, w_b);
                        break;
                    }


                    //On the ABC x AC plane, so origin could be closest to either AC or A
                    if (simplex.same_dir_as_AO(Vector3.Cross(simplex.ABC, simplex.AC))) {

                        if (simplex.same_dir_as_AO(simplex.AC)) {
                            simplex.early_exit_reason = "Tri -> AC";
                            simplex.direction = Vector3.Cross(Vector3.Cross(simplex.AC, simplex.AO), simplex.AC);

                            simplex.move_to_stage(spoint.A, spoint.C);

                        } else {
                            if (simplex.same_dir_as_AO(simplex.AB)) {
                                simplex.early_exit_reason = "Tri -> AB1";
                                simplex.direction = Vector3.Cross(Vector3.Cross(simplex.AB, simplex.AO), simplex.AB);

                                simplex.move_to_stage(spoint.A, spoint.B);


                            } else {
                                simplex.early_exit_reason = "Tri -> A1";
                                simplex.direction = simplex.AO;

                                simplex.move_to_stage(spoint.A);
                            }

                        } 
                    } else {
                        //On the AB x ABC plane, so we're either on AB or A
                        if (simplex.same_dir_as_AO(Vector3.Cross(simplex.AB, simplex.ABC))) {
                            if (simplex.same_dir_as_AO(simplex.AB)) {
                                simplex.early_exit_reason = "Tri -> AB2";
                                simplex.direction = Vector3.Cross(Vector3.Cross(simplex.AB, simplex.AO), simplex.AB);

                                simplex.move_to_stage(spoint.A, spoint.B);


                            } else {
                                simplex.early_exit_reason = "Tri -> A1";

                                simplex.direction = simplex.AO;
                                simplex.move_to_stage(spoint.A);
                            }


                        } else { // within plane
                            if (CollisionHelper.triangle_closest_point(simplex.A, simplex.B, simplex.C, Vector3.Zero).Length() <= Math3D.big_epsilon) {
                                result.intersects = true;
                                //simplex.move_to_stage(spoint.B, spoint.C);
                                result.save_simplex(ref simplex);
                                gjk_closest_point_calc(ref simplex, ref result, w_a, w_b);                               
                                break;
                            }

                            if (simplex.same_dir_as_AO(simplex.ABC)) {
                                simplex.early_exit_reason = "Tri -> ABC";
                                simplex.direction = simplex.ABC;
                            } else {
                                simplex.early_exit_reason = "Tri -> ACB";
                                simplex.direction = -simplex.ABC;
                                simplex.move_to_stage(spoint.A, spoint.C, spoint.B);
                            }
                        }
                    }


                    ///////////////////////////////////////////////////////////
                } else if (simplex.stage == simplex_stage.tetrahedron) { // *** TETRAHEDRON ***

                    result.save_simplex(ref simplex);

                    if (Vector3.Distance(simplex.A, simplex.B) <= Math3D.big_epsilon 
                        || Vector3.Distance(simplex.A, simplex.C) <= Math3D.big_epsilon 
                        || Vector3.Distance(simplex.A, simplex.D) <= Math3D.big_epsilon) {
                        simplex.move_to_stage(spoint.B, spoint.C, spoint.D);
                        gjk_closest_point_calc(ref simplex, ref result, w_a, w_b);
                        break;
                    }

                    bool ABC = simplex.same_dir_as_AO(simplex.ABC);
                    bool ACD = simplex.same_dir_as_AO(simplex.ACD);
                    bool ADB = simplex.same_dir_as_AO(simplex.ADB);
                    bool BCD = simplex.same_dir_as_AO(simplex.BCD);

                    if (ABC && ADB && ACD) {
                        simplex.early_exit_reason = "Tetra -> A";
                        simplex.direction = simplex.AO;
                        simplex.move_to_stage(spoint.A);

                    } else {
                        if (ABC && ADB) {
                            simplex.early_exit_reason = "Tetra -> AB";
                            simplex.direction = Vector3.Cross(Vector3.Cross(simplex.AB, simplex.AO), simplex.AB);
                            simplex.move_to_stage(spoint.A, spoint.B);

                        } else if (ABC && ACD) {
                            simplex.early_exit_reason = "Tetra -> AC";
                            simplex.direction = Vector3.Cross(Vector3.Cross(simplex.AC, simplex.AO), simplex.AC);

                            simplex.move_to_stage(spoint.A, spoint.C);

                        } else if (ACD && ADB) {
                            simplex.early_exit_reason = "Tetra -> AD";
                            simplex.direction = Vector3.Cross(Vector3.Cross(simplex.AD, simplex.AO), simplex.AD);

                            simplex.move_to_stage(spoint.A, spoint.D);

                        } else {
                            //face
                            if (ABC) {
                                simplex.early_exit_reason = "Tetra -> ABC";
                                simplex.direction = simplex.ABC;

                                simplex.move_to_stage(spoint.A, spoint.B, spoint.C);

                            } else if (ACD) {
                                simplex.early_exit_reason = "Tetra -> ACD";
                                simplex.direction = simplex.ACD;

                                simplex.move_to_stage(spoint.A, spoint.C, spoint.D);

                            } else if (ADB) {
                                simplex.early_exit_reason = "Tetra -> ADB";
                                simplex.direction = simplex.ADB;

                                simplex.move_to_stage(spoint.A, spoint.D, spoint.B);

                            } else {

                                if (!ABC && !ACD && !ADB && !BCD) {
                                    result.intersects = true;
                                    result.save_simplex(ref simplex);
                                    break;
                                }
              
                                simplex.early_exit_reason = "oh no";
                                break;
                            }
                        }
                    }

                }

                gjk_closest_point_calc(ref simplex, ref result, w_a, w_b);


                result.save_simplex(ref simplex);
                iteration++;
            }

            result.end_simplex = simplex;
            if (result.intersects) {
                result.distance = 0;
                if (simplex.stage == simplex_stage.tetrahedron)
                    result.polytope = EPA3D.expand_polytope(shape_A, shape_B, ref simplex, ref result);

                else if (simplex.stage == simplex_stage.triangle) {
                    simplex.add_new_point(
                        Vector3.Transform(shape_A.support(Vector3.Transform(simplex.ABC, Matrix.Invert(simplex.A_transform_direction)), sweep_a), w_a),
                        Vector3.Transform(shape_B.support(Vector3.Transform(-simplex.ABC, Matrix.Invert(simplex.B_transform_direction)), sweep_b), w_b));
                   
                    result.polytope = EPA3D.expand_polytope(shape_A, shape_B, ref simplex, ref result);

                    if (result.polytope.failed) {
                        simplex.add_new_point(
                            Vector3.Transform(shape_A.support(Vector3.Transform(-simplex.ABC, Matrix.Invert(simplex.A_transform_direction)), sweep_a), w_a),
                            Vector3.Transform(shape_B.support(Vector3.Transform(simplex.ABC, Matrix.Invert(simplex.B_transform_direction)), sweep_b), w_b));

                        result.polytope = EPA3D.expand_polytope(shape_A, shape_B, ref simplex, ref result);

                    }
                    
                } else if (simplex.stage == simplex_stage.line) {
                    var dA = Vector3.Distance(Vector3.Zero, simplex.A);
                    var dB = Vector3.Distance(Vector3.Zero, simplex.B);

                    if (dA < dB ) {
                        result.penetration_normal = Vector3.Normalize(simplex.A);
                        result.penetration = dA;
                    } else {
                        result.penetration_normal = Vector3.Normalize(simplex.B);
                        result.penetration = dB;
                    }
                }
            }

            if (result.penetration_normal == Vector3.Zero || result.penetration == 0f) { 
                result.intersects = false;
            }

            return result;
        }




        public static bool gjk_intersects_bool_only(Shape3D shape_A, Shape3D shape_B, Matrix w_a, Matrix w_b, Vector3 sweep_a, Vector3 sweep_b) {
            gjk_simplex simplex = new gjk_simplex();

            Vector3 scale_a; Quaternion rot_a;
            Vector3 scale_b; Quaternion rot_b;

            w_a.Decompose(out scale_a, out rot_a, out _);
            w_b.Decompose(out scale_b, out rot_b, out _);

            simplex.A_transform = w_a;
            simplex.B_transform = w_b;

            simplex.sweep_A = sweep_a;
            simplex.sweep_B = sweep_b;

            simplex.A_transform_direction = Matrix.CreateFromQuaternion(rot_a);
            simplex.B_transform_direction = Matrix.CreateFromQuaternion(rot_b);

            simplex.direction = w_b.Translation - w_a.Translation;
            //simplex.direction = Vector3.Up;

            simplex.add_new_point(
                Vector3.Transform(
                    shape_A.support(
                        Vector3.Transform(simplex.direction, Matrix.Invert(simplex.A_transform_direction)),
                        Vector3.Transform(sweep_a, Matrix.Invert(simplex.A_transform_direction))),
                    w_a),
                Vector3.Transform(shape_B.support(Vector3.Transform(-simplex.direction, Matrix.Invert(simplex.B_transform_direction)), sweep_b), w_b));

            simplex.direction = simplex.AO;
            //simplex.direction = w_b.Translation - w_a.Translation;
            //simplex.direction = Vector3.Up + Vector3.Right + Vector3.Forward;
            simplex.direction = Vector3.Up;

            int iteration = 1;

            while (iteration < max_iterations) {
                simplex.add_new_point(
                    Vector3.Transform(
                        shape_A.support(
                            Vector3.Transform(simplex.direction, Matrix.Invert(simplex.A_transform_direction)),
                            Vector3.Transform(sweep_a, Matrix.Invert(simplex.A_transform_direction))),
                        w_a),
                        Vector3.Transform(shape_B.support(Vector3.Transform(-simplex.direction, Matrix.Invert(simplex.B_transform_direction)), sweep_b), w_b));

                simplex.iteration = iteration;

                ///////////////////*** LINE ***////////////////////////////////////////// 
                if (simplex.stage == simplex_stage.line) {
                    if (Vector3.Distance(simplex.A, simplex.B) <= Math3D.big_epsilon) {
                        simplex.move_to_stage(spoint.B);
                        return false;
                    }

                    if (CollisionHelper.line_closest_point(simplex.A, simplex.B, Vector3.Zero).Length() <= Math3D.big_epsilon)
                        return true;


                    //origin between A and B
                    if (simplex.same_dir_as_AO(simplex.AB))
                        simplex.direction = Vector3.Cross(Vector3.Cross(simplex.AB, simplex.AO), simplex.AB);
                    else {
                        simplex.direction = simplex.AO;
                        simplex.move_to_stage(spoint.A);
                    }


                    //////////////*** TRIANGLE ***/////////////////////////////////////////////// 
                } else if (simplex.stage == simplex_stage.triangle) {

                    if (Vector3.Distance(simplex.A, simplex.B) <= Math3D.big_epsilon
                        || Vector3.Distance(simplex.A, simplex.C) <= Math3D.big_epsilon) {
                        simplex.move_to_stage(spoint.B, spoint.C);
                        return false;
                    }

                    //On the ABC x AC plane, so origin could be closest to either AC or A
                    if (simplex.same_dir_as_AO(Vector3.Cross(simplex.ABC, simplex.AC))) {
                        if (simplex.same_dir_as_AO(simplex.AC)) {
                            simplex.direction = Vector3.Cross(Vector3.Cross(simplex.AC, simplex.AO), simplex.AC);
                            simplex.move_to_stage(spoint.A, spoint.C);

                        } else {
                            if (simplex.same_dir_as_AO(simplex.AB)) {
                                simplex.direction = Vector3.Cross(Vector3.Cross(simplex.AB, simplex.AO), simplex.AB);
                                simplex.move_to_stage(spoint.A, spoint.B);
                            } else {
                                simplex.direction = simplex.AO;
                                simplex.move_to_stage(spoint.A);
                            }
                        }
                    } else {
                        //On the AB x ABC plane, so we're either on AB or A
                        if (simplex.same_dir_as_AO(Vector3.Cross(simplex.AB, simplex.ABC))) {
                            if (simplex.same_dir_as_AO(simplex.AB)) {
                                simplex.direction = Vector3.Cross(Vector3.Cross(simplex.AB, simplex.AO), simplex.AB);
                                simplex.move_to_stage(spoint.A, spoint.B);
                            } else {
                                simplex.direction = simplex.AO;
                                simplex.move_to_stage(spoint.A);
                            }
                        } else { // within plane
                            if (CollisionHelper.triangle_closest_point(simplex.A, simplex.B, simplex.C, Vector3.Zero).Length() <= Math3D.big_epsilon)
                                return true;

                            if (simplex.same_dir_as_AO(simplex.ABC)) {
                                simplex.direction = simplex.ABC;
                            } else {
                                simplex.direction = -simplex.ABC;
                                simplex.move_to_stage(spoint.A, spoint.C, spoint.B);
                            }
                        }
                    }


                    ///////////////////////////////////////////////////////////
                } else if (simplex.stage == simplex_stage.tetrahedron) { // *** TETRAHEDRON ***
                    if (Vector3.Distance(simplex.A, simplex.B) <= Math3D.big_epsilon
                        || Vector3.Distance(simplex.A, simplex.C) <= Math3D.big_epsilon
                        || Vector3.Distance(simplex.A, simplex.D) <= Math3D.big_epsilon) {
                        simplex.move_to_stage(spoint.B, spoint.C, spoint.D);
                        return false;
                    }

                    bool ABC = simplex.same_dir_as_AO(simplex.ABC);
                    bool ACD = simplex.same_dir_as_AO(simplex.ACD);
                    bool ADB = simplex.same_dir_as_AO(simplex.ADB);
                    bool BCD = simplex.same_dir_as_AO(simplex.BCD);

                    if (ABC && ADB && ACD) {
                        simplex.direction = simplex.AO;
                        simplex.move_to_stage(spoint.A);
                        //break; // ????????? 

                    } else {
                        if (ABC && ADB) {
                            simplex.direction = Vector3.Cross(Vector3.Cross(simplex.AB, simplex.AO), simplex.AB);
                            simplex.move_to_stage(spoint.A, spoint.B);
                        } else if (ABC && ACD) {
                            simplex.direction = Vector3.Cross(Vector3.Cross(simplex.AC, simplex.AO), simplex.AC);
                            simplex.move_to_stage(spoint.A, spoint.C);
                        } else if (ACD && ADB) {
                            simplex.direction = Vector3.Cross(Vector3.Cross(simplex.AD, simplex.AO), simplex.AD);
                            simplex.move_to_stage(spoint.A, spoint.D);
                        } else { //face
                            if (ABC) {
                                simplex.direction = simplex.ABC; simplex.move_to_stage(spoint.A, spoint.B, spoint.C);
                            } else if (ACD) {
                                simplex.direction = simplex.ACD; simplex.move_to_stage(spoint.A, spoint.C, spoint.D);
                            } else if (ADB) {
                                simplex.direction = simplex.ADB; simplex.move_to_stage(spoint.A, spoint.D, spoint.B);
                            } else {
                                if (!ABC && !ACD && !ADB && !BCD)
                                    return true;

                                return false;
                            }
                        }
                    }
                }
                iteration++;
            }

            return false;
        }




        static void gjk_closest_point_calc(ref gjk_simplex simplex, ref collision_result result, Matrix w_a, Matrix w_b) {

            Vector3 closest_A = Vector3.Zero;
            Vector3 closest_B = Vector3.Zero;

            switch (simplex.stage) {
                case simplex_stage.line:
                    simplex.set_bary_line();
                    break;
                case simplex_stage.triangle:

                    simplex.set_bary_tri();
                    break;
                case simplex_stage.tetrahedron:
                    var bary = CollisionHelper.tetrahedron_barycentric(Vector3.Zero, simplex.A, simplex.B, simplex.C, simplex.D);

                    simplex.set_bary(spoint.A, bary.U);
                    simplex.set_bary(spoint.B, bary.V);
                    simplex.set_bary(spoint.C, bary.W);
                    simplex.set_bary(spoint.D, bary.Z);
                    break;
            }

            float d = float.MaxValue;
            float dot = float.MaxValue;
            float denom = simplex.get_denom();

            switch (simplex.stage) {
                case simplex_stage.point:
                    closest_A = simplex.A_support.A_support;
                    closest_B = simplex.A_support.B_support;

                    break;
                case simplex_stage.line:

                    var l_AS = denom * simplex.A_bary;
                    var l_BS = denom * simplex.B_bary;

                    closest_A = (simplex.A_support.A_support * l_AS) + (simplex.B_support.A_support * l_BS);
                    closest_B = (simplex.A_support.B_support * l_AS) + (simplex.B_support.B_support * l_BS);


                    dot = Vector3.Dot(closest_A, closest_B);
                    break;
                case simplex_stage.triangle:
                    //closest_A = CollisionHelper.triangle_closest_point(simplex.A_support.A_support, simplex.B_support.A_support, simplex.C_support.A_support, Vector3.Zero);
                    //closest_B = CollisionHelper.triangle_closest_point(simplex.A_support.B_support, simplex.B_support.B_support, simplex.C_support.B_support, Vector3.Zero);

                    var t_AS = denom * simplex.A_bary;
                    var t_BS = denom * simplex.B_bary;
                    var t_CS = denom * simplex.C_bary;

                    closest_A = (simplex.A_support.A_support * t_AS) + (simplex.B_support.A_support * t_BS) + (simplex.C_support.A_support * t_CS);
                    closest_B = (simplex.A_support.B_support * t_AS) + (simplex.B_support.B_support * t_BS) + (simplex.C_support.B_support * t_CS);

                    //closest_A = Vector3.Transform(closest_A, (simplex.A_transform));
                    //closest_B = Vector3.Transform(closest_B, (simplex.B_transform));

                    dot = Vector3.Dot(closest_A, closest_B);
                    break;
                case simplex_stage.tetrahedron:

                    closest_A =
                        (simplex.A_support.A_support * (denom * simplex.A_bary)) +
                        (simplex.B_support.A_support * (denom * simplex.B_bary)) +
                        (simplex.C_support.A_support * (denom * simplex.C_bary)) +
                        (simplex.D_support.A_support * (denom * simplex.D_bary));

                    //closest_A = Vector3.Transform(closest_A, (simplex.A_transform));

                    closest_B = closest_A;
                    break;

            }

            d = Vector3.Distance(closest_A, closest_B);

            var da = closest_A.Length();
            var db = closest_B.Length();

            simplex.closest_A = closest_A;
            simplex.closest_B = closest_B;

            if (da < result.distance_to_zero_A) {
                result.distance_to_zero_A = da;
            }
            if (db < result.distance_to_zero_B) {
                result.distance_to_zero_B = db;
            }

            if (d < result.distance && d > 0) {
                result.distance = d;

                result.closest_A = closest_A;
                result.closest_B = closest_B;

                result.closest_iteration = simplex.iteration;
            }

        }
    }
}