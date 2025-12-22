using System;
using Microsoft.Xna.Framework;

namespace Raven.Engine.Collision {
    public static class Raycasting {
        public struct raycast {
            public Vector3 start;
            public Vector3 direction;
            
            public raycast(Vector3 start, Vector3 direction_norm) {
                this.start = start;
                this.direction = direction_norm;
            }
        }

        public struct raycast_result {
            public bool hit;
            public Vector3 point;
            public float distance;
            public Vector3 hit_normal;
        }

        private static float scalar_triple(Vector3 A, Vector3 B, Vector3 C) { return Vector3.Dot(B, Vector3.Cross(C, A)); }
        public static bool same_direction(Vector3 direction, Vector3 origin_dir) {
            var vd = Vector3.Dot(direction, origin_dir);
            return (vd >= 0f);
        }

        private const float epsilon = 1e-6f;
        private static bool compare(float x, float y) {
            return Math.Abs(x - y) <= epsilon * Math.Max(1.0f, Math.Max(Math.Abs(x), Math.Abs(y)));
        }

        private static Vector3[] norms = new Vector3[] {
            new Vector3(-1, 0, 0),
            new Vector3(1, 0, 0),
            new Vector3(0, -1, 0),
            new Vector3(0, 1, 0),
            new Vector3(0, 0, -1),
            new Vector3(0, 0, 1)
        };


        public static bool ray_intersects_plane(raycast ray, Plane plane, out raycast_result result) {
            return ray_intersects_plane(ray.start, ray.direction, plane, out result);
        }
        public static bool ray_intersects_plane(Vector3 ray_start, Vector3 ray_dir, Plane plane, out raycast_result result) {
            float nd = Vector3.Dot(ray_dir, plane.Normal);
            float pn = Vector3.Dot(ray_start, plane.Normal);
            result = new raycast_result();

            float t = (plane.D - pn) / nd;

            if (nd < 0.0f) {

                if (t >= 0f) {
                    result.hit = true;
                    result.distance = t;
                    result.point = ray_start + ray_dir * t;
                    result.hit_normal = Vector3.Normalize(plane.Normal);
                    return true;
                }
            }

            return false;
        }
        
        public static bool ray_intersects_triangle(Vector3 ray_start, Vector3 ray_dir, Vector3 A, Vector3 B, Vector3 C, out raycast_result result) {           
            Vector3 tri_normal = Vector3.Cross(B-A, C-A);
            float area_sq = tri_normal.Length();

            float norm_dot_ray = Vector3.Dot(tri_normal, ray_dir);
            result = new raycast_result() { hit = false };

            //parallel case
            if (Math.Abs(norm_dot_ray) < epsilon) return false; 

            float d = Vector3.Dot(-tri_normal, A);
            float t = -(Vector3.Dot(tri_normal, ray_start) + d) / norm_dot_ray;

            //behind the ray
            if (t < 0) return false;

            //intersection point
            var P = ray_start + t * ray_dir;

            result.point = P;
            result.distance = Vector3.Distance(ray_start, P);

            var E = B - A;
            var EP = P - A;
            var cr = Vector3.Cross(E,EP);
            if (Vector3.Dot(tri_normal, cr) < 0) return false;

            E = C - B;
            EP = P - B;
            cr = Vector3.Cross(E, EP);
            if (Vector3.Dot(tri_normal, cr) < 0) return false;

            E = A - C;
            EP = P - C;
            cr = Vector3.Cross(E, EP);
            if (Vector3.Dot(tri_normal, cr) < 0) return false;

            result.hit = true;
            result.hit_normal = tri_normal;
            return true;
        }


        public static bool ray_intersects_BoundingBox(Vector3 ray_start, Vector3 ray_dir, Vector3 aabb_min, Vector3 aabb_max, out raycast_result result) {
            Vector3 min, max;
            result = new raycast_result();

            min = aabb_min;
            max = aabb_max;

            float t1 = (min.X - ray_start.X) / (compare(ray_dir.X, 0f) ? 0.00001f : ray_dir.X);
            float t2 = (max.X - ray_start.X) / (compare(ray_dir.X, 0f) ? 0.00001f : ray_dir.X);

            float t3 = (min.Y - ray_start.Y) / (compare(ray_dir.Y, 0f) ? 0.00001f : ray_dir.Y);
            float t4 = (max.Y - ray_start.Y) / (compare(ray_dir.Y, 0f) ? 0.00001f : ray_dir.Y);

            float t5 = (min.Z - ray_start.Z) / (compare(ray_dir.Z, 0f) ? 0.00001f : ray_dir.Z);
            float t6 = (max.Z - ray_start.Z) / (compare(ray_dir.Z, 0f) ? 0.00001f : ray_dir.Z);

            float tmin = Math.Max(Math.Max(Math.Min(t1, t2), Math.Min(t3, t4)), Math.Min(t5, t6));
            float tmax = Math.Min(Math.Min(Math.Max(t1, t2), Math.Max(t3, t4)), Math.Max(t5, t6));

            if (tmax < 0 || tmin > tmax) { result.hit = false; return false; }

            float tres = tmin;
            if (tmin < 0f) tres = tmax;

            float[] t = new float[] { t1, t2, t3, t4, t5, t6 };

            result.hit = true;
            result.point = ray_start + ray_dir * tres;
            result.distance = tres;

            for (int i = 0; i < 6; i++)
                if (compare(tres, t[i])) result.hit_normal = norms[i];

            return true;
        }


        public static void swap(ref float a, ref float b) {
            float tmp = a;
            a = b;
            b = tmp;
        }
        

        public static bool ray_intersects_sphere(Vector3 ray_start, Vector3 ray_dir, Vector3 sphere_pos, float sphere_radius, out raycast_result result) {
            result = new raycast_result();

            Vector3 dir = sphere_pos - ray_start;
            float radius_sq = sphere_radius * sphere_radius;


            float dir_mag_sq = Vector3.DistanceSquared(Vector3.Zero, dir);

            float a = Vector3.Dot(dir, ray_dir);
            float sq = dir_mag_sq - (a * a);

            float f = (float)Math.Sqrt(Math.Abs(radius_sq - sq));

            float t = a - f;

            if (radius_sq - (dir_mag_sq - a * a) < 0f) return false; //early exit
            if (dir_mag_sq < radius_sq) t = a + f; //ray starts in sphere, reverse direction

            result.distance = t;
            result.hit = true;
            result.point = ray_start + (ray_dir * t);
            result.hit_normal = Vector3.Normalize(result.point - sphere_pos);

            return true;
        }


        public static bool ray_intersects_quad(Vector3 ray_start, Vector3 ray_dir, Vector3 A, Vector3 B, Vector3 C, Vector3 D, out Vector3 intersection_point, out Vector3 normal) {
            intersection_point = Vector3.Zero;

            Vector3 rsrd = ray_dir;
            Vector3 start_A = A - ray_start;
            Vector3 start_B = B - ray_start;
            Vector3 start_C = C - ray_start;

            Vector3 tri = Vector3.Cross(start_C, rsrd);
            float v = Vector3.Dot(start_A, tri);
            normal = Vector3.Zero;

            if (v > 0f) {
                float u = -Vector3.Dot(start_B, tri);
                if (u > 0f) return false;

                float w = scalar_triple(rsrd, start_B, start_A);
                if (w > 0f) return false;

                float denom = 1f / (u + v + w);
                u *= denom; v *= denom; w *= denom;

                intersection_point = (u * A) + (v * B) + (w * C);
                normal = Vector3.Normalize(Vector3.Cross(B - A, B - C));
            }
            else {
                Vector3 start_D = D - ray_start;

                float u = Vector3.Dot(start_D, tri);
                if (u > 0f) return false;

                float w = scalar_triple(rsrd, start_A, start_D);
                if (w > 0f) return false;

                v = -v;

                float denom = 1f / (u + v + w);
                u *= denom; v *= denom; w *= denom;

                intersection_point = (u * A) + (v * D) + (w * C);
                normal = Vector3.Normalize(Vector3.Cross(D - C, C - A));
            }

            //if (CollisionHelper.same_direction(ray_dir, intersection_point - ray_start)) { intersection_point = Vector3.Zero; return false; }

            return true;
        }
    }
}
