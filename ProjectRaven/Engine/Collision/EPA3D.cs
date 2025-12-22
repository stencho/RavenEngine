using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using ProjectRaven.Graphics.Drawing3D;
using ProjectRaven.Graphics;

namespace ProjectRaven.Engine.Collision {
    public class polytope {       
        public struct epa_vert {
            public Vector3 P;
            public Vector3 support_A, support_B;

            public epa_vert(Vector3 support_A, Vector3 support_B) {                
                this.support_A = support_A;
                this.support_B = support_B;
                P = support_A - support_B;
            }
            public epa_vert(Vector3 P, Vector3 support_A, Vector3 support_B) {
                this.support_A = support_A;
                this.support_B = support_B;
                this.P = P; 
            }
        }
        public struct index_tri {
            public int A;
            public int B; 
            public int C;            
            
            public index_tri(int a, int b, int c, bool remove) {
                A = a;
                B = b;
                C = c;
            }
        }

        public List<epa_vert> points = new List<epa_vert>();
        public List<index_tri> triangle_indices = new List<index_tri>();
        public List<(int A, int B)> edge_indices = new List<(int A, int B)>();

        public bool failed = false;

        Vector3 tri_normal(index_tri tri) {
            return Vector3.Cross(points[tri.B].P - points[tri.A].P, points[tri.C].P - points[tri.A].P);
        }
        Vector3 tri_center(index_tri tri) {
            return (points[tri.A].P + points[tri.B].P + points[tri.C].P) / 3;
        }

        public polytope copy() {
            var p = new polytope();
            p.points = points;
            p.triangle_indices = triangle_indices;
            p.edge_indices = edge_indices;
            return p;
        }

        public polytope() { }
        public polytope(gjk_simplex simplex) {
            if (simplex.stage != simplex_stage.tetrahedron)
                throw new Exception("Simplex was too small to apply EPA");

            triangle_indices = new List<index_tri>();
            edge_indices = new List<(int A, int B)>();
            points = new List<epa_vert>();

            add_triangle(
                new epa_vert(simplex.A, simplex.A_support.A_support, simplex.A_support.B_support) ,
                new epa_vert(simplex.B, simplex.B_support.A_support, simplex.B_support.B_support) ,
                new epa_vert(simplex.C, simplex.C_support.A_support, simplex.C_support.B_support));
            add_triangle(
                new epa_vert(simplex.A, simplex.A_support.A_support, simplex.A_support.B_support),
                new epa_vert(simplex.C, simplex.C_support.A_support, simplex.C_support.B_support),
                new epa_vert(simplex.D, simplex.D_support.A_support, simplex.D_support.B_support));
            add_triangle(
                new epa_vert(simplex.A, simplex.A_support.A_support, simplex.A_support.B_support),
                new epa_vert(simplex.D, simplex.D_support.A_support, simplex.D_support.B_support),
                new epa_vert(simplex.B, simplex.B_support.A_support, simplex.B_support.B_support));
            add_triangle(
                new epa_vert(simplex.C, simplex.C_support.A_support, simplex.C_support.B_support),
                new epa_vert(simplex.B, simplex.B_support.A_support, simplex.B_support.B_support),
                new epa_vert(simplex.D, simplex.D_support.A_support, simplex.D_support.B_support));
        }
        void add_triangle(epa_vert A, epa_vert B, epa_vert C) {
            var pa = try_add(A);
            var pb = try_add(B);
            var pc = try_add(C);

            triangle_indices.Add(new index_tri(pa, pb, pc, false));
        }

        int try_add(epa_vert P) {
            for (int i = 0; i < points.Count; i++) {
                if (P.P == points[i].P) return i;
            }

            points.Add(P);
            return points.Count - 1;
        }


        void add_edge(int A, int B) {
            var c = edge_indices.Count;
            for (int i = 0; i < c; i++) {
                if (edge_indices[i].A == B && edge_indices[i].B == A) {
                    edge_indices.RemoveAt(i);
                    return;
                }
            }

            edge_indices.Add((A, B));
        }

        public void expand(epa_vert P, ref gjk_simplex simplex, ref collision_result result) {

            for (int i = 0; i < triangle_indices.Count; i++) {
                var index = i;

                if (
                    Math3D.same_dir(
                        tri_normal(triangle_indices[index]),
                        P.P - tri_center(triangle_indices[index]) )) {
                    add_edge(triangle_indices[index].A, triangle_indices[index].C);
                    add_edge(triangle_indices[index].C, triangle_indices[index].B);
                    add_edge(triangle_indices[index].B, triangle_indices[index].A);

                    triangle_indices.RemoveAt(index);
                    i--;
                }

            }

            points.Add(P);
            var pi = points.Count-1;

            for (int i = 0; i < edge_indices.Count; i++) {
                triangle_indices.Add(new index_tri(pi,edge_indices[i].B,edge_indices[i].A,false));

            }

            edge_indices.Clear();
        }

        public void draw() {
            if (triangle_indices == null) return;
            foreach (var tri in triangle_indices) {
                //Draw3D.fill_tri(Matrix.Identity, points[tri.A], points[tri.B], points[tri.C], Color.Red);
            }
            foreach (var tri in triangle_indices) {                
                Draw3D.sprite_line(points[tri.A].P, points[tri.B].P, 0.02f, Color.Orange);
                Draw3D.sprite_line(points[tri.A].P, points[tri.C].P, 0.02f, Color.Orange);
                Draw3D.sprite_line(points[tri.C].P, points[tri.B].P, 0.02f, Color.Orange);

            }
            foreach (var edge in edge_indices) {
                Draw3D.sprite_line(points[edge.A].P, points[edge.B].P, 0.02f, Color.Red);
            }

            foreach (var tri in triangle_indices) {
                Draw3D.sprite_line(points[tri.A].P, points[tri.B].P, 0.02f, Color.Orange);
                Draw3D.sprite_line(points[tri.A].P, points[tri.C].P, 0.02f, Color.Orange);
                Draw3D.sprite_line(points[tri.C].P, points[tri.B].P, 0.02f, Color.Orange);
            }


        }
    }
    public static class EPA3D {

        //find closest facet /
        //find new support in direction /
        //tag faces that have the same normal dir as -P for removal /
        //add their edges to the edge list, CCW /
        //if an opposite edge already exists, remove it and don't add the new edge /
        //remove tris /
        //add new triangles using the support point + the edges in the edge list /
        //mind the wind /
        //clear the edge list /
        //repeat until closest facet is already in list /

        public static polytope expand_polytope(Shape3D shape_A, Shape3D shape_B, ref gjk_simplex simplex, ref collision_result result) {
            polytope poly = new polytope(simplex);

            Vector3 last_closest = Vector3.Zero;

            float closest = float.MaxValue;
            Vector3 closest_facet_point = Vector3.Zero;
            int closest_facet_index = -1;

            var iterations = 0;
            while (iterations < 10) {
                closest = float.MaxValue;
                closest_facet_point = Vector3.Zero;
                closest_facet_index = -1;

                for (int i = 0; i < poly.triangle_indices.Count; i++) {
                    var v = CollisionHelper.triangle_closest_point_alternative(
                        poly.points[poly.triangle_indices[i].A].P,
                        poly.points[poly.triangle_indices[i].B].P,
                        poly.points[poly.triangle_indices[i].C].P,
                        Vector3.Zero
                        );
                    var d = v.Length();
                    if (d < closest) {
                        closest = d;
                        closest_facet_point = v;
                        closest_facet_index = i;
                    }
                }

                if (iterations > 0) {
                    if (Vector3.Distance(last_closest, closest_facet_point) < Math3D.big_epsilon) {

                        result.penetration = Vector3.Distance(closest_facet_point, Vector3.Zero);
                        result.penetration_normal = Vector3.Normalize(-closest_facet_point);

                        break;
                    }
                }

                last_closest = closest_facet_point;
                var A = Vector3.Transform(
                    shape_A.support(
                        Vector3.Transform(
                            closest_facet_point,
                            Matrix.Invert(simplex.A_transform_direction)),
                        simplex.sweep_A),
                    simplex.A_transform);

                var B = Vector3.Transform(
                    shape_B.support(
                        Vector3.Transform(
                            -closest_facet_point,
                            Matrix.Invert(simplex.B_transform_direction)),
                        simplex.sweep_B),
                    simplex.B_transform);

                poly.expand(new polytope.epa_vert(A, B), ref simplex, ref result);

                
                iterations++;
            }

            result.penetration = Vector3.Distance(closest_facet_point, Vector3.Zero);
            result.penetration_normal = Vector3.Normalize(-closest_facet_point);

            var basis_tangents = compute_basis(result.penetration_normal);
            
            result.penetration_tangent_A = basis_tangents.A;
            result.penetration_tangent_B = basis_tangents.B;


            if (result.penetration_normal.contains_nan()) {
                result.penetration_normal = Vector3.Zero;
                result.penetration = 0f;
            }


            closest_facet_index = -1; closest = float.MaxValue;

            for (int i = 0; i < poly.triangle_indices.Count; i++) {
                var v = CollisionHelper.triangle_closest_point_alternative(
                        poly.points[poly.triangle_indices[i].A].P,
                        poly.points[poly.triangle_indices[i].B].P,
                        poly.points[poly.triangle_indices[i].C].P,
                        Vector3.Zero
                        );
                var d = v.Length();
                if (d < closest) {
                    closest = d;
                    closest_facet_index = i;
                }
            }
            
            if (poly.triangle_indices.Count == 0 || closest_facet_index == -1) { poly.failed = true; return poly; }
            
            var closest_tri = poly.triangle_indices[closest_facet_index];

            var uvw = CollisionHelper.triangle_barycentric(
                Vector3.Zero,
                (poly.points[closest_tri.A].P),
                (poly.points[closest_tri.B].P),
                (poly.points[closest_tri.C].P));


            var contact_A = 
                ((poly.points[closest_tri.A].support_A) * uvw.u +
                 (poly.points[closest_tri.B].support_A) * uvw.v + 
                 (poly.points[closest_tri.C].support_A) * uvw.w);

            var contact_B =
                ((poly.points[closest_tri.A].support_B) * uvw.u +
                 (poly.points[closest_tri.B].support_B) * uvw.v +
                 (poly.points[closest_tri.C].support_B) * uvw.w);

            result.closest_A = contact_A;
            result.closest_B = contact_B;



            return poly;
        }

        static (Vector3 A, Vector3 B) compute_basis(Vector3 normal) {
            var x = MathF.Abs(normal.X);
            var y = MathF.Abs(normal.Y);
            var z = MathF.Abs(normal.Z);

            var a = Vector3.Zero;
            var b = Vector3.Zero;

            if (x >= 0.57735f) {
                a = new Vector3(y,-x,0f);
            } else {
                a = new Vector3(0f, z, -y);
            }

            a.Normalize();
            b = Vector3.Cross(normal, a);

            return (a, b);
        }
    }
}
