using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Raven.Graphics;
using Raven.Graphics.Drawing3D;

namespace Raven.Engine {
    [GuidManagedClass]
    public partial class Camera {
        public static partial class Manager {
            
            public static string ListAllCameras {
                get {
                    string output = "[Cameras]\n";
                    foreach (var camera in cameras) {
                        output += $"  [{camera.Value.ManagedGuid}]\n";
                        output += $"   | position > {camera.Value.position.ToXString()}\n";
                        output += $"   | forward > {camera.Value.orientation.Forward.ToXString()}\n";
                        output += $"   | GBuffer > {(camera.Value.ManagedGBufferGuid != Guid.Empty ? camera.Value.ManagedGBufferGuid.ToString() : "")}\n";
                        output += "\n";
                    }
                    return output;
                }
            }

            public static void UpdateAllCameras() {
                foreach (var camera in cameras.Values) {
                    camera.update();
                }
            }
            
            public static void BuildAllViewLists() {
            
            }
        
            public static void BuildAllCameraGBuffers() {
                foreach (var camera in cameras.Values) {
                    if (camera.using_gbuffer) {
                        camera.gbuffer.prepare(camera);
                        camera.gbuffer.Draw3DLayer?.Invoke();
                        //draw vis lists here
                        Renderer.draw_lighting(camera, camera.gbuffer);
                        camera.gbuffer.Draw2DLayer?.Invoke();
                        camera.gbuffer.compose(camera);
                    }
                }
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

        public float FOV { get; set; } = 90f;
        public float FOV_default { get; set; } = 90f;

        public float aspect_ratio { get; set; }

        public string name { get; set; } = "camera";

        Viewport viewport;

        public bool using_gbuffer = false;
        public GBuffer gbuffer;
        public RenderTarget2D render_target;

        protected Guid managed_guid;
        public Guid ManagedGuid => managed_guid;

        public Guid ManagedGBufferGuid => gbuffer.ManagedGuid;

        public LinkedChunkPosition linked_chunk_position;
        public ChunkPosition current_camera_chunk => linked_chunk_position.child;
        
        //public Mouse_Picker mouse_picker;

        
        public Camera() {
            position = Vector3.Zero;
            viewport = new Viewport(0, 0, State.resolution.X, State.resolution.Y);
            //mouse_picker = new Mouse_Picker(viewport, this);
            frustum = new BoundingFrustum(view * projection);

            update_projection(State.resolution);
            managed_guid = Manager.Add(this);
        }
        public Camera(Vector3 position) {
            this.position = position;
            viewport = new Viewport(0, 0, State.resolution.X, State.resolution.Y);
            //mouse_picker = new Mouse_Picker(viewport, this);
            frustum = new BoundingFrustum(view * projection);

            update_projection(State.resolution);
            managed_guid = Manager.Add(this);
        }

        public Camera(Vector3 position, Vector3 facing) {
            this.position = position;
            this.orientation = Matrix.CreateLookAt(position, position + facing, Vector3.Up);
            viewport = new Viewport(0, 0, State.resolution.X, State.resolution.Y);
            //mouse_picker = new Mouse_Picker(viewport, this);
            frustum = new BoundingFrustum(view * projection);

            update_projection(State.resolution);
            managed_guid = Manager.Add(this);
        }

        public Camera(Vector3 position, Matrix orientation) {
            this.position = position;
            this.orientation = orientation;
            viewport = new Viewport(0, 0, State.resolution.X, State.resolution.Y);
            //mouse_picker = new Mouse_Picker(viewport, this);
            frustum = new BoundingFrustum(view * projection);

            update_projection(State.resolution);
            managed_guid = Manager.Add(this);
        }

        public void enable_gbuffer(int width, int height, float res_scale = 1f) {
            gbuffer = new GBuffer(width, height, res_scale);
            using_gbuffer = true;
            gbuffer.AttachCamera(this);
        }
        
        ~Camera() {
            Manager.Remove(managed_guid);
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
