using Microsoft.Xna.Framework;

namespace Raven.Engine.Collision {
    public enum shape_type {
        cube,
        polyhedron,
        quad,
        tri,
        capsule,
        cylinder,
        line,
        sphere,
        dummy
    }

    public interface Shape3D {
        Vector3 start_point { get; }
        Vector3 center { get; }

        BoundingBox find_bounding_box(Matrix world);
        BoundingBox sweep_bounding_box(Matrix world, Vector3 sweep);

        shape_type shape { get; }

        Vector3 support(Vector3 direction, Vector3 sweep);

        void draw(Matrix world);
    }  
}
