using System;
using Microsoft.Xna.Framework;

namespace Raven.Engine {
    //disable warnings for equals/hashcode overrides - works fine without
#pragma warning disable CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
#pragma warning disable CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()

    public struct Vector3i {
        public int X;
        public int Y;
        public int Z;

        public Vector3i(int XYZ) {
            this.X = XYZ;
            this.Y = XYZ;
            this.Z = XYZ;
        }
        public Vector3i(int X, int Y, int Z) {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }
        public Vector3i(float X, float Y, float Z) {
            this.X = (int)X;
            this.Y = (int)Y;
            this.Z = (int)Z;
        }

        public Vector3i(Vector2 vector) {
            this.X = (int)vector.X;
            this.Y = (int)vector.Y;
            this.Z = 0;
        }
        
        public Vector3i(Vector2 vector, int Z) {
            this.X = (int)vector.X;
            this.Y = (int)vector.Y;
            this.Z = Z;
        }
        public Vector3i(Vector2i vector) {
            this.X = vector.X;
            this.Y = vector.Y;
            this.Z = 0;
        }
        public Vector3i(Vector2i vector, int Z) {
            this.X = vector.X;
            this.Y = vector.Y;
            this.Z = Z;
        }

        public Vector3i(Vector3 vector) {
            this.X = (int)vector.X;
            this.Y = (int)vector.Y;
            this.Z = (int)vector.Z;
        }

        public Vector3i(Point point) {
            this.X = point.X;
            this.Y = point.Y;
            this.Z = 0;
        }
        public Vector3i(Point point, int Z) {
            this.X = point.X;
            this.Y = point.Y;
            this.Z = Z;
        }
        
        public float Length() {
            int x2 = X * X;
            int y2 = Y * Y;
            int z2 = Z * Z;
            return (float)Math.Sqrt(x2 + y2 + z2);
        }

        public static float Length(Vector3i a, Vector3i b) {
            float x2 = b.X - a.X; x2 *= x2;
            float y2 = b.Y - a.Y; y2 *= y2;
            float z2 = b.Z - a.Z; z2 *= z2;
            return (float)Math.Sqrt(x2 + y2 + z2);
        }

        public static float Length(Vector3i a, Vector3 b) {
            float x2 = b.X - a.X; x2 *= x2;
            float y2 = b.Y - a.Y; y2 *= y2;
            float z2 = b.Z - a.Z; z2 *= z2;
            return (float)Math.Sqrt(x2 + y2 + z2);
        }

        public static explicit operator Vector3i(Vector3 v) {
            return new Vector3i((int)v.X, (int)v.Y, (int)v.Z);
        }

        #region int
        public static Vector3i operator -(Vector3i a, int b) => new Vector3i() { X = a.X - b, Y = a.Y - b, Z = a.Z - b };
        public static Vector3i operator +(Vector3i a, int b) => new Vector3i() { X = a.X + b, Y = a.Y + b, Z = a.Z + b };
        public static Vector3i operator *(Vector3i a, int b) => new Vector3i() { X = a.X * b, Y = a.Y * b, Z = a.Z * b };
        public static Vector3i operator /(Vector3i a, int b) => new Vector3i() { X = a.X / b, Y = a.Y / b, Z = a.Z / b };

        #endregion
        #region Vector3i
        public static Vector3i operator -(Vector3i a) => new Vector3i() { X = -a.X, Y = -a.Y, Z = -a.Z};
        public static Vector3i operator -(Vector3i a, Vector3i b) => new Vector3i() { X = a.X - b.X, Y = a.Y - b.Y, Z = a.Z - b.Z };
        public static Vector3i operator +(Vector3i a, Vector3i b) => new Vector3i() { X = a.X + b.X, Y = a.Y + b.Y, Z = a.Z + b.Z };
        public static Vector3i operator *(Vector3i a, Vector3i b) => new Vector3i() { X = a.X * b.X, Y = a.Y * b.Y, Z = a.Z * b.Z };
        public static Vector3i operator /(Vector3i a, Vector3i b) => new Vector3i() { X = a.X / b.X, Y = a.Y / b.Y, Z = a.Z / b.Z };

        #endregion
        #region float
        public static Vector3 operator *(float b, Vector3i a) => new Vector3(a.X * b, a.Y * b, a.Z * b);
        public static Vector3 operator -(Vector3i a, float b) => new Vector3(a.X - b, a.Y - b, a.Z - b);
        public static Vector3 operator +(Vector3i a, float b) => new Vector3(a.X + b, a.Y + b, a.Z + b);
        public static Vector3 operator *(Vector3i a, float b) => new Vector3(a.X * b, a.Y * b, a.Z * b);
        public static Vector3 operator /(Vector3i a, float b) => new Vector3(a.X / b, a.Y / b, a.Z / b);

        #endregion
        #region Vector3
        public static Vector3 operator -(Vector3 a, Vector3i b) => new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        public static Vector3 operator +(Vector3 a, Vector3i b) => new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static Vector3 operator *(Vector3 a, Vector3i b) => new Vector3(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        public static Vector3 operator /(Vector3 a, Vector3i b) => new Vector3(a.X / b.X, a.Y / b.Y, a.Z / b.Z);

        public static Vector3i operator *(Vector3i a, Vector3 b) => new Vector3i(a.X * (int)b.X, a.Y * (int)b.Y, a.Z * (int)b.Z);
        public static Vector3i operator /(Vector3i a, Vector3 b) => new Vector3i(a.X / (int)b.X, a.Y / (int)b.Y, a.Z / (int)b.Z);
        public static Vector3i operator -(Vector3i a, Vector3 b) => new Vector3i(a.X - (int)b.X, a.Y - (int)b.Y, a.Z - (int)b.Z);
        public static Vector3i operator +(Vector3i a, Vector3 b) => new Vector3i(a.X + (int)b.X, a.Y + (int)b.Y, a.Z + (int)b.Z);
        
        #endregion
        #region Point
        public static Point operator *(Point b, Vector3i a) => new Point(a.X * b.X, a.Y * b.Y);
        public static Point operator /(Point b, Vector3i a) => new Point(a.X / b.X, a.Y / b.Y);
        public static Point operator -(Point b, Vector3i a) => new Point(a.X - b.X, a.Y - b.Y);
        public static Point operator +(Point b, Vector3i a) => new Point(a.X + b.X, a.Y + b.Y);

        public static Vector3i operator *(Vector3i a, Point b) => new Vector3i(a.X * (int)b.X, a.Y * (int)b.Y, a.Z * 1);
        public static Vector3i operator /(Vector3i a, Point b) => new Vector3i(a.X / (int)b.X, a.Y / (int)b.Y, a.Z / 1);
        public static Vector3i operator -(Vector3i a, Point b) => new Vector3i(a.X - (int)b.X, a.Y - (int)b.Y, a.Z - 0);
        public static Vector3i operator +(Vector3i a, Point b) => new Vector3i(a.X + (int)b.X, a.Y + (int)b.Y, a.Z + 0);
        #endregion
        
        public static bool operator ==(Vector3i a, Vector3i b) => (a.X == b.X && a.Y == b.Y);
        public static bool operator !=(Vector3i a, Vector3i b) => (a.X != b.X || a.Y != b.Y);

        public Vector2i XY => new Vector2i(X, Y);
        public Vector2i XZ => new Vector2i(X, Z);
        public Vector2i ZY => new Vector2i(Z, Y);
        
        public override string ToString() => $"{{ {X} : {Y} : {Z} }}";
        public string ToXString() => $"{X}x{Y}x{Z}";
        public static string simple_string(Vector3i input) { return $"{input.X}, {input.Y}, {input.Z}"; }
        public static string simple_string_brackets(Vector3i input) { return $"[{input.X}, {input.Y}, {input.Z}]"; }
        
        public static bool TryParse(string input, out Vector3i result) {
            result = Zero;

            string[] split = input.Split('x', ',');

            if (split.Length != 3) return false;

            for (int i = 0; i < split.Length; i++) {
                split[i] = split[i].Trim(' ', '[', ']', '{', '}', '(', ')');
            }

            if (int.TryParse(split[0], out result.X) && int.TryParse(split[1], out result.Y) && int.TryParse(split[2], out result.Z)) {
                return true;
            }
            
            result = Zero;
            return false;
        }

        private static Vector3i _one = new Vector3i { X = 1, Y = 1, Z = 1 };
        private static Vector3i _zero = new Vector3i { X = 0, Y = 0 };

        private static Vector3i _unitX = new Vector3i { X = 1, Y = 0, Z = 0 };
        private static Vector3i _unitY = new Vector3i { X = 0, Y = 1, Z = 0 };
        private static Vector3i _unitZ = new Vector3i { X = 0, Y = 0, Z = 1 };

        public static Vector3i UnitX => _unitX;
        public static Vector3i UnitY => _unitY;
        public static Vector3i UnitZ => _unitZ;

        public static Vector3i Width => _unitX;
        public static Vector3i Height => _unitY;
        public static Vector3i Depth => _unitZ;

        public static Vector3i One => _one;
        public static Vector3i Zero => _zero;

        public static Vector3i Up => -_unitY;
        public static Vector3i Down => _unitY;

        public static Vector3i Left => -_unitX;
        public static Vector3i Right => _unitX;
        
        public static Vector3i Forward => -_unitZ;
        public static Vector3i Backward => -_unitZ;
    }
    public struct Vector3d {
        public double X;
        public double Y;
        public double Z;

        public Vector3d(double XYZ) {
            this.X = XYZ;
            this.Y = XYZ;
            this.Z = XYZ;
        }
        public Vector3d(double X, double Y, double Z) {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }
        public Vector3d(float X, float Y, double Z) {
            this.X = (double)X;
            this.Y = (double)Y;
            this.Z = (double)Z;
        }

        public Vector3d(Vector2 vector) {
            this.X = (double)vector.X;
            this.Y = (double)vector.Y;
            this.Z = 0;
        }
        
        public Vector3d(Vector2 vector, double Z) {
            this.X = (double)vector.X;
            this.Y = (double)vector.Y;
            this.Z = Z;
        }
        public Vector3d(Vector2i vector) {
            this.X = vector.X;
            this.Y = vector.Y;
            this.Z = 0;
        }
        public Vector3d(Vector2i vector, double Z) {
            this.X = vector.X;
            this.Y = vector.Y;
            this.Z = Z;
        }

        public Vector3d(Vector3 vector) {
            this.X = (double)vector.X;
            this.Y = (double)vector.Y;
            this.Z = (double)vector.Z;
        }

        public Vector3d(Point point) {
            this.X = point.X;
            this.Y = point.Y;
            this.Z = 0;
        }
        public Vector3d(Point point, int Z) {
            this.X = point.X;
            this.Y = point.Y;
            this.Z = Z;
        }
        
        public double Length() {
            double x2 = X * X;
            double y2 = Y * Y;
            double z2 = Z * Z;
            return Math.Sqrt(x2 + y2 + z2);
        }

        public static double Length(Vector3d a, Vector3d b) {
            double x2 = b.X - a.X; x2 *= x2;
            double y2 = b.Y - a.Y; y2 *= y2;
            double z2 = b.Z - a.Z; z2 *= z2;
            return Math.Sqrt(x2 + y2 + z2);
        }

        public static double Length(Vector3d a, Vector3 b) {
            double x2 = b.X - a.X; x2 *= x2;
            double y2 = b.Y - a.Y; y2 *= y2;
            double z2 = b.Z - a.Z; z2 *= z2;
            return Math.Sqrt(x2 + y2 + z2);
        }

        public static explicit operator Vector3d(Vector3 v) {
            return new Vector3d((int)v.X, (int)v.Y, (int)v.Z);
        }

        #region int
        public static Vector3d operator -(Vector3d a, int b) => new Vector3d() { X = a.X - b, Y = a.Y - b, Z = a.Z - b };
        public static Vector3d operator +(Vector3d a, int b) => new Vector3d() { X = a.X + b, Y = a.Y + b, Z = a.Z + b };
        public static Vector3d operator *(Vector3d a, int b) => new Vector3d() { X = a.X * b, Y = a.Y * b, Z = a.Z * b };
        public static Vector3d operator /(Vector3d a, int b) => new Vector3d() { X = a.X / b, Y = a.Y / b, Z = a.Z / b };

        #endregion
        #region Vector3d
        public static Vector3d operator -(Vector3d a) => new Vector3d() { X = -a.X, Y = -a.Y, Z = -a.Z};
        public static Vector3d operator -(Vector3d a, Vector3d b) => new Vector3d() { X = a.X - b.X, Y = a.Y - b.Y, Z = a.Z - b.Z };
        public static Vector3d operator +(Vector3d a, Vector3d b) => new Vector3d() { X = a.X + b.X, Y = a.Y + b.Y, Z = a.Z + b.Z };
        public static Vector3d operator *(Vector3d a, Vector3d b) => new Vector3d() { X = a.X * b.X, Y = a.Y * b.Y, Z = a.Z * b.Z };
        public static Vector3d operator /(Vector3d a, Vector3d b) => new Vector3d() { X = a.X / b.X, Y = a.Y / b.Y, Z = a.Z / b.Z };

        #endregion
        #region float
        public static Vector3d operator *(double b, Vector3d a) => new Vector3d(a.X * b, a.Y * b, a.Z * b);
        public static Vector3d operator -(Vector3d a, double b) => new Vector3d(a.X - b, a.Y - b, a.Z - b);
        public static Vector3d operator +(Vector3d a, double b) => new Vector3d(a.X + b, a.Y + b, a.Z + b);
        public static Vector3d operator *(Vector3d a, double b) => new Vector3d(a.X * b, a.Y * b, a.Z * b);
        public static Vector3d operator /(Vector3d a, double b) => new Vector3d(a.X / b, a.Y / b, a.Z / b);

        #endregion
        #region Vector3
        public static Vector3 operator -(Vector3 a, Vector3d b) => new Vector3(a.X - (float)b.X, a.Y - (float)b.Y, a.Z - (float)b.Z);
        public static Vector3 operator +(Vector3 a, Vector3d b) => new Vector3(a.X + (float)b.X, a.Y + (float)b.Y, a.Z + (float)b.Z);
        public static Vector3 operator *(Vector3 a, Vector3d b) => new Vector3(a.X * (float)b.X, a.Y * (float)b.Y, a.Z * (float)b.Z);
        public static Vector3 operator /(Vector3 a, Vector3d b) => new Vector3(a.X / (float)b.X, a.Y / (float)b.Y, a.Z / (float)b.Z);

        public static Vector3d operator *(Vector3d a, Vector3 b) => new Vector3d(a.X * (int)b.X, a.Y * (int)b.Y, a.Z * (int)b.Z);
        public static Vector3d operator /(Vector3d a, Vector3 b) => new Vector3d(a.X / (int)b.X, a.Y / (int)b.Y, a.Z / (int)b.Z);
        public static Vector3d operator -(Vector3d a, Vector3 b) => new Vector3d(a.X - (int)b.X, a.Y - (int)b.Y, a.Z - (int)b.Z);
        public static Vector3d operator +(Vector3d a, Vector3 b) => new Vector3d(a.X + (int)b.X, a.Y + (int)b.Y, a.Z + (int)b.Z);
        
        #endregion
        
        public static bool operator ==(Vector3d a, Vector3d b) => (a.X == b.X && a.Y == b.Y);
        public static bool operator !=(Vector3d a, Vector3d b) => (a.X != b.X || a.Y != b.Y);

        public override string ToString() => $"{{ {X} : {Y} : {Z} }}";
        public string ToXString() => $"{X}x{Y}x{Z}";
        public static string simple_string(Vector3d input) { return $"{input.X}, {input.Y}, {input.Z}"; }
        public static string simple_string_brackets(Vector3d input) { return $"[{input.X}, {input.Y}, {input.Z}]"; }
        
        public static bool TryParse(string input, out Vector3d result) {
            result = Zero;

            string[] split = input.Split('x', ',');

            if (split.Length != 3) return false;

            for (int i = 0; i < split.Length; i++) {
                split[i] = split[i].Trim(' ', '[', ']', '{', '}', '(', ')');
            }

            if (double.TryParse(split[0], out result.X) && double.TryParse(split[1], out result.Y) && double.TryParse(split[2], out result.Z)) {
                return true;
            }
            
            result = Zero;
            return false;
        }

        private static Vector3d _one = new Vector3d { X = 1, Y = 1, Z = 1 };
        private static Vector3d _zero = new Vector3d { X = 0, Y = 0 };

        private static Vector3d _unitX = new Vector3d { X = 1, Y = 0, Z = 0 };
        private static Vector3d _unitY = new Vector3d { X = 0, Y = 1, Z = 0 };
        private static Vector3d _unitZ = new Vector3d { X = 0, Y = 0, Z = 1 };

        public static Vector3d UnitX => _unitX;
        public static Vector3d UnitY => _unitY;
        public static Vector3d UnitZ => _unitZ;

        public static Vector3d Width => _unitX;
        public static Vector3d Height => _unitY;
        public static Vector3d Depth => _unitZ;

        public static Vector3d One => _one;
        public static Vector3d Zero => _zero;

        public static Vector3d Up => -_unitY;
        public static Vector3d Down => _unitY;

        public static Vector3d Left => -_unitX;
        public static Vector3d Right => _unitX;
        
        public static Vector3d Forward => -_unitZ;
        public static Vector3d Backward => -_unitZ;
    }
    
    public struct Vector3i64 {
        public long X;
        public long Y;
        public long Z;

        public Vector3i64(long XYZ) {
            this.X = XYZ;
            this.Y = XYZ;
            this.Z = XYZ;
        }
        public Vector3i64(double X,double Y, double Z) {
            this.X = (long)X;
            this.Y = (long)Y;
            this.Z = (long)Z;
        }
        public Vector3i64(long X, long Y, long Z) {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }
        public Vector3i64(float X, float Y, float Z) {
            this.X = (long)X;
            this.Y = (long)Y;
            this.Z = (long)Z;
        }

        public Vector3i64(Vector3 vector) {
            this.X = (long)vector.X;
            this.Y = (long)vector.Y;
            this.Z = (long)vector.Z;
        }
        public Vector3i64(Vector2 vector) {
            this.X = (long)vector.X;
            this.Y = (long)vector.Y;
            this.Z = 0;
        }
        public Vector3i64(Vector3 vector, long Z) {
            this.X = (long)vector.X;
            this.Y = (long)vector.Y;
            this.Z = Z;
        }
        public Vector3i64(Vector2i64 vector) {
            this.X = vector.X;
            this.Y = vector.Y;
            this.Z = 0;
        }
        public Vector3i64(Vector2i64 vector, long Z) {
            this.X = vector.X;
            this.Y = vector.Y;
            this.Z = Z;
        }

        public Vector3i64(Point point) {
            this.X = point.X;
            this.Y = point.Y;
            this.Z = 0;
        }
        public Vector3i64(Point point, long Z) {
            this.X = point.X;
            this.Y = point.Y;
            this.Z = Z;
        }

        public UInt64 Length() {
            UInt128 x2 = (UInt128)X * (UInt128)X;
            UInt128 y2 = (UInt128)Y * (UInt128)Y;
            UInt128 z2 = (UInt128)Z * (UInt128)Z;
            return (UInt64)(x2 + y2 + z2).Sqrt();
        }

        public static double Length(Vector3i64 a, Vector3i64 b) {
            double x2 = b.X - a.X; x2 *= x2;
            double y2 = b.Y - a.Y; y2 *= y2;
            double z2 = b.Z - a.Z; z2 *= z2;
            return Math.Sqrt(x2 + y2 + z2);
        }

        public static double Length(Vector3i64 a, Vector3 b) {
            double x2 = b.X - a.X; x2 *= x2;
            double y2 = b.Y - a.Y; y2 *= y2;
            double z2 = b.Z - a.Z; z2 *= z2;
            return Math.Sqrt(x2 + y2 + z2);
        }
        
        public static explicit operator Vector3i64(Vector3 v) {
            return new Vector3i64((long)v.X, (long)v.Y, (long)v.Z);
        }

        #region long
        public static Vector3i64 operator -(Vector3i64 a, long b) => new Vector3i64() { X = a.X - b, Y = a.Y - b, Z = a.Y - b };
        public static Vector3i64 operator +(Vector3i64 a, long b) => new Vector3i64() { X = a.X + b, Y = a.Y + b, Z = a.Y + b };
        public static Vector3i64 operator *(Vector3i64 a, long b) => new Vector3i64() { X = a.X * b, Y = a.Y * b, Z = a.Y * b };
        public static Vector3i64 operator /(Vector3i64 a, long b) => new Vector3i64() { X = a.X / b, Y = a.Y / b, Z = a.Y / b };
        #endregion
        #region ulong
        public static Vector3i64 operator -(Vector3i64 a, ulong b) => new Vector3i64() { X = a.X - (uint)b, Y = a.Y - (uint)b, Z = a.Y - (uint)b };
        public static Vector3i64 operator +(Vector3i64 a, ulong b) => new Vector3i64() { X = a.X + (uint)b, Y = a.Y + (uint)b, Z = a.Y + (uint)b };
        public static Vector3i64 operator *(Vector3i64 a, ulong b) => new Vector3i64() { X = a.X * (uint)b, Y = a.Y * (uint)b, Z = a.Y * (uint)b };
        public static Vector3i64 operator /(Vector3i64 a, ulong b) => new Vector3i64() { X = a.X / (uint)b, Y = a.Y / (uint)b, Z = a.Y / (uint)b };
        #endregion
        #region int
        public static Vector3i64 operator -(Vector3i64 a, int b) => new Vector3i64() { X = a.X - b, Y = a.Y - b, Z = a.Y - b };
        public static Vector3i64 operator +(Vector3i64 a, int b) => new Vector3i64() { X = a.X + b, Y = a.Y + b, Z = a.Y + b };
        public static Vector3i64 operator *(Vector3i64 a, int b) => new Vector3i64() { X = a.X * b, Y = a.Y * b, Z = a.Y * b };
        public static Vector3i64 operator /(Vector3i64 a, int b) => new Vector3i64() { X = a.X / b, Y = a.Y / b, Z = a.Y / b };

        #endregion
        #region Vector3i64
        public static Vector3i64 operator -(Vector3i64 a) => new Vector3i64() { X = -a.X, Y = -a.Y, Z = -a.Z};
        public static Vector3i64 operator -(Vector3i64 a, Vector3i64 b) => new Vector3i64() { X = a.X - b.X, Y = a.Y - b.Y, Z = a.Y - b.Z };
        public static Vector3i64 operator +(Vector3i64 a, Vector3i64 b) => new Vector3i64() { X = a.X + b.X, Y = a.Y + b.Y, Z = a.Y + b.Z };
        public static Vector3i64 operator *(Vector3i64 a, Vector3i64 b) => new Vector3i64() { X = a.X * b.X, Y = a.Y * b.Y, Z = a.Y * b.Z };
        public static Vector3i64 operator /(Vector3i64 a, Vector3i64 b) => new Vector3i64() { X = a.X / b.X, Y = a.Y / b.Y, Z = a.Y / b.Z };
        #endregion
        
        #region Vector3i64
        public static Vector3ui64 operator -(Vector3i64 a, Vector3ui64 b) => new Vector3ui64() { X = (uint)a.X - b.X, Y =(uint)a.Y - b.Y, Z =(uint)a.Z - b.Z };
        public static Vector3ui64 operator +(Vector3i64 a, Vector3ui64 b) => new Vector3ui64() { X = (uint)a.X + b.X, Y =(uint)a.Y + b.Y, Z =(uint)a.Z + b.Z };
        public static Vector3ui64 operator *(Vector3i64 a, Vector3ui64 b) => new Vector3ui64() { X = (uint)a.X * b.X, Y =(uint)a.Y * b.Y, Z =(uint)a.Z * b.Z };
        public static Vector3ui64 operator /(Vector3i64 a, Vector3ui64 b) => new Vector3ui64() { X = (uint)a.X / b.X, Y =(uint)a.Y / b.Y, Z =(uint)a.Z / b.Z };
        #endregion
        
        #region float
        public static Vector3 operator -(Vector3i64 a, float b) => new Vector3(a.X - b, a.Y - b, a.Z - b);
        public static Vector3 operator +(Vector3i64 a, float b) => new Vector3(a.X + b, a.Y + b, a.Z + b);
        public static Vector3 operator *(Vector3i64 a, float b) => new Vector3(a.X * b, a.Y * b, a.Z * b);
        public static Vector3 operator /(Vector3i64 a, float b) => new Vector3(a.X / b, a.Y / b, a.Z / b);
        public static Vector3 operator *(float b, Vector3i64 a) => new Vector3(a.X * b, a.Y * b, a.Z * b);

        #endregion
        #region Vector2
        public static Vector3i64 operator *(Vector3i64 a, Vector3 b) => new Vector3i64(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        public static Vector3i64 operator /(Vector3i64 a, Vector3 b) => new Vector3i64(a.X / b.X, a.Y / b.Y, a.Z / b.Z);
        public static Vector3i64 operator -(Vector3i64 a, Vector3 b) => new Vector3i64(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        public static Vector3i64 operator +(Vector3i64 a, Vector3 b) => new Vector3i64(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

        #endregion
        
        public static bool operator ==(Vector3i64 a, Vector3i64 b) => (a.X == b.X && a.Y == b.Y);
        public static bool operator !=(Vector3i64 a, Vector3i64 b) => (a.X != b.X || a.Y != b.Y);

        public Vector2i XY => new Vector2i(X, Y);
        public Vector2i XZ => new Vector2i(X, Z);
        public Vector2i ZY => new Vector2i(Z, Y);
        
        public override string ToString() => $"{{ {X} : {Y} : {Z} }}";
        public string ToXString() => $"{X}x{Y}x{Z}";
        public static string simple_string(Vector3i input) { return $"{input.X}, {input.Y}, {input.Z}"; }
        public static string simple_string_brackets(Vector3i input) { return $"[{input.X}, {input.Y}, {input.Z}]"; }
        
        public static bool TryParse(string input, out Vector3i result) {
            result = Zero;

            string[] split = input.Split('x', ',');

            if (split.Length != 3) return false;

            for (int i = 0; i < split.Length; i++) {
                split[i] = split[i].Trim(' ', '[', ']', '{', '}', '(', ')');
            }

            if (int.TryParse(split[0], out result.X) && int.TryParse(split[1], out result.Y) && int.TryParse(split[2], out result.Z)) {
                return true;
            }
            
            result = Zero;
            return false;
        }

        private static Vector3i _one = new Vector3i { X = 1, Y = 1, Z = 1 };
        private static Vector3i _zero = new Vector3i { X = 0, Y = 0 };

        private static Vector3i _unitX = new Vector3i { X = 1, Y = 0, Z = 0 };
        private static Vector3i _unitY = new Vector3i { X = 0, Y = 1, Z = 0 };
        private static Vector3i _unitZ = new Vector3i { X = 0, Y = 0, Z = 1 };

        public static Vector3i UnitX => _unitX;
        public static Vector3i UnitY => _unitY;
        public static Vector3i UnitZ => _unitZ;

        public static Vector3i Width => _unitX;
        public static Vector3i Height => _unitY;
        public static Vector3i Depth => _unitZ;

        public static Vector3i One => _one;
        public static Vector3i Zero => _zero;

        public static Vector3i Up => -_unitY;
        public static Vector3i Down => _unitY;

        public static Vector3i Left => -_unitX;
        public static Vector3i Right => _unitX;
        
        public static Vector3i Forward => -_unitZ;
        public static Vector3i Backward => -_unitZ;
    }
    
    
    public struct Vector3ui64 {
        public UInt64 X;
        public UInt64 Y;
        public UInt64 Z;
        
        public Vector3ui64(UInt64 XYZ) {
            this.X = XYZ;
            this.Y = XYZ;
            this.Z = XYZ;
        }
        public Vector3ui64(UInt64 X, UInt64 Y, UInt64 Z) {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }
        public Vector3ui64(float X, float Y, float Z) {
            this.X = (UInt64)X;
            this.Y = (UInt64)Y;
            this.Z = (UInt64)Z;
        }
        public Vector3ui64(double X, double Y, double Z) {
            this.X = (UInt64)X;
            this.Y = (UInt64)Y;
            this.Z = (UInt64)Z;
        }

        public Vector3ui64(Vector3i vector) {
            this.X = (UInt64)vector.X;
            this.Y = (UInt64)vector.Y;
            this.Z = (UInt64)vector.Z;
        }
        
        public Vector3ui64(Vector2i vector, int Z) {
            this.X = (UInt64)vector.X;
            this.Y = (UInt64)vector.Y;
            this.Z = (UInt64)Z;
        }
        
        public Vector3ui64(Vector2i64 vector, Int64 Z) {
            this.X = (UInt64)vector.X;
            this.Y = (UInt64)vector.Y;
            this.Z = (UInt64)Z;
        }
        
        public Vector3ui64(Vector2ui64 vector, UInt64 Z) {
            this.X = vector.X;
            this.Y = vector.Y;
            this.Z = Z;
        }
        
        public Vector3ui64(Vector3i64 vector) {
            this.X = (UInt64)vector.X;
            this.Y = (UInt64)vector.Y;
            this.Z = (UInt64)vector.Z;
        }
        
        public static explicit operator Vector3ui64(Vector3i64 v) {
            return new Vector3ui64((UInt64)v.X, (UInt64)v.Y, (UInt64)v.Z);
        }

        #region UInt64
        public static Vector3ui64 operator -(Vector3ui64 a, UInt64 b) => new Vector3ui64() { X = a.X - b, Y = a.Y - b, Z = a.Z - b };
        public static Vector3ui64 operator +(Vector3ui64 a, UInt64 b) => new Vector3ui64() { X = a.X + b, Y = a.Y + b, Z = a.Z + b };
        public static Vector3ui64 operator *(Vector3ui64 a, UInt64 b) => new Vector3ui64() { X = a.X * b, Y = a.Y * b, Z = a.Z * b };
        public static Vector3ui64 operator /(Vector3ui64 a, UInt64 b) => new Vector3ui64() { X = a.X / b, Y = a.Y / b, Z = a.Z / b };
        #endregion
        
        #region int
        public static Vector3ui64 operator -(Vector3ui64 a, int b) => new Vector3ui64() { X = a.X - (ulong)b, Y = a.Y - (ulong)b, Z = a.Z - (ulong)b };
        public static Vector3ui64 operator +(Vector3ui64 a, int b) => new Vector3ui64() { X = a.X + (ulong)b, Y = a.Y + (ulong)b, Z = a.Z + (ulong)b };
        public static Vector3ui64 operator *(Vector3ui64 a, int b) => new Vector3ui64() { X = a.X * (ulong)b, Y = a.Y * (ulong)b, Z = a.Z * (ulong)b };
        public static Vector3ui64 operator /(Vector3ui64 a, int b) => new Vector3ui64() { X = a.X / (ulong)b, Y = a.Y / (ulong)b, Z = a.Z / (ulong)b };

        #endregion
        #region Vector3ui64
        public static Vector3ui64 operator -(Vector3ui64 a) => new Vector3ui64() { X = 0 - a.X, Y = 0 - a.Y, Z = 0 - a.Z };
        public static Vector3ui64 operator -(Vector3ui64 a, Vector3ui64 b) => new Vector3ui64() { X = a.X - b.X, Y = a.Y - b.Y, Z = a.Z - b.Z };
        public static Vector3ui64 operator +(Vector3ui64 a, Vector3ui64 b) => new Vector3ui64() { X = a.X + b.X, Y = a.Y + b.Y, Z = a.Z + b.Z };
        public static Vector3ui64 operator *(Vector3ui64 a, Vector3ui64 b) => new Vector3ui64() { X = a.X * b.X, Y = a.Y * b.Y, Z = a.Z * b.Z };
        public static Vector3ui64 operator /(Vector3ui64 a, Vector3ui64 b) => new Vector3ui64() { X = a.X / b.X, Y = a.Y / b.Y, Z = a.Z / b.Z };
        #endregion
        #region float
        public static Vector3ui64 operator *(float b, Vector3ui64 a) => new Vector3ui64(a.X * b, a.Y * b, a.Z * b);
        public static Vector3ui64 operator -(Vector3ui64 a, float b) => new Vector3ui64(a.X - b, a.Y - b, a.Z - b);
        public static Vector3ui64 operator +(Vector3ui64 a, float b) => new Vector3ui64(a.X + b, a.Y + b, a.Z + b);
        public static Vector3ui64 operator *(Vector3ui64 a, float b) => new Vector3ui64(a.X * b, a.Y * b, a.Z * b);
        public static Vector3ui64 operator /(Vector3ui64 a, float b) => new Vector3ui64(a.X / b, a.Y / b, a.Z / b);

        #endregion
        
        public static bool operator ==(Vector3ui64 a, Vector3ui64 b) => (a.X == b.X && a.Y == b.Y);
        public static bool operator !=(Vector3ui64 a, Vector3ui64 b) => (a.X != b.X || a.Y != b.Y);
        
        public Vector2i XY => new Vector2i(X, Y);
        public Vector2i XZ => new Vector2i(X, Z);
        public Vector2i ZY => new Vector2i(Z, Y);
        
        public override string ToString() => $"{{ {X} : {Y} : {Z} }}";
        public string ToXString() => $"{X}x{Y}x{Z}";
        public static string simple_string(Vector3ui64 input) { return $"{input.X}, {input.Y}, {input.Z}"; }
        public static string simple_string_brackets(Vector3ui64 input) { return $"[{input.X}, {input.Y}, {input.Z}]"; }
        
        /*
        public static bool TryParse(string input, out Vector3ui64 result) {
            result = Zero;

            string[] split = input.Split('x', ',');

            if (split.Length != 3) return false;

            for (int i = 0; i < split.Length; i++) {
                split[i] = split[i].Trim(' ', '[', ']', '{', '}', '(', ')');
            }

            if (int.TryParse(split[0], out result.X) && int.TryParse(split[1], out result.Y) && int.TryParse(split[2], out result.Z)) {
                return true;
            }
            
            result = Zero;
            return false;
        }
        */
        
        private static Vector3ui64 _one = new Vector3ui64 { X = 1, Y = 1, Z = 1 };
        private static Vector3ui64 _zero = new Vector3ui64 { X = 0, Y = 0 };

        private static Vector3ui64 _unitX = new Vector3ui64 { X = 1, Y = 0, Z = 0 };
        private static Vector3ui64 _unitY = new Vector3ui64 { X = 0, Y = 1, Z = 0 };
        private static Vector3ui64 _unitZ = new Vector3ui64 { X = 0, Y = 0, Z = 1 };

        public static Vector3ui64 UnitX => _unitX;
        public static Vector3ui64 UnitY => _unitY;
        public static Vector3ui64 UnitZ => _unitZ;

        public static Vector3ui64 Width => _unitX;
        public static Vector3ui64 Height => _unitY;
        public static Vector3ui64 Depth => _unitZ;

        public static Vector3ui64 One => _one;
        public static Vector3ui64 Zero => _zero;

        public static Vector3ui64 Up => -_unitY;
        public static Vector3ui64 Down => _unitY;

        public static Vector3ui64 Left => -_unitX;
        public static Vector3ui64 Right => _unitX;
        
        public static Vector3ui64 Forward => -_unitZ;
        public static Vector3ui64 Backward => -_unitZ;
    }
    
    public struct Vector3ui128 {
        public UInt128 X;
        public UInt128 Y;
        public UInt128 Z;
        
        public Vector3ui128(UInt128 XYZ) {
            this.X = XYZ;
            this.Y = XYZ;
            this.Z = XYZ;
        }
        public Vector3ui128(UInt128 X, UInt128 Y, UInt128 Z) {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }
        public Vector3ui128(float X, float Y, float Z) {
            this.X = (UInt128)X;
            this.Y = (UInt128)Y;
            this.Z = (UInt128)Z;
        }
        public Vector3ui128(double X, double Y, double Z) {
            this.X = (UInt128)X;
            this.Y = (UInt128)Y;
            this.Z = (UInt128)Z;
        }

        public Vector3ui128(Vector3i vector) {
            this.X = (UInt128)vector.X;
            this.Y = (UInt128)vector.Y;
            this.Z = (UInt128)vector.Z;
        }
        
        public Vector3ui128(Vector2i vector, int Z) {
            this.X = (UInt128)vector.X;
            this.Y = (UInt128)vector.Y;
            this.Z = (UInt128)Z;
        }
        
        public Vector3ui128(Vector3i64 vector) {
            this.X = (UInt128)vector.X;
            this.Y = (UInt128)vector.Y;
            this.Z = (UInt128)vector.Z;
        }
        
        public Vector3ui128(Vector3ui64 vector) {
            this.X = (UInt128)vector.X;
            this.Y = (UInt128)vector.Y;
            this.Z = (UInt128)vector.Z;
        }
        
        public static explicit operator Vector3ui128(Vector3ui64 v) {
            return new Vector3ui128((UInt128)v.X, (UInt128)v.Y, (UInt128)v.Z);
        }
        public static explicit operator Vector3ui128(Vector3i64 v) {
            return new Vector3ui128((UInt128)v.X, (UInt128)v.Y, (UInt128)v.Z);
        }

        #region UInt128
        public static Vector3ui128 operator -(Vector3ui128 a, UInt128 b) => new Vector3ui128() { X = a.X - b, Y = a.Y - b, Z = a.Z - b };
        public static Vector3ui128 operator +(Vector3ui128 a, UInt128 b) => new Vector3ui128() { X = a.X + b, Y = a.Y + b, Z = a.Z + b };
        public static Vector3ui128 operator *(Vector3ui128 a, UInt128 b) => new Vector3ui128() { X = a.X * b, Y = a.Y * b, Z = a.Z * b };
        public static Vector3ui128 operator /(Vector3ui128 a, UInt128 b) => new Vector3ui128() { X = a.X / b, Y = a.Y / b, Z = a.Z / b };
        #endregion
        
        #region int
        public static Vector3ui128 operator -(Vector3ui128 a, int b) => new Vector3ui128() { X = a.X - (ulong)b, Y = a.Y - (ulong)b, Z = a.Z - (ulong)b };
        public static Vector3ui128 operator +(Vector3ui128 a, int b) => new Vector3ui128() { X = a.X + (ulong)b, Y = a.Y + (ulong)b, Z = a.Z + (ulong)b };
        public static Vector3ui128 operator *(Vector3ui128 a, int b) => new Vector3ui128() { X = a.X * (ulong)b, Y = a.Y * (ulong)b, Z = a.Z * (ulong)b };
        public static Vector3ui128 operator /(Vector3ui128 a, int b) => new Vector3ui128() { X = a.X / (ulong)b, Y = a.Y / (ulong)b, Z = a.Z / (ulong)b };

        #endregion
        #region Vector3ui128
        public static Vector3ui128 operator -(Vector3ui128 a) => new Vector3ui128() { X = 0 - a.X, Y = 0 - a.Y, Z = 0 - a.Z };
        public static Vector3ui128 operator -(Vector3ui128 a, Vector3ui128 b) => new Vector3ui128() { X = a.X - b.X, Y = a.Y - b.Y, Z = a.Z - b.Z };
        public static Vector3ui128 operator +(Vector3ui128 a, Vector3ui128 b) => new Vector3ui128() { X = a.X + b.X, Y = a.Y + b.Y, Z = a.Z + b.Z };
        public static Vector3ui128 operator *(Vector3ui128 a, Vector3ui128 b) => new Vector3ui128() { X = a.X * b.X, Y = a.Y * b.Y, Z = a.Z * b.Z };
        public static Vector3ui128 operator /(Vector3ui128 a, Vector3ui128 b) => new Vector3ui128() { X = a.X / b.X, Y = a.Y / b.Y, Z = a.Z / b.Z };
        #endregion
        
        public static bool operator ==(Vector3ui128 a, Vector3ui128 b) => (a.X == b.X && a.Y == b.Y);
        public static bool operator !=(Vector3ui128 a, Vector3ui128 b) => (a.X != b.X || a.Y != b.Y);
        
        public override string ToString() => $"{{ {X} : {Y} : {Z} }}";
        public string ToXString() => $"{X}x{Y}x{Z}";
        public static string simple_string(Vector3ui128 input) { return $"{input.X}, {input.Y}, {input.Z}"; }
        public static string simple_string_brackets(Vector3ui128 input) { return $"[{input.X}, {input.Y}, {input.Z}]"; }
        
        /*
        public static bool TryParse(string input, out Vector3ui128 result) {
            result = Zero;

            string[] split = input.Split('x', ',');

            if (split.Length != 3) return false;

            for (int i = 0; i < split.Length; i++) {
                split[i] = split[i].Trim(' ', '[', ']', '{', '}', '(', ')');
            }

            if (int.TryParse(split[0], out result.X) && int.TryParse(split[1], out result.Y) && int.TryParse(split[2], out result.Z)) {
                return true;
            }
            
            result = Zero;
            return false;
        }
        */
        
        private static Vector3ui128 _one = new Vector3ui128 { X = 1, Y = 1, Z = 1 };
        private static Vector3ui128 _zero = new Vector3ui128 { X = 0, Y = 0 };

        private static Vector3ui128 _unitX = new Vector3ui128 { X = 1, Y = 0, Z = 0 };
        private static Vector3ui128 _unitY = new Vector3ui128 { X = 0, Y = 1, Z = 0 };
        private static Vector3ui128 _unitZ = new Vector3ui128 { X = 0, Y = 0, Z = 1 };

        public static Vector3ui128 UnitX => _unitX;
        public static Vector3ui128 UnitY => _unitY;
        public static Vector3ui128 UnitZ => _unitZ;

        public static Vector3ui128 Width => _unitX;
        public static Vector3ui128 Height => _unitY;
        public static Vector3ui128 Depth => _unitZ;

        public static Vector3ui128 One => _one;
        public static Vector3ui128 Zero => _zero;

        public static Vector3ui128 Up => -_unitY;
        public static Vector3ui128 Down => _unitY;

        public static Vector3ui128 Left => -_unitX;
        public static Vector3ui128 Right => _unitX;
        
        public static Vector3ui128 Forward => -_unitZ;
        public static Vector3ui128 Backward => -_unitZ;
    }
    
#pragma warning restore CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
#pragma warning restore CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)

}
