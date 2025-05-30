using System;
using Microsoft.Xna.Framework;

namespace RavenRPG.Engine {
    //disable warnings for equals/hashcode overrides - works fine without
#pragma warning disable CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
#pragma warning disable CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()

    public struct Vector2i {
        public int X;
        public int Y;

        public Vector2i(int XY) {
            this.X = XY;
            this.Y = XY;
        }
        public Vector2i(int X, int Y) {
            this.X = X;
            this.Y = Y;
        }
        public Vector2i(float X, float Y) {
            this.X = (int)X;
            this.Y = (int)Y;
        }

        public Vector2i(Vector2 vector) {
            this.X = (int)vector.X;
            this.Y = (int)vector.Y;
        }

        public Vector2i(Point point) {
            this.X = point.X;
            this.Y = point.Y;
        }

        public int Length() {
            float y2 = this.Y;
            float x2 = this.X;

            y2 = (float)Math.Pow(y2, 2.0);
            x2 = (float)Math.Pow(x2, 2.0);

            return (int)Math.Sqrt(x2 + y2);
        }

        public static int Length(Vector2i a, Vector2i b) {
            float y2 = b.Y - a.Y;
            float x2 = b.X - a.X;

            y2 = (float)Math.Pow(y2, 2.0);
            x2 = (float)Math.Pow(x2, 2.0);

            return (int)Math.Sqrt(x2 + y2);
        }

        public static int Length(Vector2i a, Vector2 b) {
            float y2 = b.Y - a.Y;
            float x2 = b.X - a.X;

            y2 = (float)Math.Pow(y2, 2.0);
            x2 = (float)Math.Pow(x2, 2.0);

            return (int)Math.Sqrt(x2 + y2);
        }


        public static int Length(Vector2i a, Vector3 b) {
            float y2 = b.Y - a.Y;
            float x2 = b.Z - a.X;

            y2 = (float)Math.Pow(y2, 2.0);
            x2 = (float)Math.Pow(x2, 2.0);

            return (int)Math.Sqrt(x2 + y2);
        }

        public static explicit operator Vector2i(Vector2 v) {
            return new Vector2i((int)v.X, (int)v.Y);
        }

        #region int
        public static Vector2i operator -(Vector2i a, int b) => new Vector2i() { X = a.X - b, Y = a.Y - b };
        public static Vector2i operator +(Vector2i a, int b) => new Vector2i() { X = a.X + b, Y = a.Y + b };
        public static Vector2i operator *(Vector2i a, int b) => new Vector2i() { X = a.X * b, Y = a.Y * b };
        public static Vector2i operator /(Vector2i a, int b) => new Vector2i() { X = a.X / b, Y = a.Y / b };

        #endregion
        #region Vector2i
        public static Vector2i operator -(Vector2i a, Vector2i b) => new Vector2i() { X = a.X - b.X, Y = a.Y - b.Y };
        public static Vector2i operator -(Vector2i a) => new Vector2i() { X = -a.X, Y = -a.Y };
        public static Vector2i operator +(Vector2i a, Vector2i b) => new Vector2i() { X = a.X + b.X, Y = a.Y + b.Y };
        public static Vector2i operator *(Vector2i a, Vector2i b) => new Vector2i() { X = a.X * b.X, Y = a.Y * b.Y };
        public static Vector2i operator /(Vector2i a, Vector2i b) => new Vector2i() { X = a.X / b.X, Y = a.Y / b.Y };

        #endregion
        #region float
        public static Vector2 operator *(Vector2i a, float b) => new Vector2(a.X * b, a.Y * b);
        public static Vector2 operator *(float b, Vector2i a) => new Vector2(a.X * b, a.Y * b);
        public static Vector2 operator /(Vector2i a, float b) => new Vector2(a.X / b, a.Y / b);
        public static Vector2 operator -(Vector2i a, float b) => new Vector2(a.X - b, a.Y - b);
        public static Vector2 operator +(Vector2i a, float b) => new Vector2(a.X + b, a.Y + b);

        #endregion
        #region Vector2
        public static Vector2 operator *(Vector2 a, Vector2i b) => new Vector2(a.X * b.X, a.Y * b.Y);
        public static Vector2 operator /(Vector2 a, Vector2i b) => new Vector2(a.X / b.X, a.Y / b.Y);
        public static Vector2 operator -(Vector2 a, Vector2i b) => new Vector2(a.X - b.X, a.Y - b.Y);
        public static Vector2 operator +(Vector2 a, Vector2i b) => new Vector2(a.X + b.X, a.Y + b.Y);

        public static Vector2i operator *(Vector2i a, Vector2 b) => new Vector2i(a.X * (int)b.X, a.Y * (int)b.Y);
        public static Vector2i operator /(Vector2i a, Vector2 b) => new Vector2i(a.X / (int)b.X, a.Y / (int)b.Y);
        public static Vector2i operator -(Vector2i a, Vector2 b) => new Vector2i(a.X - (int)b.X, a.Y - (int)b.Y);
        public static Vector2i operator +(Vector2i a, Vector2 b) => new Vector2i(a.X + (int)b.X, a.Y + (int)b.Y);

        public Vector3 ToVector3XZ() => new Vector3(X, 0, Y);
        public Vector3 ToVector3XY() => new Vector3(X, Y, 0);

        public Vector2 ToVector2() => new Vector2(X, Y);
        #endregion
        #region Point
        public static Point operator *(Point b, Vector2i a) => new Point(a.X * b.X, a.Y * b.Y);
        public static Point operator /(Point b, Vector2i a) => new Point(a.X / b.X, a.Y / b.Y);
        public static Point operator -(Point b, Vector2i a) => new Point(a.X - b.X, a.Y - b.Y);
        public static Point operator +(Point b, Vector2i a) => new Point(a.X + b.X, a.Y + b.Y);

        public static Vector2i operator *(Vector2i a, Point b) => new Vector2i(a.X * (int)b.X, a.Y * (int)b.Y);
        public static Vector2i operator /(Vector2i a, Point b) => new Vector2i(a.X / (int)b.X, a.Y / (int)b.Y);
        public static Vector2i operator -(Vector2i a, Point b) => new Vector2i(a.X - (int)b.X, a.Y - (int)b.Y);
        public static Vector2i operator +(Vector2i a, Point b) => new Vector2i(a.X + (int)b.X, a.Y + (int)b.Y);

        public Point ToPoint() => new Point(X, Y);
        #endregion

        public static bool operator ==(Vector2i a, Vector2i b) => (a.X == b.X && a.Y == b.Y);
        public static bool operator !=(Vector2i a, Vector2i b) => (a.X != b.X || a.Y != b.Y);

        public override string ToString() => string.Format("{{ {0} : {1} }}", X, Y);
        public string ToXString() => string.Format("{0}x{1}", X, Y);

        public static string simple_string(Vector2i input) {
            return string.Format("{0}, {1}", input.X, input.Y);
        }
        public static string simple_string_brackets(Vector2i input) {
            return string.Format("[{0}, {1}]", input.X, input.Y);
        }
        public static bool TryParse(string input, out Vector2i result) {
            result = Zero;

            string[] split = input.Split('x', ',');

            if (split.Length != 2) return false;

            for (int i = 0; i < split.Length; i++) {
                split[i] = split[i].Trim(' ', '[', ']', '{', '}', '(', ')');
            }

            if (int.TryParse(split[0], out result.X) && int.TryParse(split[1], out result.Y)) {
                return true;
            }
            result = Zero;
            return false;
        }

        private static Vector2i _one = new Vector2i { X = 1, Y = 1 };
        private static Vector2i _zero = new Vector2i { X = 0, Y = 0 };

        private static Vector2i _unitX = new Vector2i { X = 1, Y = 0 };
        private static Vector2i _unitY = new Vector2i { X = 0, Y = 1 };

        public static Vector2i UnitX => _unitX;
        public static Vector2i UnitY => _unitY;

        public static Vector2i Width => _unitX;
        public static Vector2i Height => _unitY;

        public static Vector2i One => _one;
        public static Vector2i Zero => _zero;

        public static Vector2i Up => -_unitY;
        public static Vector2i Down => _unitY;

        public static Vector2i Right => _unitX;
        public static Vector2i Left => -_unitX;
    }
    
    public struct Vector2i64 {
        public long X;
        public long Y;

        public Vector2i64(long XY) {
            this.X = XY;
            this.Y = XY;
        }
        public Vector2i64(double X,double Y) {
            this.X = (long)X;
            this.Y = (long)Y;
        }
        public Vector2i64(long X, long Y) {
            this.X = X;
            this.Y = Y;
        }
        public Vector2i64(float X, float Y) {
            this.X = (long)X;
            this.Y = (long)Y;
        }

        public Vector2i64(Vector2 vector) {
            this.X = (long)vector.X;
            this.Y = (long)vector.Y;
        }

        public Vector2i64(Point point) {
            this.X = point.X;
            this.Y = point.Y;
        }

        public long Length() {
            float y2 = this.Y;
            float x2 = this.X;

            y2 = (float)Math.Pow(y2, 2.0);
            x2 = (float)Math.Pow(x2, 2.0);

            return (long)Math.Sqrt(x2 + y2);
        }

        public static long Length(Vector2i64 a, Vector2i64 b) {
            float y2 = b.Y - a.Y;
            float x2 = b.X - a.X;

            y2 = (float)Math.Pow(y2, 2.0);
            x2 = (float)Math.Pow(x2, 2.0);

            return (long)Math.Sqrt(x2 + y2);
        }

        public static long Length(Vector2i64 a, Vector2 b) {
            float y2 = b.Y - a.Y;
            float x2 = b.X - a.X;

            y2 = (float)Math.Pow(y2, 2.0);
            x2 = (float)Math.Pow(x2, 2.0);

            return (long)Math.Sqrt(x2 + y2);
        }


        public static long Length(Vector2i64 a, Vector3 b) {
            float y2 = b.Y - a.Y;
            float x2 = b.Z - a.X;

            y2 = (float)Math.Pow(y2, 2.0);
            x2 = (float)Math.Pow(x2, 2.0);

            return (long)Math.Sqrt(x2 + y2);
        }

        public static explicit operator Vector2i64(Vector2 v) {
            return new Vector2i64((long)v.X, (long)v.Y);
        }

        #region long
        public static Vector2i64 operator -(Vector2i64 a, long b) => new Vector2i64() { X = a.X - b, Y = a.Y - b };
        public static Vector2i64 operator +(Vector2i64 a, long b) => new Vector2i64() { X = a.X + b, Y = a.Y + b };
        public static Vector2i64 operator *(Vector2i64 a, long b) => new Vector2i64() { X = a.X * b, Y = a.Y * b };
        public static Vector2i64 operator /(Vector2i64 a, long b) => new Vector2i64() { X = a.X / b, Y = a.Y / b };
        #endregion
        #region ulong
        public static Vector2i64 operator -(Vector2i64 a, ulong b) => new Vector2i64() { X = a.X - (uint)b, Y = a.Y - (uint)b };
        public static Vector2i64 operator +(Vector2i64 a, ulong b) => new Vector2i64() { X = a.X + (uint)b, Y = a.Y + (uint)b };
        public static Vector2i64 operator *(Vector2i64 a, ulong b) => new Vector2i64() { X = a.X * (uint)b, Y = a.Y * (uint)b };
        public static Vector2i64 operator /(Vector2i64 a, ulong b) => new Vector2i64() { X = a.X / (uint)b, Y = a.Y / (uint)b };
        #endregion
        #region int
        public static Vector2i64 operator -(Vector2i64 a, int b) => new Vector2i64() { X = a.X - b, Y = a.Y - b };
        public static Vector2i64 operator +(Vector2i64 a, int b) => new Vector2i64() { X = a.X + b, Y = a.Y + b };
        public static Vector2i64 operator *(Vector2i64 a, int b) => new Vector2i64() { X = a.X * b, Y = a.Y * b };
        public static Vector2i64 operator /(Vector2i64 a, int b) => new Vector2i64() { X = a.X / b, Y = a.Y / b };

        #endregion
        #region Vector2i64
        public static Vector2i64 operator -(Vector2i64 a, Vector2i64 b) => new Vector2i64() { X = a.X - b.X, Y = a.Y - b.Y };
        public static Vector2i64 operator -(Vector2i64 a) => new Vector2i64() { X = 0-a.X, Y =0-a.Y };
        public static Vector2i64 operator +(Vector2i64 a, Vector2i64 b) => new Vector2i64() { X = a.X + b.X, Y = a.Y + b.Y };
        public static Vector2i64 operator *(Vector2i64 a, Vector2i64 b) => new Vector2i64() { X = a.X * b.X, Y = a.Y * b.Y };
        public static Vector2i64 operator /(Vector2i64 a, Vector2i64 b) => new Vector2i64() { X = a.X / b.X, Y = a.Y / b.Y };
        #endregion
        
        #region Vector2i64
        public static Vector2ui64 operator -(Vector2i64 a, Vector2ui64 b) => new Vector2ui64() { X = (uint)a.X - b.X, Y =(uint)a.Y - b.Y };
        public static Vector2ui64 operator +(Vector2i64 a, Vector2ui64 b) => new Vector2ui64() { X = (uint)a.X + b.X, Y =(uint)a.Y + b.Y };
        public static Vector2ui64 operator *(Vector2i64 a, Vector2ui64 b) => new Vector2ui64() { X = (uint)a.X * b.X, Y =(uint)a.Y * b.Y };
        public static Vector2ui64 operator /(Vector2i64 a, Vector2ui64 b) => new Vector2ui64() { X = (uint)a.X / b.X, Y =(uint)a.Y / b.Y };
        #endregion
        
        #region float
        public static Vector2 operator *(Vector2i64 a, float b) => new Vector2(a.X * b, a.Y * b);
        public static Vector2 operator *(float b, Vector2i64 a) => new Vector2(a.X * b, a.Y * b);
        public static Vector2 operator /(Vector2i64 a, float b) => new Vector2(a.X / b, a.Y / b);
        public static Vector2 operator -(Vector2i64 a, float b) => new Vector2(a.X - b, a.Y - b);
        public static Vector2 operator +(Vector2i64 a, float b) => new Vector2(a.X + b, a.Y + b);

        #endregion
        #region Vector2
        public static Vector2 operator *(Vector2 a, Vector2i64 b) => new Vector2(a.X * b.X, a.Y * b.Y);
        public static Vector2 operator /(Vector2 a, Vector2i64 b) => new Vector2(a.X / b.X, a.Y / b.Y);
        public static Vector2 operator -(Vector2 a, Vector2i64 b) => new Vector2(a.X - b.X, a.Y - b.Y);
        public static Vector2 operator +(Vector2 a, Vector2i64 b) => new Vector2(a.X + b.X, a.Y + b.Y);

        public static Vector2i64 operator *(Vector2i64 a, Vector2 b) => new Vector2i64(a.X * b.X, a.Y * b.Y);
        public static Vector2i64 operator /(Vector2i64 a, Vector2 b) => new Vector2i64(a.X / b.X, a.Y / b.Y);
        public static Vector2i64 operator -(Vector2i64 a, Vector2 b) => new Vector2i64(a.X - b.X, a.Y - b.Y);
        public static Vector2i64 operator +(Vector2i64 a, Vector2 b) => new Vector2i64(a.X + b.X, a.Y + b.Y);

        public Vector3 ToVector3XZ() => new Vector3(X, 0, Y);
        public Vector3 ToVector3XY() => new Vector3(X, Y, 0);

        public Vector2 ToVector2() => new Vector2(X, Y);
        #endregion
        
        public static bool operator ==(Vector2i64 a, Vector2i64 b) => (a.X == b.X && a.Y == b.Y);
        public static bool operator !=(Vector2i64 a, Vector2i64 b) => (a.X != b.X || a.Y != b.Y);

        public override string ToString() => string.Format("{{ {0} : {1} }}", X, Y);
        public string ToXString() => string.Format("{0}x{1}", X, Y);

        public static string simple_string(Vector2i64 input) {
            return string.Format("{0}, {1}", input.X, input.Y);
        }
        public static string simple_string_brackets(Vector2i64 input) {
            return string.Format("[{0}, {1}]", input.X, input.Y);
        }
        public static bool TryParse(string input, out Vector2i64 result) {
            result = Zero;

            string[] split = input.Split('x', ',');

            if (split.Length != 2) return false;

            for (int i = 0; i < split.Length; i++) {
                split[i] = split[i].Trim(' ', '[', ']', '{', '}', '(', ')');
            }

            if (long.TryParse(split[0], out result.X) && long.TryParse(split[1], out result.Y)) {
                return true;
            }
            result = Zero;
            return false;
        }

        private static Vector2i64 _one = new Vector2i64 { X = 1, Y = 1 };
        private static Vector2i64 _zero = new Vector2i64 { X = 0, Y = 0 };

        private static Vector2i64 _unitX = new Vector2i64 { X = 1, Y = 0 };
        private static Vector2i64 _unitY = new Vector2i64 { X = 0, Y = 1 };

        public static Vector2i64 UnitX => _unitX;
        public static Vector2i64 UnitY => _unitY;

        public static Vector2i64 Width => _unitX;
        public static Vector2i64 Height => _unitY;

        public static Vector2i64 One => _one;
        public static Vector2i64 Zero => _zero;

        public static Vector2i64 Up => -_unitY;
        public static Vector2i64 Down => _unitY;
        public static Vector2i64 Right => _unitX;
        public static Vector2i64 Left => -_unitX;
    }
    
    
    public struct Vector2ui64 {
        public UInt64 X;
        public UInt64 Y;

        public Vector2ui64(UInt64 XY) {
            this.X = XY;
            this.Y = XY;
        }
        public Vector2ui64(UInt64 X, UInt64 Y) {
            this.X = X;
            this.Y = Y;
        }
        public Vector2ui64(float X, float Y) {
            this.X = (UInt64)X;
            this.Y = (UInt64)Y;
        }
        public Vector2ui64(double X, double Y) {
            this.X = (UInt64)X;
            this.Y = (UInt64)Y;
        }

        public Vector2ui64(Vector2 vector) {
            this.X = (UInt64)vector.X;
            this.Y = (UInt64)vector.Y;
        }

        public UInt64 Length() {
            float y2 = this.Y;
            float x2 = this.X;

            y2 = (float)Math.Pow(y2, 2.0);
            x2 = (float)Math.Pow(x2, 2.0);

            return (UInt64)Math.Sqrt(x2 + y2);
        }

        public static UInt64 Length(Vector2ui64 a, Vector2ui64 b) {
            float y2 = b.Y - a.Y;
            float x2 = b.X - a.X;

            y2 = (float)Math.Pow(y2, 2.0);
            x2 = (float)Math.Pow(x2, 2.0);

            return (UInt64)Math.Sqrt(x2 + y2);
        }

        public static UInt64 Length(Vector2ui64 a, Vector2 b) {
            float y2 = b.Y - a.Y;
            float x2 = b.X - a.X;

            y2 = (float)Math.Pow(y2, 2.0);
            x2 = (float)Math.Pow(x2, 2.0);

            return (UInt64)Math.Sqrt(x2 + y2);
        }


        public static UInt64 Length(Vector2ui64 a, Vector3 b) {
            float y2 = b.Y - a.Y;
            float x2 = b.Z - a.X;

            y2 = (float)Math.Pow(y2, 2.0);
            x2 = (float)Math.Pow(x2, 2.0);

            return (UInt64)Math.Sqrt(x2 + y2);
        }

        
        public static explicit operator Vector2ui64(Vector2 v) {
            return new Vector2ui64((UInt64)v.X, (UInt64)v.Y);
        }

        #region UInt64
        public static Vector2ui64 operator -(Vector2ui64 a, UInt64 b) => new Vector2ui64() { X = a.X - b, Y = a.Y - b };
        public static Vector2ui64 operator +(Vector2ui64 a, UInt64 b) => new Vector2ui64() { X = a.X + b, Y = a.Y + b };
        public static Vector2ui64 operator *(Vector2ui64 a, UInt64 b) => new Vector2ui64() { X = a.X * b, Y = a.Y * b };
        public static Vector2ui64 operator /(Vector2ui64 a, UInt64 b) => new Vector2ui64() { X = a.X / b, Y = a.Y / b };
        #endregion
        
        #region int
        public static Vector2ui64 operator -(Vector2ui64 a, int b) => new Vector2ui64() { X = a.X - (ulong)b, Y = a.Y - (ulong)b };
        public static Vector2ui64 operator +(Vector2ui64 a, int b) => new Vector2ui64() { X = a.X + (ulong)b, Y = a.Y + (ulong)b };
        public static Vector2ui64 operator *(Vector2ui64 a, int b) => new Vector2ui64() { X = a.X * (ulong)b, Y = a.Y * (ulong)b };
        public static Vector2ui64 operator /(Vector2ui64 a, int b) => new Vector2ui64() { X = a.X / (ulong)b, Y = a.Y / (ulong)b };

        #endregion
        #region Vector2ui64
        public static Vector2ui64 operator -(Vector2ui64 a, Vector2ui64 b) => new Vector2ui64() { X = a.X - b.X, Y = a.Y - b.Y };
        public static Vector2ui64 operator -(Vector2ui64 a) => new Vector2ui64() { X = 0 - a.X, Y = 0 - a.Y };
        public static Vector2ui64 operator +(Vector2ui64 a, Vector2ui64 b) => new Vector2ui64() { X = a.X + b.X, Y = a.Y + b.Y };
        public static Vector2ui64 operator *(Vector2ui64 a, Vector2ui64 b) => new Vector2ui64() { X = a.X * b.X, Y = a.Y * b.Y };
        public static Vector2ui64 operator /(Vector2ui64 a, Vector2ui64 b) => new Vector2ui64() { X = a.X / b.X, Y = a.Y / b.Y };
        #endregion
        #region float
        public static Vector2 operator *(Vector2ui64 a, float b) => new Vector2(a.X * b, a.Y * b);
        public static Vector2 operator *(float b, Vector2ui64 a) => new Vector2(a.X * b, a.Y * b);
        public static Vector2 operator /(Vector2ui64 a, float b) => new Vector2(a.X / b, a.Y / b);
        public static Vector2 operator -(Vector2ui64 a, float b) => new Vector2(a.X - b, a.Y - b);
        public static Vector2 operator +(Vector2ui64 a, float b) => new Vector2(a.X + b, a.Y + b);

        #endregion
        #region Vector2
        
        //public static Vector2x64 operator *(Vector2x64 a, Vector2ui64 b) => new Vector2x64(a.X * b.X, a.Y * b.Y);
        //public static Vector2x64 operator /(Vector2x64 a, Vector2ui64 b) => new Vector2x64(a.X / b.X, a.Y / b.Y);
        //public static Vector2x64 operator -(Vector2x64 a, Vector2ui64 b) => new Vector2x64(a.X - b.X, a.Y - b.Y);
        //public static Vector2x64 operator +(Vector2x64 a, Vector2ui64 b) => new Vector2x64(a.X + b.X, a.Y + b.Y);

        //public static Vector2x64 operator *(Vector2ui64 a, Vector2x64 b) => new Vector2x64(a.X * b.X, a.Y * b.Y);
        //public static Vector2x64 operator /(Vector2ui64 a, Vector2x64 b) => new Vector2x64(a.X / b.X, a.Y / b.Y);
        //public static Vector2x64 operator -(Vector2ui64 a, Vector2x64 b) => new Vector2x64(a.X - b.X, a.Y - b.Y);
        //public static Vector2x64 operator +(Vector2ui64 a, Vector2x64 b) => new Vector2x64(a.X + b.X, a.Y + b.Y);

        //public static Vector2ui64 operator *(Vector2ui64 a, Vector2x64 b) => new Vector2ui64(a.X * b.X, a.Y * b.Y);
        //public static Vector2ui64 operator /(Vector2ui64 a, Vector2x64 b) => new Vector2ui64(a.X / b.X, a.Y / b.Y);
        //public static Vector2ui64 operator -(Vector2ui64 a, Vector2x64 b) => new Vector2ui64(a.X - b.X, a.Y - b.Y);
        //public static Vector2ui64 operator +(Vector2ui64 a, Vector2x64 b) => new Vector2ui64(a.X + b.X, a.Y + b.Y);

        public Vector3 ToVector3XZ() => new Vector3(X, 0, Y);
        public Vector3 ToVector3XY() => new Vector3(X, Y, 0);

        public Vector2 ToVector2() => new Vector2(X, Y);
        #endregion
        
        public static bool operator ==(Vector2ui64 a, Vector2ui64 b) => (a.X == b.X && a.Y == b.Y);
        public static bool operator !=(Vector2ui64 a, Vector2ui64 b) => (a.X != b.X || a.Y != b.Y);
        

        public override string ToString() => string.Format("{{ {0} : {1} }}", X, Y);
        public string ToXString() => string.Format("{0}x{1}", X, Y);

        public static string simple_string(Vector2ui64 input) {
            return string.Format("{0}, {1}", input.X, input.Y);
        }
        public static string simple_string_brackets(Vector2ui64 input) {
            return string.Format("[{0}, {1}]", input.X, input.Y);
        }
        public static bool TryParse(string input, out Vector2ui64 result) {
            result = Zero;

            string[] split = input.Split('x', ',');

            if (split.Length != 2) return false;

            for (int i = 0; i < split.Length; i++) {
                split[i] = split[i].Trim(' ', '[', ']', '{', '}', '(', ')');
            }

            if (UInt64.TryParse(split[0], out result.X) && UInt64.TryParse(split[1], out result.Y)) {
                return true;
            }
            result = Zero;
            return false;
        }

        private static Vector2ui64 _one = new Vector2ui64 { X = 1, Y = 1 };
        private static Vector2ui64 _zero = new Vector2ui64 { X = 0, Y = 0 };

        private static Vector2ui64 _unitX = new Vector2ui64 { X = 1, Y = 0 };
        private static Vector2ui64 _unitY = new Vector2ui64 { X = 0, Y = 1 };

        public static Vector2ui64 UnitX => _unitX;
        public static Vector2ui64 UnitY => _unitY;

        public static Vector2ui64 Width => _unitX;
        public static Vector2ui64 Height => _unitY;

        public static Vector2ui64 One => _one;
        public static Vector2ui64 Zero => _zero;

        public static Vector2ui64 Up => _unitY;
        public static Vector2ui64 Down => _zero - _unitY;

        public static Vector2ui64 Right => _unitX;
        public static Vector2ui64 Left => _zero - _unitX;
    }
    
#pragma warning restore CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
#pragma warning restore CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)

}
