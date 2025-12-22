using System;
using Microsoft.Xna.Framework;

namespace Raven.Engine.Collision.Shapes3D {
    class DummySupport : Shape3D {
        public Vector3 start_point => Vector3.Zero;
        public Vector3 center => Vector3.Zero;
        public shape_type shape => shape_type.dummy;

        public void draw(Matrix world) {}

        public BoundingBox sweep_bounding_box(Matrix world, Vector3 sweep) {
            return new BoundingBox(Vector3.Zero, Vector3.Zero);
        }
        public BoundingBox find_bounding_box(Matrix world) {
            return new BoundingBox(Vector3.Zero, Vector3.Zero);
        }
        public Vector3 support(Vector3 direction, Vector3 sweep) {
            throw new NotImplementedException();
        }
    }
}
