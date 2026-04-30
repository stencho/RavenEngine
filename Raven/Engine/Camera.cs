using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Raven.Graphics;
using Raven.Graphics.Drawing2D;
using Raven.Graphics.Drawing3D;

namespace Raven.Engine {
    [GuidManaged]
    public partial class Camera : IDisposable {
        public static Camera current_render_camera = null;
        
        public static partial class Manager {
            public static string ListAllCameras {
                get {
                    string output = "[Cameras]\n";
                    foreach (var camera in cameras) {
                        output += $"  [{camera.Value.ManagedGuid}]\n";
                        output += $"   | position > {camera.Value.position.ToXString()}\n";
                        output += $"   | forward > {camera.Value.orientation.Forward.ToXString()}\n";
                        if (camera.Value.using_gbuffer) output += $"   | GBuffer > {(camera.Value.ManagedGBufferGuid != Guid.Empty ? camera.Value.ManagedGBufferGuid.ToString() : "")}\n";
                        output += "\n";
                    }
                    return output;
                }
            }

            public static void UpdateAllCameras() {
                foreach (var camera in cameras.Values) {
                    camera.update_projection();
                    camera.update();
                }
            }
            
            public static void BuildAllCameraGBuffers() {
                foreach (var camera in cameras.Values) {
                    if (camera.using_gbuffer) {
                        current_render_camera = camera;
                        Renderer.render_scene_to_gbuffer();
                    }
                }

                current_render_camera = null;
            }
        }
        
        public Matrix view { get; set; }

        public Matrix inverse_view { get; set; }
        public Matrix projection { get; set; }
        public Matrix InverseViewProjection { get; set; }

        public Matrix orientation { get; set; } = Matrix.Identity;

        public Vector3 direction => orientation.Forward;
        public Vector3 up_direction => orientation.Up;
        
        public Vector3 position { get; set; } = Vector3.Zero;
        public Vector2 position_xz { get { return new Vector2(position.X, position.Z); } }
        public Vector3 lookat_offset { get; set; } = Vector3.Zero;

        public BoundingFrustum frustum { get; set; } = new BoundingFrustum(Matrix.Identity);

        public Matrix frustum_view { get; set; }
        public Matrix frustum_projection { get; set; }

        public float near_clip { get; set; } = 0.1f;
        public float far_clip { get; set; } = 10000f;

        public bool use_gvar_field_of_view { get; set; }= false;
        public float field_of_view => use_gvar_field_of_view ? gvars.get_float("r_field_of_view") : _field_of_view;
        private float _field_of_view = 90f;
        
        public float aspect_ratio {
            get {
                if (using_gbuffer) 
                    return gbuffer.aspect_ratio;
                
                return _aspect_ratio;
            }
            set {
                if (!using_gbuffer) _aspect_ratio = value;
            }
        }

        private float _aspect_ratio = 1f;
        
        private bool ar_wider_than_tall => using_gbuffer ? gbuffer.resolution.X > gbuffer.resolution.Y : aspect_ratio < 1f;

        public string name { get; set; } = "Camera";

        Viewport viewport;

        public bool using_gbuffer = false;
        public GBuffer gbuffer;
        public RenderTarget2D render_target;

        protected Guid managed_guid;
        public Guid ManagedGuid => managed_guid;

        public Guid ManagedGBufferGuid => gbuffer.ManagedGuid;

        public LinkedObjectPosition LinkedObjectPosition;
        public EntityPosition current_camera_chunk => LinkedObjectPosition.child_position;

        private bool linked_to_object => LinkedObjectPosition != null;
        
        public Entity parent_entity => LinkedObjectPosition.parent;
        public Scene parent_scene => parent_entity.parent_scene;
        
        public Camera() {
            init();
        }
        
        public Camera(Vector3 position) {
            this.position = position;
            init();
        }

        public Camera(Vector3 position, Vector3 facing) {
            this.position = position;
            this.orientation = Matrix.CreateLookAt(position, position + facing, Vector3.Up);
            init();
        }

        public Camera(Vector3 position, Matrix orientation) {
            this.position = position;
            this.orientation = orientation;
            init();
        }

        void init() {
            this.position = position;
            
            frustum = new BoundingFrustum(view * projection);

            update_projection();
            managed_guid = Manager.Add(this);
        }

        ~Camera() {
            Dispose(false);
        }

        public void enable_gbuffer(int width, int height, float res_scale = 1f) {
            gbuffer = new GBuffer(width, height, res_scale);
            
            using_gbuffer = true;
            gbuffer.AttachCamera(this);
        }

        public void enable_gbuffer_draw_to_screen(int X, int Y, int width, int height) {
            if (using_gbuffer)
                gbuffer.enable_screen_draw(new Vector2i(X,Y), new Vector2i(width, height), 1);
            
            viewport = new Viewport(X, Y, width, height);
        }
        
        private void update_frustum_projection() {
            if (ar_wider_than_tall)
                frustum_projection = Matrix.CreatePerspectiveFieldOfView(
                    MathHelper.ToRadians(field_of_view / aspect_ratio), aspect_ratio, near_clip, far_clip);
            else
                frustum_projection = Matrix.CreatePerspectiveFieldOfView(
                    MathHelper.ToRadians(field_of_view * aspect_ratio), aspect_ratio, near_clip, far_clip);
        }

        public void update_projection() {
            if (ar_wider_than_tall) {
                projection = Matrix.CreatePerspectiveFieldOfView(
                    MathHelper.ToRadians(field_of_view / aspect_ratio), aspect_ratio, near_clip, far_clip);
            } else {
                projection = Matrix.CreatePerspectiveFieldOfView(
                    MathHelper.ToRadians(field_of_view * aspect_ratio), aspect_ratio, near_clip, far_clip);
            }
            
            update_frustum_projection();
        }
        
        public void update_projection_ortho(Vector2i res) {
            if (res.X > res.Y) 
                aspect_ratio = (res.X / (float)res.Y);
            else
                aspect_ratio = (res.Y / (float)res.X);
            
            viewport = new Viewport(0, 0, res.X, res.Y);
            //mouse_picker.setup(viewport, this);
            update_frustum_projection();

            projection = Matrix.CreatePerspectiveFieldOfView(
                            MathHelper.ToRadians(field_of_view / aspect_ratio), aspect_ratio, near_clip, far_clip);
        }

        public void update() {
            //orientation = Matrix.CreateLookAt(Vector3.Zero, Vector3.Normalize(position + direction), Vector3.Up);
            
            view = Matrix.CreateLookAt(position, position + direction + lookat_offset, Vector3.Up);

            frustum_view = Matrix.CreateLookAt(position, position + lookat_offset + (direction * (far_clip)), Vector3.Up);

            inverse_view = Matrix.Invert(view);
            InverseViewProjection = Matrix.Invert(view * projection);

            frustum.Matrix = frustum_view * frustum_projection;
        }

        private void ReleaseUnmanagedResources() {
            // TODO release unmanaged resources here
        }

        private void Dispose(bool disposing) {
            ReleaseUnmanagedResources();
            if (disposing) {
                gbuffer?.Dispose();
                render_target?.Dispose();
                
                LinkedObjectPosition = null;
            
                Manager.Remove(managed_guid);
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
