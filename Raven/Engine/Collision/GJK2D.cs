using Microsoft.Xna.Framework;
using Raven.Graphics.Drawing2D;
using static Raven.Engine.Math2D;

namespace Raven.Engine.Collision {
    public class Collision2D {
        public delegate void ui_delegate();
        public delegate void ui_delegate_option(object data);

        public enum CollisionLevel {
            NONE = 0,
            NEAR = 1,
            COLLIDING = 2
        }
        public enum DebugDrawLevel {
            NONE = 0,
            SIMPLE = 1,
            MAX = 2,
            COMMENTARY = 3
        }

        public interface Shape2D {
            Vector2 origin { get; }
            Color debug_color { get; set; }

            Vector2 support(Vector2 direction_n, bool normalize = true, bool transform = true);

            float FindRadius();
            void Draw(Color color);

            void SetPosition(Vector2 position);
            void TranslatePosition(Vector2 distance);
        }
                
        public static class GJK2D {

            public static Vector2 AB(Shape2D A, Shape2D B, Vector2 dir) {
                return (A.support(dir)) - (B.support(-dir));
            }

            public struct GJKResult {
                public CollisionLevel collision_level;

                public Vector2 A;
                public Vector2 B;
                public Vector2 C;

                public Vector2 dbg_offset;

                public Color color;

                public void draw() {
                    color = Color.White;

                    if (collision_level == CollisionLevel.NONE)
                        color = Color.Red;
                    else if (collision_level == CollisionLevel.NEAR)
                        color = Color.Orange;
                    else if (collision_level == CollisionLevel.COLLIDING)
                        color = Color.LightGreen;
                    else
                        color = Color.DarkGray;

                    Draw2D.line(A, B,  color, 1f);

                    Draw2D.cross(new Vector2i(dbg_offset), 15, Color.Red);
                }
            }

            public static bool test_shapes_simple(Shape2D A, Shape2D B, out GJKResult result) {
                GJKResult res = TestShapes(A, B);
                result = res;
                return res.collision_level == CollisionLevel.COLLIDING;
            }

            public static GJKResult TestShapes(Shape2D A, Shape2D B) {
                GJKResult result = new GJKResult();
                if (A == null || B == null) {
                    result.collision_level = CollisionLevel.NONE;
                    return result;
                }
                //setup initial direction, points, etc
                Vector2 direction = Vector2.Normalize(A.origin - B.origin);
                Vector2 first_point = AB(A, B, direction);

                direction = -(first_point);

                Vector2 second_point = AB(A, B, direction);
                Vector2 third_point = second_point;
                
                Vector2 ab = first_point - second_point;
                Vector2 bc = Vector2.Zero;
                Vector2 ca = Vector2.Zero;
                Vector2 ao = -second_point;

                Vector2 p = point_of_minimum_norm(first_point, second_point, Vector2.Zero);
                Vector2 p2 = Vector2.Zero;
                Vector2 p3 = Vector2.Zero;

                Vector2 dbg_offset = (Vector2.UnitX * 1100) + (Vector2.UnitY * 500);

                result.A = (A.support(direction));
                result.B = (B.support(-direction));

                if (Vector2.Dot(second_point, direction) < 0) {
                    result.collision_level = CollisionLevel.NONE;

                } else {
                    if (same_direction_as_origin(ab, ao)) {
                        if (same_direction_as_origin(perpendicular(-ab), ao)) {
                            direction -= perpendicular(ab);
                        } else {
                            direction -= perpendicular_inverse(ab);
                        }

                        result.collision_level = CollisionLevel.NEAR;

                        third_point = AB(A, B, direction);

                        p2 = point_of_minimum_norm(second_point, third_point, Vector2.Zero);
                        p3 = point_of_minimum_norm(third_point, first_point, Vector2.Zero);

                        bc = second_point - third_point;
                        ca = third_point - first_point;
                        bool bct, cat;

                        if (same_direction_as_origin(perpendicular(-ab), ao)) {
                            bct = same_direction_as_origin(perpendicular_inverse(bc), ao);
                            cat = same_direction_as_origin(perpendicular(ca), ao);

                        } else {
                            bct = same_direction_as_origin(perpendicular(bc), ao);
                            cat = same_direction_as_origin(perpendicular_inverse(ca), ao);
                        }

                        if (bct && cat) {
                            result.collision_level = CollisionLevel.COLLIDING;
                        }

                    } else {
                        direction -= second_point;
                    }
                }
                
                result.dbg_offset = dbg_offset;

                return result;
            }
        }
    }
}
