using Microsoft.Xna.Framework;
using ProjectRaven.Engine.Collision.Shapes3D;

namespace ProjectRaven.Engine.Collision {
    public class Supports {        
        public static Vector3 Polyhedron(Vector3 direction, params Vector3[] verts) {
            return Math3D.highest_dot(verts, direction, out _);
        }

        public static Vector3 Capsule(Vector3 direction, Vector3 A, Vector3 B, float radius) {
            var lp = Supports.Line(direction, A, B);
            return Supports.Sphere(direction, lp, radius);
        }
        public static Vector3 Line(Vector3 direction, Vector3 A, Vector3 B) {
            if (Vector3.Dot(A, direction) > Vector3.Dot(B, direction)) {
                return A;
            } else {
                return B;                
            }            
        }
        
        public static Vector3 Sphere(Vector3 direction, Vector3 P, float radius) {
            return P + Vector3.Normalize(direction) * radius;
        }

        public static Vector3 Point(Vector3 direction, Vector3 P) {
            return P;
        }
        public static Vector3 GetFarthestPointInDirection(Vector3 direction, Cube cube) {
            Vector3 output = Vector3.Zero;

            if (direction.X >= 0) 
                output.X = cube.half_scale.X;
            else if (direction.X < 0) 
                output.X = -cube.half_scale.X;
            
            if (direction.Y >= 0)
                output.Y = cube.half_scale.Y;
            else if (direction.Y < 0)
                output.Y = -cube.half_scale.Y;            

            if (direction.Z >= 0) 
                output.Z = cube.half_scale.Z;
            else if (direction.Z < 0)
                output.Z = -cube.half_scale.Z;
            
            return output;

        }

        public static Vector3 Tri(Vector3 direction, Vector3 A, Vector3 B, Vector3 C) {                       
            return CollisionHelper.triangle_farthest_point(A, B, C, direction);  
        }

        public static Vector3 Quad(Vector3 direction, Vector3 A, Vector3 B, Vector3 C, Vector3 D) { 
            return Math3D.highest_dot(new Vector3[4] { A,B,C,D }, direction, out _);
        }

        public static Vector3 Cube(Vector3 direction, Cube cube) {
            return GetFarthestPointInDirection(direction, cube);
            return Math3D.highest_dot(new Vector3[8] {
                cube.A,
                cube.B,
                cube.C,
                cube.D,
                cube.E,
                cube.F,
                cube.G,
                cube.H }, 
                direction, out _);
        }
        public static Vector3 CubeSweep(Vector3 direction, Cube cube, Vector3 sweep) {
            return Math3D.highest_dot(new Vector3[16] {
                cube.A,
                cube.B,
                cube.C,
                cube.D,
                cube.E,
                cube.F,
                cube.G,
                cube.H,
                cube.A + sweep,
                cube.B + sweep,
                cube.C + sweep,
                cube.D + sweep,
                cube.E + sweep,
                cube.F + sweep,
                cube.G + sweep,
                cube.H + sweep },
                direction, out _);
        }
    }
}
