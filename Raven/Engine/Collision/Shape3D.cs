using Microsoft.Xna.Framework;
using Raven.Graphics;

namespace Raven.Engine.Collision {
    public enum shape_type {
        line,
        tri,
        circle,
        quad,
        sphere,
        cube,
        cylinder,
        capsule,
        polyhedron,
        cone,
        dummy
    }

    public interface Shape3D {
        Vector3 start_point { get; }
        Vector3 center { get; }

        BoundingBox find_bounding_box(Matrix world);

        shape_type type { get; }

        //TODO NOT ALL SWEEPS ARE IMPLEMENTED
        Vector3 support(Vector3 direction, Vector3 sweep);

        void draw(Camera camera, GBuffer gbuffer, Matrix world);
        Vector3[] get_all_points();

        public BoundingBox sweep_bounding_box(Matrix world, Vector3 sweep) {
            if (sweep != Vector3.Zero) {
                return CollisionHelper.BoundingBox_around_BoundingBoxes(
                    find_bounding_box(world),
                    find_bounding_box(world * Matrix.CreateTranslation(sweep))
                );
            } else {
                return find_bounding_box(world);
            }
        }
    }  
}
