using Microsoft.Xna.Framework;
using Raven.Engine.Collision;
using Raven.Engine.Collision.Shapes3D;

namespace Raven.Engine.Controls;

public class PickerRaycast {
    public Raycasting.raycast crosshair_ray;
    public Raycasting.raycast mouse_pick_ray;

    public Line3D gjk_crosshair_ray = new Line3D();
    public Line3D gjk_mouse_pick_ray = new Line3D();

    public void update(Camera camera) {
        crosshair_ray = new Raycasting.raycast(camera.position, camera.direction);

        gjk_crosshair_ray.A = camera.position;
        gjk_crosshair_ray.B = camera.direction;

        //mouse picker stuff
        Vector3 n = new Vector3(State.input_main_thread.mouse_position.X, State.input_main_thread.mouse_position.Y, 0);
        Vector3 f = new Vector3(State.input_main_thread.mouse_position.X, State.input_main_thread.mouse_position.Y, 1);

        Vector3 near = State.viewport.Unproject(n, camera.projection, camera.view, Matrix.Identity);
        Vector3 far = State.viewport.Unproject(f, camera.projection, camera.view, Matrix.Identity);

        Vector3 d = far - near;
        d.Normalize();

        mouse_pick_ray = new Raycasting.raycast(near, d);

        gjk_mouse_pick_ray.A = near;
        gjk_mouse_pick_ray.B = d;
    }
}