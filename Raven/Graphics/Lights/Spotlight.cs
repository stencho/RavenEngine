using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Raven.Engine;
using Raven.Graphics.Drawing3D;

namespace Raven.Graphics.Lights {
    public class SpotLight : DynamicLight {
        public LightType type => LightType.SPOT;
        public int depth_map_resolution => gvars.get_int("light_spot_resolution");

        RenderTarget2D _depth;
        public RenderTarget2D depth_map => _depth;

        public string shader => "spotlight";

        public float far_clip => gvars.get_float("light_far");
        public float near_clip => gvars.get_float("light_near");

        public float fov { get; set; } = (MathHelper.Pi / 4f) + 0.01f;

        public BoundingFrustum frustum { get; set; }
        public Vector3 position { get; set; } = (Vector3.Up * 15.91909f) + (Vector3.Forward *3.921314f);
        public Matrix orientation { get; set; } 

        public Matrix view { get; set; }
        public Matrix projection { get; set; }
        public Matrix world { get; set; }

        public Color light_color { get; set; } = Color.Wheat;

        public float radial_scale;
        public Matrix actual_scale;

        public float angle_cos => (float)Math.Cos(fov);

        public SpotLight() {
            _depth = new RenderTarget2D(State.graphics_device, depth_map_resolution, depth_map_resolution, false, SurfaceFormat.Single, DepthFormat.Depth24);

            orientation = Matrix.CreateFromAxisAngle(Vector3.Left, MathHelper.ToRadians(90f));

            view = Matrix.CreateLookAt(position, position + orientation.Forward, orientation.Up);
            projection = Matrix.CreatePerspectiveFieldOfView(fov, 1f, near_clip, far_clip);

            radial_scale = (float)Math.Tan((double)fov) * far_clip;
            actual_scale = Matrix.CreateScale(radial_scale, radial_scale, far_clip);

            world = actual_scale * orientation * Matrix.CreateTranslation(position);
            
            frustum = new BoundingFrustum(view * projection);

            //gvars.add_change_action("light_spot_resolution", change_depth_buffer_size);
        }

        public void change_depth_buffer_size() {
            lock (_depth) {
                _depth = new RenderTarget2D(State.graphics_device, depth_map_resolution, depth_map_resolution, false, SurfaceFormat.Single, DepthFormat.Depth24);
            }
        }

        public void update() {
            view = Matrix.CreateLookAt(position, position + orientation.Forward, orientation.Up);
            projection = Matrix.CreatePerspectiveFieldOfView(fov, 1f, near_clip, far_clip);
            
            radial_scale = (float)Math.Tan((double)fov) * far_clip;

            actual_scale = Matrix.CreateScale(radial_scale, radial_scale, far_clip);

            world = actual_scale * orientation * Matrix.CreateTranslation(position);

            frustum = new BoundingFrustum(view * projection);
        }
        
    }
}
