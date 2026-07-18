using Microsoft.Xna.Framework;
using Raven.Graphics;

namespace Raven.Engine.Collision.Shapes3D;

public partial class ModelMesh : Shape3D{
    public Vector3 start_point { get; }
    public Vector3 center { get; }
    public Vector3[] get_all_points() => [center];
    public BoundingBox find_bounding_box(Matrix world) {
        throw new System.NotImplementedException();
    }

    public BoundingBox sweep_bounding_box(Matrix world, Vector3 sweep) {
        throw new System.NotImplementedException();
    }

    public shape_type shape { get; }
    public Vector3 support(Vector3 direction, Vector3 sweep) {
        throw new System.NotImplementedException();
    }

    public void draw(Camera camera, GBuffer gbuffer, Matrix world) {
        throw new System.NotImplementedException();
    }
}