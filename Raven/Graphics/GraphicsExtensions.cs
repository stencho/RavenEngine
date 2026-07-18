using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Raven.Engine;
using Raven.Engine.Collision.Shapes3D;

namespace Raven.Graphics;

public static class RenderTargetEx {
    public enum TargetMultiSampleMode : int { x1 = 0, x2 = 2, x4 = 4, x8 = 8, x16 = 16 }
    
    public static SurfaceFormat default_surface_format = SurfaceFormat.HalfVector4;
    public static DepthFormat default_depth_format = DepthFormat.None;
    public static RenderTargetUsage default_target_usage = RenderTargetUsage.PreserveContents;
    public static TargetMultiSampleMode default_multisample_mode = TargetMultiSampleMode.x1;
    public static bool default_shared_mode = false;
    
    public static RenderTarget2D create(int resolution_x, int resolution_y, 
        SurfaceFormat format = SurfaceFormat.HalfVector4,
        DepthFormat depth_format = DepthFormat.None, 
        RenderTargetUsage usage = RenderTargetUsage.PreserveContents,
        TargetMultiSampleMode multisample = TargetMultiSampleMode.x1,
        bool shared = false
        ) => new (State.graphics_device, resolution_x, resolution_y, false,
            format, depth_format, (int)multisample, usage, shared);
    
    public static RenderTarget2D create(int resolution_x, int resolution_y) => create(resolution_x, resolution_y, default_surface_format, default_depth_format, default_target_usage, default_multisample_mode, default_shared_mode);
    public static RenderTarget2D create(Vector2i resolution) => create(resolution.X, resolution.Y, default_surface_format, default_depth_format, default_target_usage, default_multisample_mode, default_shared_mode);
    
    public static RenderTarget2D create(float resolution_x, float resolution_y, 
        SurfaceFormat format = SurfaceFormat.HalfVector4,
        DepthFormat depth_format = DepthFormat.None, 
        RenderTargetUsage usage = RenderTargetUsage.PreserveContents,
        TargetMultiSampleMode multisample = TargetMultiSampleMode.x1,
        bool shared = false
        ) => create((int)resolution_x, (int)resolution_y, format, depth_format, usage, multisample, shared);
    
    public static RenderTarget2D create(Vector2i resolution, 
        SurfaceFormat format = SurfaceFormat.HalfVector4,
        DepthFormat depth_format = DepthFormat.None, 
        RenderTargetUsage usage = RenderTargetUsage.PreserveContents,
        TargetMultiSampleMode multisample = TargetMultiSampleMode.x1,
        bool shared = false
        ) => create(resolution.X, resolution.Y, format, depth_format, usage, multisample, shared);
    
    public static void use(this RenderTarget2D RT2D) {
        State.graphics_device.SetRenderTarget(RT2D);
    }
    
    public static void draw_to(this RenderTarget2D RT2D, Action<RenderTarget2D> action) {
        action.Invoke(RT2D);
    }

    public static void clear(this RenderTarget2D RT2D) {
        State.graphics_device.SetRenderTarget(RT2D);
        State.graphics_device.Clear(Color.Transparent);
    }
    public static void clear(this RenderTarget2D RT2D, Color color) {
        State.graphics_device.SetRenderTarget(RT2D);
        State.graphics_device.Clear(color);
    }
}

public static class Extensions3D {
    public static void ForAllMeshParts(this Model model, Action<VertexBuffer, IndexBuffer> action) {
        for (int mesh_index = 0; mesh_index < model.Meshes.Count; mesh_index++) {
            for (int part_index = 0; part_index < model.Meshes[mesh_index].MeshParts.Count; part_index++) {
                action(model.Meshes[mesh_index].MeshParts[part_index].VertexBuffer,
                    model.Meshes[mesh_index].MeshParts[part_index].IndexBuffer);
            }
        }
    }

    public static Polyhedron ToShape3D(this BoundingFrustum frustum) {
        return new Polyhedron(frustum.GetCorners());
    }

    public static bool contains_transparency(this Texture2D tx) {
        Color[] color_data = new Color[tx.Width * tx.Height];
        tx.GetData(color_data);
        
        bool does = false;
        foreach (Color c in color_data) {
            if (c.A != 255) {
                does = true;
                break;
            }
        }

        color_data = null;
        return does;
    } 

} 