using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Raven.Graphics.Drawing3D;
using Raven.UI;

namespace Raven.Engine;
public static class Extensions {
    

    public static Vector2 XY(this Vector3 vec) => new Vector2(vec.X, vec.Y);
    public static Vector2 XZ(this Vector3 vec) => new Vector2(vec.X, vec.Z);
    public static Vector2 YZ(this Vector3 vec) => new Vector2(vec.Y, vec.Z);
    public static Vector2 YX(this Vector3 vec) => new Vector2(vec.Y, vec.X);
    public static Vector2 ZX(this Vector3 vec) => new Vector2(vec.Z, vec.X);
    public static Vector2 ZY(this Vector3 vec) => new Vector2(vec.Z, vec.Y);

    public static void forEach(this Vector3 vec, Action<float> action) {
        action(vec.X);
        action(vec.Y);
        action(vec.Z);
    }

    public static string ToXString(this Vector2 v2) {
        return $"{v2.X}x{v2.Y}";
    }
    public static string ToXString(this Vector3 v3) {
        return $"{v3.X}x{v3.Y}x{v3.Z}";
    }
    public static string simple_vector3_string(this Vector3 input) {
        return string.Format("{0:F2}, {1:F2}, {2:F2}", input.X, input.Y, input.Z);
    }
    public static string simple_vector3_string_full_acc(this Vector3 input) {
        return string.Format("{0}, {1}, {2}", input.X, input.Y, input.Z);
    }
    public static string simple_vector2_string(this Vector2 input) {
        return string.Format("{0:F2}, {1:F2}", input.X, input.Y);
    }
    public static string simple_vector2_string_full_acc(this Vector2 input) {
        return string.Format("{0}, {1}", input.X, input.Y);
    }
    public static string simple_vector2_x_string(this Vector2 input) {
        return string.Format("{0:F2}x{1:F2}", input.X, input.Y);
    }
    public static string simple_vector2_x_string_full_acc(this Vector2 input) {
        return string.Format("{0}x{1}", input.X, input.Y);
    }


    public static string simple_vector2_x_string_no_dec(this Vector2 input) {
        return string.Format("{0:F0}x{1:F0}", input.X, input.Y);
    }

    public static string simple_vector3_string_brackets(this Vector3 input) {
        return string.Format("[{0:F2}, {1:F2}, {2:F2}]", input.X, input.Y, input.Z);
    }
    public static string simple_vector2_string_brackets(this Vector2 input) {
        return string.Format("[{0:F2}, {1:F2}]", input.X, input.Y);
    }

    public static Vector3 ToVector3XZ(this Vector2 v) {
        return new Vector3(v.X, 0, v.Y);
    }

    public static bool contains_nan(this Vector3 a) { return (float.IsNaN(a.X) || float.IsNaN(a.Y) || float.IsNaN(a.Z)); }

    public static Vector3 dimensions (this BoundingBox bb) {
        return bb.Max - bb.Min;
    }
    public static Vector3 half_size(this BoundingBox bb) {
        return bb.Max - ((bb.Min + bb.Max) / 2f);
    }

    public static Vector3 center(this BoundingBox bb) {
        return (bb.Min + bb.Max) / 2f;
    }

    public static void draw_debug(this BoundingBox bb, Color color) {
        Draw3D.cube(A(bb), B(bb), C(bb), D(bb), E(bb), F(bb), G(bb), H(bb), color);
    }

    public static Vector2i ToVector2i(this Point p) {
        return new Vector2i(p.X, p.Y);

    }
    public static Vector2i ToVector2i(this System.Drawing.Point p) {
        return new Vector2i(p.X, p.Y);
    }
    public static Vector2i ToVector2i(this System.Drawing.Size s) {
        return new Vector2i(s.Width, s.Height);
    }

    public static Vector2i ToVector2i(this Vector2 v) { 
        return new Vector2i(v); 
    }

    public static Vector3 A (this BoundingBox bb) { return bb.center() + bb.half_size(); }
    public static Vector3 B (this BoundingBox bb) { return bb.center() + (bb.half_size() * Vector3.One - (Vector3.UnitX * 2)); }

    public static Vector3 C (this BoundingBox bb) { return bb.center() + (bb.half_size() * Vector3.One - (Vector3.UnitY * 2)); }
    public static Vector3 D (this BoundingBox bb) { return bb.center() + (bb.half_size() * Vector3.One - (Vector3.UnitX * 2) - (Vector3.UnitY * 2)); }

    public static Vector3 E (this BoundingBox bb) { return bb.center() + (bb.half_size() * Vector3.One - (Vector3.UnitZ * 2)); }
    public static Vector3 F (this BoundingBox bb) { return bb.center() + (bb.half_size() * Vector3.One - (Vector3.UnitX * 2) - (Vector3.UnitZ * 2)); }
                            
    public static Vector3 G (this BoundingBox bb) { return bb.center() + (bb.half_size() * Vector3.One - (Vector3.UnitZ * 2) - (Vector3.UnitY * 2)); }
    public static Vector3 H (this BoundingBox bb) { return bb.center() + (bb.half_size() * Vector3.One - (Vector3.UnitX * 2) - (Vector3.UnitY * 2) - (Vector3.UnitZ * 2)); }

    public static bool Intersects(this Rectangle rect, Vector2i point) {

        if (rect.Right > point.X && rect.Left < point.X && rect.Top < point.Y && rect.Bottom > rect.Y) return true;
        return false;
    }

    static List<IUIForm> tmp_list = new List<IUIForm>();
    static List<IUIForm> tmp_top = new List<IUIForm>();
    static List<IUIForm> tmp_floating = new List<IUIForm>();
    static List<IUIForm> tmp_bottom = new List<IUIForm>();

    public static string Ellipsis(this string str, int ellipsis_end) {
        if (str.Length > 3) {
            string s = "";
            s = str.Substring(0, ellipsis_end - 3);
            s += "...";
            return s;
        } else
            return new string('.', str.Length);
    }

    public static void SortWindows(this List<IUIForm> list) {
        /*
        tmp_top.Clear();
        tmp_floating.Clear();
        tmp_bottom.Clear();
        tmp_list.Clear();

        foreach (IUIForm f in list) {
            if (f.layer_state == ui_layer_state.on_bottom) {
                tmp_bottom.Add(f);
            } else if (f.layer_state == ui_layer_state.on_top) {
                tmp_top.Add(f);
            } else {
                tmp_floating.Add(f);
            }
        }

        foreach (IUIForm f in tmp_bottom) {
            tmp_list.Add(f);
        }

        foreach (IUIForm f in tmp_floating) {
            tmp_list.Add(f);
        }

        foreach (IUIForm f in tmp_top) {
            tmp_list.Add(f);
        }

        return tmp_list;
        */

        int c = 0;
        int low = 0;
        int norm = 0;
        int high = 0;

        tmp_list.Clear();

        for (int i = 0; i < list.Count; i++) {
            tmp_list.Add(list[i]);
        }

        list.Clear();

        for (int i = 0; i < tmp_list.Count; i++) {

            if (tmp_list[i].layer_state == ui_layer_state.on_bottom) {
                list.Add(tmp_list[i]);
            }
            
        }

        for (int i = 0; i < tmp_list.Count; i++) {

            if (tmp_list[i].layer_state == ui_layer_state.floating && !(tmp_list[i].has_focus || tmp_list[i].top_of_mouse_stack)) {
                list.Add(tmp_list[i]);
            }

        }
        for (int i = 0; i < tmp_list.Count; i++) {
            if (tmp_list[i].layer_state == ui_layer_state.floating && tmp_list[i].top_of_mouse_stack && !tmp_list[i].has_focus) {
                list.Add(tmp_list[i]);
            }
        }
        for (int i = 0; i < tmp_list.Count; i++) {
            if (tmp_list[i].layer_state == ui_layer_state.floating && (tmp_list[i].has_focus && tmp_list[i].top_of_mouse_stack)) {
                list.Add(tmp_list[i]);
            }

        }
        for (int i = 0; i < tmp_list.Count; i++) {
            if (tmp_list[i].layer_state == ui_layer_state.floating && tmp_list[i].has_focus && !tmp_list[i].top_of_mouse_stack) {
                list.Add(tmp_list[i]);
            }
        }
        
        for (int i = 0; i < tmp_list.Count; i++) {
            if (tmp_list[i].layer_state == ui_layer_state.on_top) {
                list.Add(tmp_list[i]);
            }
        }


        /*
        for (int i = 0; i < list.Count; i++) {
            IUIForm tmp = list[i];

            switch (list[i].layer_state) {

                case ui_layer_state.on_bottom:
                    list.Remove(list[i]);
                    if (i < low) 
                        list.Insert(low-1, tmp);
                    else
                        list.Insert(low, tmp);
                    low++;
                    break;

                case ui_layer_state.floating:
                    list.Remove(list[i]);
                    if (i < low+norm)
                        list.Insert(low+norm- 1, tmp);
                    else
                        list.Insert(low + norm, tmp);
                    norm++;
                    break;
                case ui_layer_state.on_top:
                    list.Remove(list[i]);
                    if (i < low + norm + high)
                        list.Insert(low + norm + high - 1, tmp);
                    else
                        list.Insert(low + norm + high, tmp);
                    high++;
                    break;
            }

            c++;
        }
        for (int i = list.Count - 1; i >= 0; i--) {
            IUIForm tmp = list[i];
            if (list[i].top_of_mouse_stack && list[i].layer_state == ui_layer_state.floating) {
                list.Remove(list[i]);
                if (i < low + norm)
                    list.Insert(low + norm + 1, tmp);
                else
                    list.Insert(low + norm, tmp);
                break;
            }

        }

        for (int i = list.Count-1; i >=0; i--) {
            IUIForm tmp = list[i];
            if (list[i].has_focus && list[i].layer_state == ui_layer_state.floating) {
                list.Remove(list[i]);
                list.Insert(low + norm, tmp);
            }

        }*/
    }
    public static void BringToFront(this List<IUIForm> list, IUIForm window) {
        if (list.Count <= 1) { list[0].has_focus = true;  return; }
        if (window == list[list.Count - 1]) {
            
            for (int i = 0; i < list.Count-1; i++) {
                list[i].has_focus = false;
            }

            list[list.Count - 1].has_focus = true;
            return;
        }

        int found = 0;
        for (int i = 0; i < list.Count; i++) {
            if (list[i] == window) {

                found = i;
                var tmp = window;

                list[i].has_focus = false;

                for (i = found; i < list.Count - 1; i++) {
                    list[i] = list[i + 1];
                    list[i].has_focus = false;
                }

                list[list.Count - 1] = tmp;
                list[list.Count - 1].has_focus = true;
                /*
                for (i = found; i > 0; i--) {
                    list[i] = list[i-1];
                }
                list[0] = tmp;
                */
                return;
            }
        }
    }
    private static UInt128 sqrt128(UInt128 n) {
        if (n == 0)
            return 0;

        UInt128 result = 0;
        UInt128 bit = (UInt128)1 << 126; 

        while (bit > n)
            bit >>= 2;

        while (bit != 0)
        {
            UInt128 temp = result + bit;
            result >>= 1;

            if (n >= temp)
            {
                n -= temp;
                result += bit;
            }

            bit >>= 2;
        }

        return result;
    }
    
    private static Int128 sqrt128(Int128 n) {
        if (n < 0)
            throw new ArgumentOutOfRangeException(nameof(n), "Square root of negative Int128");

        if (n == 0)
            return 0;

        UInt128 un = (UInt128)n;

        UInt128 result = 0;
        UInt128 bit = (UInt128)1 << 126;

        while (bit > un)
            bit >>= 2;

        while (bit != 0)
        {
            UInt128 temp = result + bit;
            result >>= 1;

            if (un >= temp)
            {
                un -= temp;
                result += bit;
            }

            bit >>= 2;
        }

        return (Int128)result;
    }
    static ulong sqrt64(ulong n)
    {
        if (n == 0)
            return 0;

        ulong result = 0;
        ulong bit = 1UL << 62; // highest even bit <= 64

        while (bit > n)
            bit >>= 2;

        while (bit != 0)
        {
            ulong temp = result + bit;
            result >>= 1;

            if (n >= temp)
            {
                n -= temp;
                result += bit;
            }

            bit >>= 2;
        }

        return result;
    }
    
    public static long sqrt64(long n)
    {
        if (n < 0)
            throw new ArgumentOutOfRangeException(nameof(n), "Square root of negative Int64");

        return (long)Sqrt((ulong)n);
    }
    
    public static UInt128 Sqrt(this UInt128 i) => sqrt128(i);
    public static Int128 Sqrt(this Int128 i) => sqrt128(i);
    
    public static UInt64 Sqrt(this UInt64 i) => sqrt64(i);
    public static Int64 Sqrt(this Int64 i) => sqrt64(i);
    
    #region MATRIX ROWS AND COLUMNS
    public static Vector4 row1(this Matrix m) {
      return new Vector4(m.M11, m.M21, m.M31, m.M41);
    }
    public static Vector4 row2(this Matrix m) {
      return new Vector4(m.M12, m.M22, m.M32, m.M42);
    }
    public static Vector4 row3(this Matrix m) {
      return new Vector4(m.M13, m.M23, m.M33, m.M43);
    }
    public static Vector4 row4(this Matrix m) {
      return new Vector4(m.M14, m.M24, m.M34, m.M44);
    }

    public static void r1(this Matrix m, out float m11, out float m21, out float m31, out float m41) {
        m11 = m.M11;
        m21 = m.M21;
        m31 = m.M31;
        m41 = m.M41;
    }
    public static void r2(this Matrix m, out float m12, out float m22, out float m32, out float m42) {
        m12 = m.M12;
        m22 = m.M22;
        m32 = m.M32;
        m42 = m.M42;
    }
    public static void r3(this Matrix m, out float m13, out float m23, out float m33, out float m43) {
        m13 = m.M13;
        m23 = m.M23;
        m33 = m.M33;
        m43 = m.M43;
    }
    public static void r4(this Matrix m, out float m14, out float m24, out float m34, out float m44) {
        m14 = m.M14;
        m24 = m.M24;
        m34 = m.M34;
        m44 = m.M44;
    }

    #endregion

}

