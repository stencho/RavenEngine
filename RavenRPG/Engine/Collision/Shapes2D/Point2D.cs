using Microsoft.Xna.Framework;
using RavenRPG.Renderer.Drawing;

namespace RavenRPG.Engine.Collision.Shapes2D {
    public class Point2D : Collision2D.Shape2D {
        public Vector2 position;

        public Vector2 origin => position;

        public Color debug_color { get; set; } = Color.MediumPurple;

        public Point2D(Vector2 pos) {
            position = pos;
        }

        public void Draw(Color color) {
            Draw2D.point(position, Vector2.One*2, color);
        }

        public Vector2 support(Vector2 direction_n, bool normalize = true, bool transform = true) {
            return position;
        }

        public float FindRadius() {
            return 0;
        }

        public void SetPosition(Vector2 position) {
            this.position = position;
        }

        public void TranslatePosition(Vector2 distance) {
            this.position += position;
        }
    }
}
