using Microsoft.Xna.Framework;
using ProjectRaven.Graphics.Drawing2D;

namespace ProjectRaven.Engine.Collision.Shapes2D {
    public class Line2D : Collision2D.Shape2D {
        public Vector2 actual_pos = Vector2.Zero;
        public Vector2 actual_A;
        public Vector2 actual_B;
        public Vector2 A => actual_pos + actual_A;
        public Vector2 B => actual_pos + actual_B;
        public Vector2 origin => (A + B) / 2;
        float r;

        public Color debug_color { get; set; } = Color.Red;

        public Line2D(Vector2 A, Vector2 B) {
            this.actual_A = A; this.actual_B = B;
        }

        public void Draw(Color color) {
            Draw2D.line(A, B, color, 2f);
        }

        public Vector2 support(Vector2 direction_n, bool normalize = true, bool transform = true) {
            if (Vector2.Dot(direction_n, A) < Vector2.Dot(direction_n, B))
                return B;
            else
                return A;
        }

        public float FindRadius() {
            return r;
        }

        public void SetPosition(Vector2 position) {
            actual_pos = position;
        }

        public void TranslatePosition(Vector2 distance) {
            actual_pos += distance;
        }
    }
}
