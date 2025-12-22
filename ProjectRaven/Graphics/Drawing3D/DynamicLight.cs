using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectRaven.Engine;

namespace ProjectRaven.Graphics.Drawing3D {
    public enum LightType {
        SPOT,
        POINT
    }

    public class DynamicLightRequirements {
        public static BlendState blend_state = new BlendState {
            ColorSourceBlend = Blend.One,
            ColorDestinationBlend = Blend.One,
            ColorBlendFunction = BlendFunction.Add,
            AlphaSourceBlend = Blend.One,
            AlphaDestinationBlend = Blend.One,
            AlphaBlendFunction = BlendFunction.Add
        };
    }

    public class spot_info {
        public BoundingFrustum bounds;

        public Vector3 position;

        public Matrix orientation;

        public Matrix view;
        public Matrix projection;

        public RenderTarget2D depth_map;
        public Texture2D cookie;

        public float radial_scale;
        public Matrix actual_scale;
        public float angle_cos => (float)Math.Cos(fov);

        public float fov = (MathHelper.Pi / 4f) + 0.01f;
        public float far_clip = 30f;
        public float near_clip = 0.1f;
        public float C = 5f;
        public float bias = 0.0008f;
        public bool shadows = true;

        public List<int> visible = new List<int>();

        public spot_info() {
            depth_map = new RenderTarget2D(
                    State.graphics_device,
                    gvars.get_int("light_spot_resolution"), 
                    gvars.get_int("light_spot_resolution"), 
                    false, SurfaceFormat.Single, DepthFormat.Depth24);

            cookie = Resources.GetTexture("radial_glow");

            orientation = Matrix.CreateFromAxisAngle(Vector3.Left, MathHelper.ToRadians(90f));

            view = Matrix.CreateLookAt(position, position + orientation.Forward, orientation.Up);
            projection = Matrix.CreatePerspectiveFieldOfView(fov, 1f, near_clip, far_clip);

            radial_scale = (float)Math.Tan((double)fov) * far_clip;
            actual_scale = Matrix.CreateScale(radial_scale, radial_scale, far_clip);
            
            bounds = new BoundingFrustum(view * projection);
        }

        ~spot_info() {
            depth_map.Dispose();
        }
    }
    public class point_info {
        public Vector3 position;
        public float radius;

        public bool quantize = false;

        public BoundingSphere bounds => new BoundingSphere(position, radius);
    }

    public class light {
        public const int max_visible_lights = 256;

        public LightType type;

        public Color color;

        public Matrix world = Matrix.Identity;

        public Vector3 position {
            get {
                if (type == LightType.SPOT) {
                    if (spot_info == null) return Vector3.Zero;
                    return spot_info.position;
                } else {
                    if (point_info == null) return Vector3.Zero;
                    return point_info.position;
                }
            }
            set {
                if (type == LightType.SPOT) {
                    if (spot_info == null) throw new Exception("spot_info is null");
                    spot_info.position = value;
                } else if (type == LightType.POINT) {
                    if (point_info == null) throw new Exception("point_info is null");
                    point_info.position = value;
                }
            }
        }

        public spot_info spot_info;
        public point_info point_info;

    }


    public interface DynamicLight {
        Vector3 position { get; set; }

        Matrix world { get; set; }

        Color light_color { get; set; }

        LightType type { get; }
        
        float far_clip { get;  }
        float near_clip { get; }
        

        void update();
    }
}
