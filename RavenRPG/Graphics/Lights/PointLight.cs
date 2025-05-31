using Microsoft.Xna.Framework;
using RavenRPG.Graphics.Drawing3D;

namespace RavenRPG.Graphics.Lights {
    public class PointLight : DynamicLight {

        public LightType type => LightType.POINT;
        
        public float far_clip { get; set; } = 50f;
        public float near_clip { get; set; } = 0.2f;
        
        public BoundingFrustum frustum { get; set; }

        public Vector3 position { get; set; } = (Vector3.Up * 15.91909f) + (Vector3.Backward * 9.921314f);

        public Matrix world { get; set; }

        public Color light_color { get; set; } = Color.White;

        public float radius { get; set; } = 1f;
        
        public PointLight() {
            world = Matrix.CreateScale(radius) * Matrix.CreateTranslation(position);
        }

        public PointLight(Vector3 position, float radius) {
            this.position = position;
            this.radius = radius;

            world = Matrix.CreateScale(radius) * Matrix.CreateTranslation(position);
        }

        public PointLight(Vector3 position, float radius, Color color) {
            this.position = position;
            this.radius = radius;
            this.light_color = color;

            world = Matrix.CreateScale(radius) * Matrix.CreateTranslation(position);
        }


        public void update() {
            world = Matrix.CreateScale(radius) * Matrix.CreateTranslation(position);
        }
    }
}
