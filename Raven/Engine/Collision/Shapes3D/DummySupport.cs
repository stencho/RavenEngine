using System;
using Microsoft.Xna.Framework;
using Raven.Graphics;

namespace Raven.Engine.Collision.Shapes3D {
    class DummySupport : Shape3D {
        public Vector3 start_point => Vector3.Zero;
        public Vector3 center => Vector3.Zero;
        
        public Vector3[] get_all_points() => [center];
        
        public shape_type type => shape_type.dummy;

        public void draw(Camera camera, GBuffer gbuffer,Matrix world) {}

        public BoundingBox find_bounding_box(Matrix world) {
            return new BoundingBox(Vector3.Zero, Vector3.Zero);
        }
        public Vector3 support(Vector3 direction, Vector3 sweep) {
            throw new NotImplementedException();
        }
    }
}
