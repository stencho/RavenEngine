using System.Buffers.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Xna.Framework;
using Raven.Engine.Worlds;
using Raven.Graphics;

namespace Raven.Engine.Components;

[ListManaged]
public partial class GBufferCamera : Component {
    public static partial class Manager {
        public static void UpdateLinkedChunkPositions() {
            foreach (GBufferCamera gbc in gbuffercameras) {
                if (gbc.parent == null) continue;
                gbc.camera_position.parent = gbc.parent.position;
                gbc.camera_position.Update();
                gbc.camera.position = gbc.camera.LinkedObjectPosition.child.position_interpolated;
            }
        }
    }
    
    public Camera camera;
    public GBuffer buffer => camera.gbuffer;

    private Vector2i _resolution = -Vector2i.One;

    private LinkedObjectPosition camera_position = new LinkedObjectPosition();
    
    public float resolution_scale {
        get { return buffer.resolution_scale; }
        set {
            if (resolution == -Vector2i.One)
                camera.gbuffer.change_resolution(State.graphics.PreferredBackBufferWidth, State.graphics.PreferredBackBufferHeight, value);    
            else
                camera.gbuffer.change_resolution(resolution, value);
        }
    }
    
    public Vector2i resolution {
        get { return _resolution; }
        set {
            if (value == -Vector2i.One)
                camera.gbuffer.change_resolution(State.graphics.PreferredBackBufferWidth, State.graphics.PreferredBackBufferHeight, resolution_scale);    
            else
                camera.gbuffer.change_resolution(value, resolution_scale);
            
            _resolution = value;
        }
    }
    
    public GBufferCamera(Entity Parent) {
        set_up_camera(Parent, resolution, 1.0f, Vector3.Zero, Matrix.CreateLookAt(Vector3.Zero, Vector3.Forward, Vector3.Up));
    }
    public GBufferCamera(Entity Parent, Vector3 camera_offset, float res_scale = 1.0f) {
        set_up_camera(Parent, resolution, res_scale, camera_offset, Matrix.CreateLookAt(Vector3.Zero, Vector3.Forward, Vector3.Up));
    }
    public GBufferCamera(Entity Parent, Vector3 camera_offset, Matrix orientation, float res_scale = 1.0f) {
        set_up_camera(Parent, resolution, res_scale, camera_offset, orientation);
    }
    public GBufferCamera(Entity Parent, Vector2i resolution, float res_scale = 1.0f) {
        set_up_camera(Parent, resolution, res_scale, Vector3.Zero, Matrix.CreateLookAt(Vector3.Zero, Vector3.Forward, Vector3.Up));
    }
    public GBufferCamera(Entity Parent, Vector2i resolution, Vector3 camera_offset, float res_scale = 1.0f) {
        set_up_camera(Parent, resolution, res_scale, camera_offset, Matrix.CreateLookAt(Vector3.Zero, Vector3.Forward, Vector3.Up));
    }
    public GBufferCamera(Entity Parent, Vector2i resolution, Vector3 camera_offset, Matrix orientation, float res_scale = 1.0f) {
        set_up_camera(Parent, resolution, res_scale, camera_offset, orientation);
    }

    void set_up_camera(Entity Parent, Vector2i resolution, float res_scale, Vector3 position, Matrix orientation) {
        parent = Parent;
        _resolution = resolution;
        
        camera = new Camera(position, orientation);
        camera.enable_gbuffer(resolution.X, resolution.Y, res_scale);

        camera_position.parent = parent.position;
        camera.LinkedObjectPosition = camera_position;
        
        if (resolution == -Vector2i.One)
            camera.gbuffer.CreateInPlace(State.graphics.PreferredBackBufferWidth, State.graphics.PreferredBackBufferHeight, res_scale);    
        else
            camera.gbuffer.CreateInPlace(resolution.X, resolution.Y, res_scale);
        
        Manager.Add(this);
    }
}