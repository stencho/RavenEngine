using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Raven.Graphics;

namespace Raven.Engine {
    public class Camera {
        public Matrix view { get; set; }

        public Matrix inverse_view { get; set; }
        public Matrix projection { get; set; }
        public Matrix InverseViewProjection { get; set; }

        public Matrix orientation { get; set; } = Matrix.Identity;

        public Vector3 direction => orientation.Forward;
        public Vector3 up_direction => orientation.Up;

        public ChunkPosition chunk_position { get; set; } = new(null, Vector3.Zero);
        
        public Vector3 position { get; set; } = Vector3.Zero;
        public Vector2 position_xz { get { return new Vector2(position.X, position.Z); } }
        public Vector3 lookat_offset { get; set; } = Vector3.Zero;

        public BoundingFrustum frustum { get; set; } = new BoundingFrustum(Matrix.Identity);

        public Matrix frustum_view { get; set; }
        public Matrix frustum_projection { get; set; }

        public float near_clip { get; set; } = 0.1f;
        public float far_clip { get; set; } = 1000f;

        public float FOV { get; set; } = 90f;
        public float FOV_default { get; set; } = 90f;

        public float aspect_ratio { get; set; }

        public string name { get; set; } = "camera";

        //RenderTarget2D picture_in_picture;

        Viewport viewport;
        
        public GBuffer gbuffer;
        public RenderTarget2D render_target;

        //public Mouse_Picker mouse_picker;

        public Camera() {
            position = Vector3.Zero;
            viewport = new Viewport(0, 0, State.resolution.X, State.resolution.Y);
            //mouse_picker = new Mouse_Picker(viewport, this);
            frustum = new BoundingFrustum(view * projection);

            update_projection(State.resolution);
            State.CameraTracker.Add(this);
        }
        public Camera(Vector3 position) {
            this.position = position;
            viewport = new Viewport(0, 0, State.resolution.X, State.resolution.Y);
            //mouse_picker = new Mouse_Picker(viewport, this);
            frustum = new BoundingFrustum(view * projection);

            update_projection(State.resolution);
            State.CameraTracker.Add(this);
        }

        public Camera(Vector3 position, Vector3 facing) {
            this.position = position;
            this.orientation = Matrix.CreateLookAt(position, position + facing, Vector3.Up);
            viewport = new Viewport(0, 0, State.resolution.X, State.resolution.Y);
            //mouse_picker = new Mouse_Picker(viewport, this);
            frustum = new BoundingFrustum(view * projection);

            update_projection(State.resolution);
            State.CameraTracker.Add(this);
        }

        public Camera(Vector3 position, Matrix orientation) {
            this.position = position;
            this.orientation = orientation;
            viewport = new Viewport(0, 0, State.resolution.X, State.resolution.Y);
            //mouse_picker = new Mouse_Picker(viewport, this);
            frustum = new BoundingFrustum(view * projection);

            update_projection(State.resolution);
            State.CameraTracker.Add(this);
        }
        
        ~Camera() {
            State.CameraTracker.Remove(this);
        }

        private void update_frustum_projection() {
            frustum_projection = Matrix.CreatePerspectiveFieldOfView(
                        MathHelper.ToRadians(FOV / (aspect_ratio)), aspect_ratio, near_clip, far_clip);
        }

        public void update_projection(Vector2i res) {
            aspect_ratio = (res.X / (float)res.Y);
            viewport = new Viewport(0, 0, res.X, res.Y);
            //mouse_picker.setup(viewport, this);
            update_frustum_projection();

            projection = Matrix.CreatePerspectiveFieldOfView(
                            MathHelper.ToRadians(FOV / aspect_ratio), aspect_ratio, near_clip, far_clip);

        }
        public void update_projection_ortho(Vector2i res) {
            aspect_ratio = (res.X / (float)res.Y);
            viewport = new Viewport(0, 0, res.X, res.Y);
            //mouse_picker.setup(viewport, this);
            update_frustum_projection();

            projection = Matrix.CreatePerspectiveFieldOfView(
                            MathHelper.ToRadians(FOV / aspect_ratio), aspect_ratio, near_clip, far_clip);
        }

        public void update() {
            //orientation = Matrix.CreateLookAt(Vector3.Zero, Vector3.Normalize(position + direction), Vector3.Up);

            view = Matrix.CreateLookAt(position, position + direction + lookat_offset, Vector3.Up);

            frustum_view = Matrix.CreateLookAt(position, position + lookat_offset + (direction * (far_clip)), Vector3.Up);

            inverse_view = Matrix.Invert(view);
            InverseViewProjection = Matrix.Invert(view * projection);

            frustum.Matrix = frustum_view * frustum_projection;
        }
    }
}
