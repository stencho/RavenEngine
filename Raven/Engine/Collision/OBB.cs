using Microsoft.Xna.Framework;

namespace Raven.Engine.Collision {
    public class OBB {
        Vector3 _origin;

        public Matrix orientation = Matrix.Identity;

        public Vector3 origin => _origin;
        
        public Vector3 half_scale = Vector3.One / 2f;
        
        public Color color { get; set; } = Color.White;
        public BoundingBox? bounding_box { get; set; }

        public Vector3 A => origin + (orientation.Left * half_scale.X) + (orientation.Up * half_scale.Y) + (orientation.Forward * half_scale.Z);
        public Vector3 B => origin + (orientation.Right * half_scale.X) + (orientation.Up * half_scale.Y) + (orientation.Forward * half_scale.Z);
        public Vector3 C => origin + (orientation.Left * half_scale.X) + (orientation.Down * half_scale.Y) + (orientation.Forward * half_scale.Z);
        public Vector3 D => origin + (orientation.Right * half_scale.X) + (orientation.Down * half_scale.Y) + (orientation.Forward * half_scale.Z);

        public Vector3 E => origin + (orientation.Left * half_scale.X) + (orientation.Up * half_scale.Y) + (orientation.Backward * half_scale.Z);
        public Vector3 F => origin + (orientation.Right * half_scale.X) + (orientation.Up * half_scale.Y) + (orientation.Backward * half_scale.Z);
        public Vector3 G => origin + (orientation.Left * half_scale.X) + (orientation.Down * half_scale.Y) + (orientation.Backward * half_scale.Z);
        public Vector3 H => origin + (orientation.Right * half_scale.X) + (orientation.Down * half_scale.Y) + (orientation.Backward * half_scale.Z);

        public OBB(Vector3 pos) {
            _origin = pos;

            find_bounding_box(true);
        }

        public OBB(Vector3 pos, Vector3 half_scale) {
            _origin = pos;
            this.half_scale = half_scale;

            find_bounding_box(true);
        }

        public OBB(Vector3 pos, Vector3 half_scale, Matrix orientation) {
            _origin = pos;
            this.half_scale = half_scale;
            this.orientation = orientation;

            find_bounding_box(true);
        }

        public BoundingBox find_bounding_box(bool find_new_value = false) {
            if (!bounding_box.HasValue || find_new_value)
                bounding_box = CollisionHelper.BoundingBox_around_OBB(this);

            return bounding_box.Value;
        }

        public void origin_set(Vector3 new_origin) {
            _origin = new_origin;
            find_bounding_box(true);
        }

        public void origin_translate(Vector3 distance) {
            _origin += distance;
            find_bounding_box(true);
        }
    }
}
