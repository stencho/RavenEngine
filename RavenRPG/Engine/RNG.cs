using System;
using Microsoft.Xna.Framework;

namespace RavenRPG.Engine {
    public static class RNG {
        static Random _rng = new Random();
        public static void next() => _rng.Next();
        public static void change_seed(int seed) => _rng = new Random(seed);
        public static void change_seed() => _rng = new Random();
        public static float rng_float => (float)_rng.NextDouble();
        public static float rng_float_seeded_one_off(int seed) { return (float)(new Random(seed).NextDouble()); }
        public static float rng_float_neg_one_to_one => (float)((_rng.NextDouble() - 0.5) * 2);
        
        

        public static Vector2 rng_v2 => new Vector2(rng_float, rng_float);
        public static Vector2 rng_v2_by_v2(Vector2 input) { return new Vector2(input.X * rng_float, input.Y * rng_float); }
        public static Vector2 rng_v2_neg_one_to_one => new Vector2(rng_float_neg_one_to_one, rng_float_neg_one_to_one);
        public static Vector3 rng_v3 => new Vector3(rng_float, rng_float, rng_float);
        public static Vector3 rng_v3_by_v3(Vector3 input) { return new Vector3(input.X * rng_float, input.Y * rng_float, input.Z * rng_float); }
        public static Vector3 rng_v3_neg_one_to_one => new Vector3(rng_float_neg_one_to_one, rng_float_neg_one_to_one, rng_float_neg_one_to_one);

        public static Vector3 rng_v3_near_v3(Vector3 center, float radius) { return center + (Vector3.Normalize(RNG.rng_v3_neg_one_to_one) * radius); } 

        

        public static double rng_double => _rng.NextDouble();

        public static int rng_int() => _rng.Next();
        public static int rng_int(int min, int max) => _rng.Next(min, max);

        public static byte rng_byte() => (byte)_rng.Next(0, 255);

        public static bool rng_bool => (_rng.NextDouble() >= 0.5);
        public static Color random_opaque_color() {
            return Color.FromNonPremultiplied((int)(255 * rng_float), (int)(255 * rng_float), (int)(255 * rng_float), 255);
        }

        public static Color similar_color(Color inputColor, float maxDifference) {
            var diff = (int)(maxDifference * 255);

            var r = int.Clamp((int)(inputColor.R + diff * ((RNG.rng_float * 2f) - 1.0f)), 0, 255);
            var g = int.Clamp((int)(inputColor.G + diff * ((RNG.rng_float * 2f) - 1.0f)), 0, 255);
            var b = int.Clamp((int)(inputColor.B + diff * ((RNG.rng_float * 2f) - 1.0f)), 0, 255);

            return Color.FromNonPremultiplied(r, g, b, 255);
        }
        public static Color RandomShadeOfGrey(float fromWhite) {
            var val = 255 - ((int)(RNG.rng_float * (255 * fromWhite)));
            return Color.FromNonPremultiplied(val, val, val, 255);
        }
    }
}
