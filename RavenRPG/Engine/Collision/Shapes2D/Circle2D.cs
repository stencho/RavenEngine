using Microsoft.Xna.Framework;
using RavenRPG.Renderer.Drawing;

namespace RavenRPG.Engine.Collision.Shapes2D {
    public class Circle2D : Collision2D.Shape2D {
        Vector2 pos;
        public Vector2 origin => pos;
        float r;
        public float radius => r;

        public Color debug_color { get; set; } = Color.Red;

        public Circle2D(Vector2 pos, float r) {
            this.pos = pos;
            this.r = r;
        }

        public void Draw(Color color) {
            Draw2D.circle(pos, r, 1f, color);
        }

        public Vector2 support(Vector2 direction_n, bool normalize = true, bool transform = true) {
            return origin + (Vector2.Normalize(direction_n) * radius);
        }

        public float FindRadius() {
            return r;
        }

        public void SetPosition(Vector2 position) {
            pos = position;
        }

        public void TranslatePosition(Vector2 distance) {
            pos += distance;
        }
    }
}
