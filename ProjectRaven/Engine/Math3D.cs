using System.Collections.Generic;
using Microsoft.Xna.Framework;
using ProjectRaven.Engine.Collision;
using ProjectRaven.Engine.Collision.Shapes3D;

namespace ProjectRaven.Engine {

    public static class Math3D {
        public const float epsilon = 1e-6f;
        public const float big_epsilon = 0.001f;
        public static bool close_enough(Vector3 A, Vector3 B) { return Vector3.Distance(A, B) <= big_epsilon; }
        public static bool same_dir(Vector3 direction, Vector3 origin_dir) {
            var vd = Vector3.Dot(direction, origin_dir);
            return (vd >= 0f);
        }
        public static bool perpendicular(Vector3 direction, Vector3 direction2) {
            var vd = Vector3.Dot(direction, direction2);
            return (vd > -.250f && vd < .250f);
        }

        public static Vector3 highest_dot(Vector3[] verts, Vector3 direction, out float dot) {
            dot = float.MinValue;
            Vector3 v = Vector3.Zero;

            for (int i = 0; i < verts.Length; i++) {
                float d = Vector3.Dot(direction, verts[i]);

                if (d > dot) {
                    dot = d;
                    v = verts[i];
                }
            }

            return v;
        }

        public static Vector3 CrossCross(this Vector3 a, Vector3 b) {
            return Vector3.Cross(Vector3.Cross(a, b), a);
        }
    }

    public static class CollisionHelper {
        public const float epsilon = 1e-6f;


        public static bool point_inside_plane_facing(Vector3 A, Vector3 B, Vector3 C, Vector3 D, Vector3 P) { return (Vector3.Dot(P - A, Vector3.Cross(B - A, C - A)) * Vector3.Dot(D - A, Vector3.Cross(B - A, C - A))) > 0f; }


        public static bool point_inside_tetrahedron(Vector3 A, Vector3 B, Vector3 C, Vector3 D, Vector3 point) {
            var t = point_inside_plane_facing(A, B, C, D, Vector3.Zero);
            var t2 = point_inside_plane_facing(A, B, D, C, Vector3.Zero);
            var t3 = point_inside_plane_facing(B, D, C, A, Vector3.Zero);
            var t4 = point_inside_plane_facing(A, C, D, B, Vector3.Zero);
            return (t && t2 && t3 && t4);
        }


        #region LINE
        public static Vector3 line_closest_point(Vector3 a, Vector3 b, Vector3 point) {
            var ab = b - a;
            var t = Vector3.Dot(point - a, ab) / Vector3.Dot(ab, ab);

            if (t < 0) t = 0;
            if (t > 1) t = 1;

            return a + t * ab;
        }
        public static float lines_closest_points(Vector3 AA, Vector3 AB, Vector3 BA, Vector3 BB, out float s, out float t, out Vector3 P1, out Vector3 P2) {
            Vector3 d1 = AB - AA;
            Vector3 d2 = BB - BA;
            Vector3 r = AA - BA;

            float a = Vector3.Dot(d1, d1);
            float e = Vector3.Dot(d2, d2);
            float f = Vector3.Dot(d2, r);

            if (a <= epsilon && e <= epsilon) {
                s = t = 0.0f;
                P1 = AA;
                P2 = BA;
                return Vector3.Dot(P1, P2);
            }

            if (a <= epsilon) {
                s = 0.0f;
                t = f / e;
                t = MathHelper.Clamp(t, 0.0f, 1.0f);
            } else {
                float c = Vector3.Dot(d1, r);
                if (e <= epsilon) {
                    t = 0.0f;
                    s = MathHelper.Clamp(-c / a, 0f, 1f);
                } else {
                    float b = Vector3.Dot(d1, d2);
                    float denom = a * e - b * b;

                    if (denom != 0f) {
                        s = MathHelper.Clamp((b * f - c * e) / denom, 0f, 1f);
                    } else s = 0f;

                    t = (b * s + f) / e;

                    if (t < 0f) {
                        t = 0f;
                        s = MathHelper.Clamp(-c / a, 0f, 1f);
                    } else if (t > 1f) {
                        t = 1f;
                        s = MathHelper.Clamp((b - c) / a, 0f, 1f);
                    }
                }
            }

            P1 = AA + d1 * s;
            P2 = BA + d2 * t;

            return Vector3.Dot(P1 - P2, P1 - P2);
        }

        public static (float U, float V) line_barycentric(Vector3 P, Vector3 A, Vector3 B) {
            Vector3 AB = B - A;
            Vector3 AP = P - A;
            float dotABAB = Vector3.Dot(AB, AB);
            float dotACAB = Vector3.Dot(AP, AB);
            float u = dotACAB / dotABAB;
            float v = 1 - u;
            return (u, v);
        }
        #endregion

        #region TRIANGLE
        public static Vector3 triangle_normal(Vector3 A, Vector3 B, Vector3 C) {
            var AB = B-A; var AC = C-A;
            return Vector3.Normalize(Vector3.Cross(AB, AC)); //ACAB
        }

        /*
        public static (float u, float v, float w) triangle_barycentric(Vector3 P, Vector3 A, Vector3 B, Vector3 C) {
            Vector3 AB = B - A;
            Vector3 AC = C - A;
            Vector3 AP = P - A;

            float abab = Vector3.Dot(AB, AB);
            float abac = Vector3.Dot(AB, AC);
            float acac = Vector3.Dot(AC, AC);
            float apab = Vector3.Dot(AP, AB);
            float apac = Vector3.Dot(AP, AC);

            float denom = abab * acac - (abac * abac);

            (float u, float v, float w) output;
            output.v = (acac * apab - abac * apac) / denom;
            output.w = (abab * apac - abac * apab) / denom;
            output.u = 1f - output.v - output.w;
            return output;
        }
        */
        public static (float u, float v, float w) triangle_barycentric(Vector3 P, Vector3 A, Vector3 B, Vector3 C) {
            Vector3 v0 = B - A;
            Vector3 v1 = C - A;
            Vector3 v2 = P - A;

            float f0 = Vector3.Dot(v0, v0);
            float f1 = Vector3.Dot(v0, v1);
            float f2 = Vector3.Dot(v1, v1);
            float f3 = Vector3.Dot(v2, v0);
            float f4 = Vector3.Dot(v2, v1);

            float denom = f0 * f2 - f1 * f1;
            (float u, float v, float w) output = (0f,0f,0f);

            output.v = (f2 * f3 - f1 * f4) / denom;
            output.w = (f0 * f4 - f1 * f3) / denom;
            output.u = 1.0f - output.v - output.w;

            return output;
        }

        public static Vector3 triangle_closest_point(Vector3 A, Vector3 B, Vector3 C, Vector3 point) {
            Vector3 ab = B - A;
            Vector3 ac = C - A;
            Vector3 ap = point - A;

            //past A
            float abap = Vector3.Dot(ab, ap); //ain't talkin bout no wii
            float acap = Vector3.Dot(ac, ap);

            if (abap <= 0f && acap <= 0f) return A;

            //past B
            Vector3 bp = point - B;

            float abbp = Vector3.Dot(ab, bp);
            float acbp = Vector3.Dot(ac, bp);

            if (abbp >= 0f && acbp <= 0f) return B;

            //between A and B
            float vc = abap * acbp - abbp * acap;
            float v, w;
            if (vc <= 0f && abap >= 0f && abbp <= 0f) {
                v = abap / (abap - abbp);
                return A + v * ab;
            }

            //past C  
            Vector3 cp = point - C;
            float abcp = Vector3.Dot(ab, cp);
            float accp = Vector3.Dot(ac, cp);

            if (accp >= 0f && abcp <= accp) return C;

            //between A and C
            float vb = abcp * acap - abap * accp;
            if (vb <= 0f && acap >= 0f && accp <= 0f) {
                w = acap / (acap - accp);
                return A + w * ac;
            }

            //between B and C
            float va = abbp * accp - abcp * acbp;
            if (va <= 0f && (acbp - abbp) >= 0f && (abcp - accp) >= 0f) {
                w = (acbp - abbp) / ((acbp - abbp) + (abcp - accp));
                return B + w * (C - B);
            }

            //on face
            float denom = 1.0f / (va + vb + vc);
            v = vb * denom;
            w = vc * denom;

            return A + ab * v + ac * w;
        }

        public static Vector3 triangle_closest_point_alternative(Vector3 A, Vector3 B, Vector3 C, Vector3 point) {
            Vector3 ab = B - A;
            Vector3 ac = C - A;
            Vector3 bc = C - B;

            float unom = Vector3.Dot(point - B, bc);
            float sdnom = Vector3.Dot(point - B, A - B);
            if (sdnom <= 0f && unom <= 0f) return B;
            float tdnom = Vector3.Dot(point - C, A - C);
            float udnom = Vector3.Dot(point - C, B - C);
            if (tdnom <= 0f && udnom <= 0f) return C;


            float snom = Vector3.Dot(point - A, ab);
            float tnom = Vector3.Dot(point - A, ac);

            Vector3 n = Vector3.Cross(ab, ac);
            float vc = Vector3.Dot(n, Vector3.Cross(A - point, B - point));
            if (vc <= 0f && snom >= 0f && sdnom >= 0f)
                return A + snom / (snom + sdnom) * ab;

            float va = Vector3.Dot(n, Vector3.Cross(B - point, C - point));

            if (va <= 0f && unom >= 0f && udnom >= 0f)
                return B + unom / (unom + udnom) * bc;

            float vb = Vector3.Dot(n, Vector3.Cross(C - point, A - point));

            if (vb <= 0f && tnom >= 0f && tdnom >= 0f)
                return A + tnom / (tnom + tdnom) * ac;

            float u = va / (va + vb + vc);
            float v = vb / (va + vb + vc);
            float w = 1.0f - u - v; // = vc / (va + vb + vc)

            return u * A + v * B + w * C;
        }

        public static Vector3 triangle_farthest_point(Vector3 A, Vector3 B, Vector3 C, Vector3 dir) {
            float dotA = Vector3.Dot(A, dir);
            float dotB = Vector3.Dot(B, dir);
            float dotC = Vector3.Dot(C, dir);

            float dotAB = Vector3.Dot(B - A, dir);
            float dotBC = Vector3.Dot(C - B, dir);
            float dotCA = Vector3.Dot(A - C, dir);

            Vector3 farthestPoint;
            float maxDot = dotA;
            if (dotB > maxDot) {
                maxDot = dotB;
                farthestPoint = B;
            } else {
                farthestPoint = A;
            }
            if (dotC > maxDot) {
                maxDot = dotC;
                farthestPoint = C;
            }
            if (dotAB > maxDot) {
                maxDot = dotAB;
                farthestPoint = line_closest_point(A, B, dir);
            }
            if (dotBC > maxDot) {
                maxDot = dotBC;
                farthestPoint = line_closest_point(B, C, dir);
            }
            if (dotCA > maxDot) {
                maxDot = dotCA;
                farthestPoint = line_closest_point(C, A, dir);
            }

            return farthestPoint;
        }
        #endregion

        #region TETRAHEDRON
        public static (float U, float V, float W, float Z) tetrahedron_barycentric(Vector3 point, Vector3 A, Vector3 B, Vector3 C, Vector3 D) {
            Vector3 AB = B - A;
            Vector3 AC = C - A;
            Vector3 AD = D - A;
            Vector3 AP = point - A;

            float V = Vector3.Dot(Vector3.Cross(AB, AC), AD);
            float va = Vector3.Dot(Vector3.Cross(AC, AD), AP) / V;
            float vb = Vector3.Dot(Vector3.Cross(AD, AB), AP) / V;
            float vc = Vector3.Dot(Vector3.Cross(AB, AC), AP) / V;
            float vd = 1 - va - vb - vc;

            return (va, vb, vc, vd);
        }
        #endregion

        #region QUAD
        public static Vector3 closest_point_on_quad(Vector3 A, Vector3 B, Vector3 C, Vector3 D, Vector3 point) {
            Vector3 ABD_closest = CollisionHelper.triangle_closest_point(A, B, D, point);
            Vector3 BCD_closest = CollisionHelper.triangle_closest_point(B, C, D, point);

            if (Vector3.Distance(point, ABD_closest) < Vector3.Distance(point, BCD_closest)) {
                return ABD_closest;
            } else return BCD_closest;
        }


        public static Vector3 closest_corner_on_quad(Vector3 A, Vector3 B, Vector3 C, Vector3 D, Vector3 point) {
            int o = -1;
            float d = float.MaxValue;
            float cd = Vector3.Distance(A, point);
            if (cd < d) { d = cd; o = 1; }
            cd = Vector3.Distance(B, point);
            if (cd < d) { d = cd; o = 2; }
            cd = Vector3.Distance(C, point);
            if (cd < d) { d = cd; o = 3; }
            cd = Vector3.Distance(D, point);
            if (cd < d) { d = cd; o = 4; }
            cd = Vector3.Distance(A, point);
            if (cd < d) { d = cd; o = 1; }
            cd = Vector3.Distance(B, point);
            if (cd < d) { d = cd; o = 2; }
            cd = Vector3.Distance(C, point);
            if (cd < d) { d = cd; o = 3; }
            cd = Vector3.Distance(D, point);
            if (cd < d) { d = cd; o = 4; }

            switch (o) {
                case 1: return A;
                case 2: return B;
                case 3: return C;
                case 4: return D;
                default: return Vector3.Zero;
            }
        }

        public static Vector3 farthest_corner_on_quad(Vector3 A, Vector3 B, Vector3 C, Vector3 D, Vector3 point) {
            int o = -1;
            float d = 0f;
            float cd = Vector3.Distance(A, point);
            if (cd > d) { d = cd; o = 1; }
            cd = Vector3.Distance(B, point);
            if (cd > d) { d = cd; o = 2; }
            cd = Vector3.Distance(C, point);
            if (cd > d) { d = cd; o = 3; }
            cd = Vector3.Distance(D, point);
            if (cd > d) { d = cd; o = 4; }
            cd = Vector3.Distance(A, point);
            if (cd > d) { d = cd; o = 1; }
            cd = Vector3.Distance(B, point);
            if (cd > d) { d = cd; o = 2; }
            cd = Vector3.Distance(C, point);
            if (cd > d) { d = cd; o = 3; }
            cd = Vector3.Distance(D, point);
            if (cd > d) { d = cd; o = 4; }

            switch (o) {
                case 1: return A;
                case 2: return B;
                case 3: return C;
                case 4: return D;
                default: return Vector3.Zero;
            }
        }
        #endregion

        #region MISC CLOSEST POINT
       
        public static Vector3 closest_point_on_AABB(Vector3 point, Vector3 min, Vector3 max) {
            //low/high aka min/max
            float lx = min.X; float hx = max.X;
            float ly = min.Y; float hy = max.Y;
            float lz = min.Z; float hz = max.Z;

            //point
            float px = point.X;
            float py = point.Y;
            float pz = point.Z;

            //output
            float ox = 0;
            float oy = 0;
            float oz = 0;

            //X, Y and Z checks respectively
            if (px >= lx && px <= hx) ox = px;
            else if (px < lx) ox = lx;
            else if (px > hx) ox = hx;            

            if (py >= ly && py <= hy) oy = py;
            else if (py < ly) oy = ly;
            else if (py > hy) oy = hy;            

            if (pz >= lz && pz <= hz) oz = pz;
            else if (pz < lz) oz = lz;
            else if (pz > hz) oz = hz;
            
            return new Vector3(ox, oy, oz);
        }

        public static Vector3 closest_point_on_OBB(Vector3 point, Vector3 obb_origin, Matrix obb_orientation, Vector3 obb_half_scale) {
            Vector3 d = point - obb_origin;
            Vector3 outp = obb_origin;

            float dist = Vector3.Dot(d, obb_orientation.Right);
            if (dist > obb_half_scale.X) dist = obb_half_scale.X;
            if (dist < -obb_half_scale.X) dist = -obb_half_scale.X;

            outp += obb_orientation.Right * dist;


            dist = Vector3.Dot(d, obb_orientation.Up);
            if (dist > obb_half_scale.Y) dist = obb_half_scale.Y;
            if (dist < -obb_half_scale.Y) dist = -obb_half_scale.Y;

            outp += obb_orientation.Up * dist;


            dist = Vector3.Dot(d, obb_orientation.Forward);
            if (dist > obb_half_scale.Z) dist = obb_half_scale.Z;
            if (dist < -obb_half_scale.Z) dist = -obb_half_scale.Z;

            outp += obb_orientation.Forward * dist;

            return outp;
        }


        public static Vector3 project_direction_onto_plane(Vector3 P, Vector3 AB, Vector3 AC) {
            var n = Vector3.Cross(Vector3.Normalize(AB),Vector3.Normalize(AC));
            var d = Vector3.Dot(P,n);
            return d * n;
        }

        public static Vector3 closest_point_in_list(Vector3 p, params Vector3[] points) {
            float closest = float.MaxValue;
            int closest_index = -1;

            for (int i = 0; i < points.Length; i++) {
                var d = Vector3.Distance(p, points[i]);
                if (d < closest) {
                    closest = d;
                    closest_index = i;
                }
            }

            return points[closest_index];
        }
        public static float shortest_distance_to_a_point(Vector3 p, params Vector3[] points) {
            float closest = float.MaxValue;

            for (int i = 0; i < points.Length; i++) {
                var d = Vector3.Distance(p, points[i]);
                if (d < closest) {
                    closest = d;
                }
            }

            return closest;
        }

        #endregion
        

        //public static class BoundingBoxes {

            public static BoundingBox BoundingBox_around_OBB(OBB obb) {
                float Xmin = float.MaxValue, Ymin = float.MaxValue, Zmin = float.MaxValue;
                float Xmax = float.MinValue, Ymax = float.MinValue, Zmax = float.MinValue;

                if (obb.A.X > Xmax) Xmax = obb.A.X; if (obb.A.X < Xmin) Xmin = obb.A.X;
                if (obb.A.Y > Ymax) Ymax = obb.A.Y; if (obb.A.Y < Ymin) Ymin = obb.A.Y;
                if (obb.A.Z > Zmax) Zmax = obb.A.Z; if (obb.A.Z < Zmin) Zmin = obb.A.Z;

                if (obb.B.X > Xmax) Xmax = obb.B.X; if (obb.B.X < Xmin) Xmin = obb.B.X;
                if (obb.B.Y > Ymax) Ymax = obb.B.Y; if (obb.B.Y < Ymin) Ymin = obb.B.Y;
                if (obb.B.Z > Zmax) Zmax = obb.B.Z; if (obb.B.Z < Zmin) Zmin = obb.B.Z;

                if (obb.C.X > Xmax) Xmax = obb.C.X; if (obb.C.X < Xmin) Xmin = obb.C.X;
                if (obb.C.Y > Ymax) Ymax = obb.C.Y; if (obb.C.Y < Ymin) Ymin = obb.C.Y;
                if (obb.C.Z > Zmax) Zmax = obb.C.Z; if (obb.C.Z < Zmin) Zmin = obb.C.Z;

                if (obb.D.X > Xmax) Xmax = obb.D.X; if (obb.D.X < Xmin) Xmin = obb.D.X;
                if (obb.D.Y > Ymax) Ymax = obb.D.Y; if (obb.D.Y < Ymin) Ymin = obb.D.Y;
                if (obb.D.Z > Zmax) Zmax = obb.D.Z; if (obb.D.Z < Zmin) Zmin = obb.D.Z;

                if (obb.E.X > Xmax) Xmax = obb.E.X; if (obb.E.X < Xmin) Xmin = obb.E.X;
                if (obb.E.Y > Ymax) Ymax = obb.E.Y; if (obb.E.Y < Ymin) Ymin = obb.E.Y;
                if (obb.E.Z > Zmax) Zmax = obb.E.Z; if (obb.E.Z < Zmin) Zmin = obb.E.Z;

                if (obb.F.X > Xmax) Xmax = obb.F.X; if (obb.F.X < Xmin) Xmin = obb.F.X;
                if (obb.F.Y > Ymax) Ymax = obb.F.Y; if (obb.F.Y < Ymin) Ymin = obb.F.Y;
                if (obb.F.Z > Zmax) Zmax = obb.F.Z; if (obb.F.Z < Zmin) Zmin = obb.F.Z;

                if (obb.G.X > Xmax) Xmax = obb.G.X; if (obb.G.X < Xmin) Xmin = obb.G.X;
                if (obb.G.Y > Ymax) Ymax = obb.G.Y; if (obb.G.Y < Ymin) Ymin = obb.G.Y;
                if (obb.G.Z > Zmax) Zmax = obb.G.Z; if (obb.G.Z < Zmin) Zmin = obb.G.Z;

                if (obb.H.X > Xmax) Xmax = obb.H.X; if (obb.H.X < Xmin) Xmin = obb.H.X;
                if (obb.H.Y > Ymax) Ymax = obb.H.Y; if (obb.H.Y < Ymin) Ymin = obb.H.Y;
                if (obb.H.Z > Zmax) Zmax = obb.H.Z; if (obb.H.Z < Zmin) Zmin = obb.H.Z;

                return new BoundingBox(new Vector3(Xmin, Ymin, Zmin), new Vector3(Xmax, Ymax, Zmax));
            }

            public static BoundingBox BoundingBox_around_OBB(OBB obb, Matrix world) {
                float Xmin = float.MaxValue, Ymin = float.MaxValue, Zmin = float.MaxValue;
                float Xmax = float.MinValue, Ymax = float.MinValue, Zmax = float.MinValue;

                Vector3 tmp_A = Vector3.Transform(obb.A, world);
                Vector3 tmp_B = Vector3.Transform(obb.B, world);
                Vector3 tmp_C = Vector3.Transform(obb.C, world);
                Vector3 tmp_D = Vector3.Transform(obb.D, world);
                Vector3 tmp_E = Vector3.Transform(obb.E, world);
                Vector3 tmp_F = Vector3.Transform(obb.F, world);
                Vector3 tmp_G = Vector3.Transform(obb.G, world);
                Vector3 tmp_H = Vector3.Transform(obb.H, world);

                if (tmp_A.X > Xmax) Xmax = tmp_A.X; if (tmp_A.X < Xmin) Xmin = tmp_A.X;
                if (tmp_A.Y > Ymax) Ymax = tmp_A.Y; if (tmp_A.Y < Ymin) Ymin = tmp_A.Y;
                if (tmp_A.Z > Zmax) Zmax = tmp_A.Z; if (tmp_A.Z < Zmin) Zmin = tmp_A.Z;

                if (tmp_B.X > Xmax) Xmax = tmp_B.X; if (tmp_B.X < Xmin) Xmin = tmp_B.X;
                if (tmp_B.Y > Ymax) Ymax = tmp_B.Y; if (tmp_B.Y < Ymin) Ymin = tmp_B.Y;
                if (tmp_B.Z > Zmax) Zmax = tmp_B.Z; if (tmp_B.Z < Zmin) Zmin = tmp_B.Z;

                if (tmp_C.X > Xmax) Xmax = tmp_C.X; if (tmp_C.X < Xmin) Xmin = tmp_C.X;
                if (tmp_C.Y > Ymax) Ymax = tmp_C.Y; if (tmp_C.Y < Ymin) Ymin = tmp_C.Y;
                if (tmp_C.Z > Zmax) Zmax = tmp_C.Z; if (tmp_C.Z < Zmin) Zmin = tmp_C.Z;

                if (tmp_D.X > Xmax) Xmax = tmp_D.X; if (tmp_D.X < Xmin) Xmin = tmp_D.X;
                if (tmp_D.Y > Ymax) Ymax = tmp_D.Y; if (tmp_D.Y < Ymin) Ymin = tmp_D.Y;
                if (tmp_D.Z > Zmax) Zmax = tmp_D.Z; if (tmp_D.Z < Zmin) Zmin = tmp_D.Z;

                if (tmp_E.X > Xmax) Xmax = tmp_E.X; if (tmp_E.X < Xmin) Xmin = tmp_E.X;
                if (tmp_E.Y > Ymax) Ymax = tmp_E.Y; if (tmp_E.Y < Ymin) Ymin = tmp_E.Y;
                if (tmp_E.Z > Zmax) Zmax = tmp_E.Z; if (tmp_E.Z < Zmin) Zmin = tmp_E.Z;

                if (tmp_F.X > Xmax) Xmax = tmp_F.X; if (tmp_F.X < Xmin) Xmin = tmp_F.X;
                if (tmp_F.Y > Ymax) Ymax = tmp_F.Y; if (tmp_F.Y < Ymin) Ymin = tmp_F.Y;
                if (tmp_F.Z > Zmax) Zmax = tmp_F.Z; if (tmp_F.Z < Zmin) Zmin = tmp_F.Z;

                if (tmp_G.X > Xmax) Xmax = tmp_G.X; if (tmp_G.X < Xmin) Xmin = tmp_G.X;
                if (tmp_G.Y > Ymax) Ymax = tmp_G.Y; if (tmp_G.Y < Ymin) Ymin = tmp_G.Y;
                if (tmp_G.Z > Zmax) Zmax = tmp_G.Z; if (tmp_G.Z < Zmin) Zmin = tmp_G.Z;

                if (tmp_H.X > Xmax) Xmax = tmp_H.X; if (tmp_H.X < Xmin) Xmin = tmp_H.X;
                if (tmp_H.Y > Ymax) Ymax = tmp_H.Y; if (tmp_H.Y < Ymin) Ymin = tmp_H.Y;
                if (tmp_H.Z > Zmax) Zmax = tmp_H.Z; if (tmp_H.Z < Zmin) Zmin = tmp_H.Z;

                return new BoundingBox(new Vector3(Xmin, Ymin, Zmin), new Vector3(Xmax, Ymax, Zmax));
            }

            public static BoundingBox BoundingBox_around_sphere(Sphere sphere, Matrix world) {
                return new BoundingBox(world.Translation - (Vector3.One * sphere.radius), world.Translation + (Vector3.One * sphere.radius));
        }
        public static BoundingBox BoundingBox_around_sphere(float radius, Matrix world) {
            return new BoundingBox(world.Translation - (Vector3.One * radius), world.Translation + (Vector3.One * radius));
        }

        public static BoundingBox BoundingBox_around_OBB(Cube obb, Matrix world) {
                float Xmin = float.MaxValue, Ymin = float.MaxValue, Zmin = float.MaxValue;
                float Xmax = float.MinValue, Ymax = float.MinValue, Zmax = float.MinValue;

                Vector3 tmp_A = Vector3.Transform(obb.A, world);
                Vector3 tmp_B = Vector3.Transform(obb.B, world);
                Vector3 tmp_C = Vector3.Transform(obb.C, world);
                Vector3 tmp_D = Vector3.Transform(obb.D, world);
                Vector3 tmp_E = Vector3.Transform(obb.E, world);
                Vector3 tmp_F = Vector3.Transform(obb.F, world);
                Vector3 tmp_G = Vector3.Transform(obb.G, world);
                Vector3 tmp_H = Vector3.Transform(obb.H, world);

                if (tmp_A.X > Xmax) Xmax = tmp_A.X; if (tmp_A.X < Xmin) Xmin = tmp_A.X;
                if (tmp_A.Y > Ymax) Ymax = tmp_A.Y; if (tmp_A.Y < Ymin) Ymin = tmp_A.Y;
                if (tmp_A.Z > Zmax) Zmax = tmp_A.Z; if (tmp_A.Z < Zmin) Zmin = tmp_A.Z;

                if (tmp_B.X > Xmax) Xmax = tmp_B.X; if (tmp_B.X < Xmin) Xmin = tmp_B.X;
                if (tmp_B.Y > Ymax) Ymax = tmp_B.Y; if (tmp_B.Y < Ymin) Ymin = tmp_B.Y;
                if (tmp_B.Z > Zmax) Zmax = tmp_B.Z; if (tmp_B.Z < Zmin) Zmin = tmp_B.Z;

                if (tmp_C.X > Xmax) Xmax = tmp_C.X; if (tmp_C.X < Xmin) Xmin = tmp_C.X;
                if (tmp_C.Y > Ymax) Ymax = tmp_C.Y; if (tmp_C.Y < Ymin) Ymin = tmp_C.Y;
                if (tmp_C.Z > Zmax) Zmax = tmp_C.Z; if (tmp_C.Z < Zmin) Zmin = tmp_C.Z;

                if (tmp_D.X > Xmax) Xmax = tmp_D.X; if (tmp_D.X < Xmin) Xmin = tmp_D.X;
                if (tmp_D.Y > Ymax) Ymax = tmp_D.Y; if (tmp_D.Y < Ymin) Ymin = tmp_D.Y;
                if (tmp_D.Z > Zmax) Zmax = tmp_D.Z; if (tmp_D.Z < Zmin) Zmin = tmp_D.Z;

                if (tmp_E.X > Xmax) Xmax = tmp_E.X; if (tmp_E.X < Xmin) Xmin = tmp_E.X;
                if (tmp_E.Y > Ymax) Ymax = tmp_E.Y; if (tmp_E.Y < Ymin) Ymin = tmp_E.Y;
                if (tmp_E.Z > Zmax) Zmax = tmp_E.Z; if (tmp_E.Z < Zmin) Zmin = tmp_E.Z;

                if (tmp_F.X > Xmax) Xmax = tmp_F.X; if (tmp_F.X < Xmin) Xmin = tmp_F.X;
                if (tmp_F.Y > Ymax) Ymax = tmp_F.Y; if (tmp_F.Y < Ymin) Ymin = tmp_F.Y;
                if (tmp_F.Z > Zmax) Zmax = tmp_F.Z; if (tmp_F.Z < Zmin) Zmin = tmp_F.Z;

                if (tmp_G.X > Xmax) Xmax = tmp_G.X; if (tmp_G.X < Xmin) Xmin = tmp_G.X;
                if (tmp_G.Y > Ymax) Ymax = tmp_G.Y; if (tmp_G.Y < Ymin) Ymin = tmp_G.Y;
                if (tmp_G.Z > Zmax) Zmax = tmp_G.Z; if (tmp_G.Z < Zmin) Zmin = tmp_G.Z;

                if (tmp_H.X > Xmax) Xmax = tmp_H.X; if (tmp_H.X < Xmin) Xmin = tmp_H.X;
                if (tmp_H.Y > Ymax) Ymax = tmp_H.Y; if (tmp_H.Y < Ymin) Ymin = tmp_H.Y;
                if (tmp_H.Z > Zmax) Zmax = tmp_H.Z; if (tmp_H.Z < Zmin) Zmin = tmp_H.Z;

                return new BoundingBox(new Vector3(Xmin, Ymin, Zmin), new Vector3(Xmax, Ymax, Zmax));
            }

            public static BoundingBox BoundingBox_around_capsule(Capsule capsule, Matrix world) {
                Vector3 min = Vector3.Zero;
                Vector3 max = Vector3.Zero;

                var wA = Vector3.Transform(capsule.A, world);
                var wB = Vector3.Transform(capsule.B, world);

                if (wA.X >= wB.X) max.X = wA.X;
                else max.X = wB.X;

                if (wA.Y >= wB.Y) max.Y = wA.Y;
                else max.Y = wB.Y;

                if (wA.Z >= wB.Z) max.Z = wA.Z;
                else max.Z = wB.Z;

                min += Vector3.One * -capsule.radius;
                max += Vector3.One * capsule.radius;

                return new BoundingBox(min, max);
            }

            public static BoundingBox BoundingBox_around_capsule(Vector3 A, Vector3 B, float radius) {
                var min = A + -((Vector3.UnitX + Vector3.UnitZ + Vector3.UnitY) * radius);
                var max = B + ((Vector3.UnitX + Vector3.UnitZ + Vector3.UnitY) * radius);

                return new BoundingBox(min, max);
            }

            public static BoundingBox BoundingBox_around_capsule(Vector3 A, Vector3 B, float radius, Matrix world) {
                Vector3 scale;
                world.Decompose(out scale, out _, out _);

                var min = A + -((Vector3.UnitX + Vector3.UnitZ + Vector3.UnitY) * radius) * scale;
                var max = B + ((Vector3.UnitX + Vector3.UnitZ + Vector3.UnitY) * radius) * scale;

                return new BoundingBox(min, max);
            }

            public static BoundingBox BoundingBox_around_BoundingBoxes(params BoundingBox[] boxes) {
                Vector3 min = Vector3.One * float.MaxValue, max = Vector3.One * float.MinValue;

                foreach (BoundingBox box in boxes) {
                    if (box.Min.X < min.X)
                        min.X = box.Min.X;
                    if (box.Min.Y < min.Y)
                        min.Y = box.Min.Y;
                    if (box.Min.Z < min.Z)
                        min.Z = box.Min.Z;

                    if (box.Max.X > max.X)
                        max.X = box.Max.X;
                    if (box.Max.Y > max.Y)
                        max.Y = box.Max.Y;
                    if (box.Max.Z > max.Z)
                        max.Z = box.Max.Z;
                }

                return new BoundingBox(min, max);
            }
            public static BoundingBox BoundingBox_around_ModelCollisions(Matrix world, params ModelCollision[] collisions) {
                Vector3 min = Vector3.One * float.MaxValue, max = Vector3.One * float.MinValue;

                foreach (ModelCollision coll in collisions) {
                    var box = coll.get_bounds(world);

                    if (box.Min.X < min.X)
                        min.X = box.Min.X;
                    if (box.Min.Y < min.Y)
                        min.Y = box.Min.Y;
                    if (box.Min.Z < min.Z)
                        min.Z = box.Min.Z;

                    if (box.Max.X > max.X)
                        max.X = box.Max.X;
                    if (box.Max.Y > max.Y)
                        max.Y = box.Max.Y;
                    if (box.Max.Z > max.Z)
                        max.Z = box.Max.Z;
                }

                return new BoundingBox(min, max);
            }
            public static BoundingBox BoundingBox_around_Shapes(Matrix world, params Shape3D[] collisions) {
                Vector3 min = Vector3.One * float.MaxValue, max = Vector3.One * float.MinValue;

                foreach (Shape3D coll in collisions) {
                    var box = coll.find_bounding_box(world);

                    if (box.Min.X < min.X)
                        min.X = box.Min.X;
                    if (box.Min.Y < min.Y)
                        min.Y = box.Min.Y;
                    if (box.Min.Z < min.Z)
                        min.Z = box.Min.Z;

                    if (box.Max.X > max.X)
                        max.X = box.Max.X;
                    if (box.Max.Y > max.Y)
                        max.Y = box.Max.Y;
                    if (box.Max.Z > max.Z)
                        max.Z = box.Max.Z;
                }

                return new BoundingBox(min, max);
            }

            public static BoundingBox BoundingBox_around_points(List<Vector3> points) {
                Vector3 min = Vector3.One * float.MaxValue, max = Vector3.One * float.MinValue;

                foreach (Vector3 point in points) {
                    if (point.X < min.X)
                        min.X = point.X;
                    if (point.Y < min.Y)
                        min.Y = point.Y;
                    if (point.Z < min.Z)
                        min.Z = point.Z;

                    if (point.X > max.X)
                        max.X = point.X;
                    if (point.Y > max.Y)
                        max.Y = point.Y;
                    if (point.Z > max.Z)
                        max.Z = point.Z;
                }

                return new BoundingBox(min, max);
            }
            public static BoundingBox BoundingBox_around_points(params Vector3[] points) {
                Vector3 min = Vector3.One * float.MaxValue, max = Vector3.One * float.MinValue;

                foreach (Vector3 point in points) {
                    if (point.X < min.X)
                        min.X = point.X;
                    if (point.Y < min.Y)
                        min.Y = point.Y;
                    if (point.Z < min.Z)
                        min.Z = point.Z;

                    if (point.X > max.X)
                        max.X = point.X;
                    if (point.Y > max.Y)
                        max.Y = point.Y;
                    if (point.Z > max.Z)
                        max.Z = point.Z;
                }

                return new BoundingBox(min, max);
            }
            public static BoundingBox BoundingBox_around_transformed_points(Matrix world, params Vector3[] points) {
                Vector3 min = Vector3.One * float.MaxValue, max = Vector3.One * float.MinValue;

                foreach (Vector3 point in points) {
                    var p = Vector3.Transform(point, world);

                    if (p.X < min.X)
                        min.X = p.X;
                    if (p.Y < min.Y)
                        min.Y = p.Y;
                    if (p.Z < min.Z)
                        min.Z = p.Z;

                    if (p.X > max.X)
                        max.X = p.X;
                    if (p.Y > max.Y)
                        max.Y = p.Y;
                    if (p.Z > max.Z)
                        max.Z = p.Z;
                }

                return new BoundingBox(min, max);
            }
            public static BoundingBox BoundingBox_around_transformed_points(Matrix world, List<Vector3> points) {
                Vector3 min = Vector3.One * float.MaxValue, max = Vector3.One * float.MinValue;

                foreach (Vector3 point in points) {
                    var p = Vector3.Transform(point, world);

                    if (p.X < min.X)
                        min.X = p.X;
                    if (p.Y < min.Y)
                        min.Y = p.Y;
                    if (p.Z < min.Z)
                        min.Z = p.Z;

                    if (p.X > max.X)
                        max.X = p.X;
                    if (p.Y > max.Y)
                        max.Y = p.Y;
                    if (p.Z > max.Z)
                        max.Z = p.Z;
                }

                return new BoundingBox(min, max);
            }
        //}


    }
}