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
    public partial class Camera {
        public static partial class Manager {
            public static string ListAllCameras {
                get {
                    string output = "[Cameras]\n";
                    foreach (var camera in cameras) {
                        output += $"  [{camera.Value.ManagedGuid}]\n";
                        output += $"   | position > {camera.Value.current_camera_chunk?.XYZ.ToXString()} {camera.Value.position.ToXString()}\n";
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
                        camera.gbuffer.RenderUniverse(camera);
                        camera.gbuffer.Draw3DLayer?.Invoke();
                        Renderer.draw_lighting(camera, camera.gbuffer);
                        State.graphics_device.SetRenderTarget(camera.gbuffer.rt_2D);
                        AutoRender2D.Manager.RenderAll();
                        camera.gbuffer.Draw2DLayer?.Invoke();
                        GBuffer.Manager.DrawUIToSelectedGBuffer();
                        camera.gbuffer.Draw2DLayerOverUI?.Invoke();
                        camera.gbuffer.Compose(camera);
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

        public bool use_gvar_field_of_view { get; set; }= false;
        public float field_of_view => use_gvar_field_of_view ? gvars.get_float("r_field_of_view") : _field_of_view;
        private float _field_of_view = 90f;
        
        public float aspect_ratio { get; set; }

        public string name { get; set; } = "Camera";

        Viewport viewport;

        public bool using_gbuffer = false;
        public GBuffer gbuffer;
        public RenderTarget2D render_target;

        protected Guid managed_guid;
        public Guid ManagedGuid => managed_guid;

        public Guid ManagedGBufferGuid => gbuffer.ManagedGuid;

        public LinkedObjectPosition LinkedObjectPosition;
        public EntityPosition current_camera_chunk => LinkedObjectPosition.child;
        
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
                        MathHelper.ToRadians(field_of_view/ aspect_ratio), aspect_ratio, near_clip, far_clip);
        }

        public void update_projection(Vector2i res) {
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
    }
}
