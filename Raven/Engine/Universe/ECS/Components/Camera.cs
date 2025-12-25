using System.Buffers.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Xna.Framework;
using Raven.Graphics;

namespace Raven.Engine.Components;

[ListManagedClass]
public partial class GBufferCamera : Component {
    public static partial class Manager {
        public static void UpdateLinkedChunkPositions() {
            foreach (GBufferCamera gbc in gbuffercameras) {
                gbc.camera_position.parent = gbc.parent.position_stable;
                gbc.camera_position.Update();
                gbc.camera.position = gbc.camera.linked_chunk_position.child.offset;
            }
        }
    }
    
    public override string name { get; set; } = "Camera";

    public Camera camera;
    public GBuffer buffer => camera.gbuffer;

    private Vector2i _resolution = -Vector2i.One;

    private LinkedChunkPosition camera_position = new LinkedChunkPosition();
    
    public float resolution_scale {
        get { return buffer.resolution_scale; }
        set {
            if (resolution == -Vector2i.One)
                camera.gbuffer.change_resolution(State.graphics_device, State.graphics.PreferredBackBufferWidth, State.graphics.PreferredBackBufferHeight, value);    
            else
                camera.gbuffer.change_resolution(State.graphics_device, resolution.X, resolution.Y, value);
        }
    }
    
    public Vector2i resolution {
        get { return _resolution; }
        set {
            if (value == -Vector2i.One)
                camera.gbuffer.change_resolution(State.graphics_device, State.graphics.PreferredBackBufferWidth, State.graphics.PreferredBackBufferHeight, resolution_scale);    
            else
                camera.gbuffer.change_resolution(State.graphics_device, value.X, value.Y, resolution_scale);
            
            _resolution = value;
        }
    }
    
    public GBufferCamera() {
        set_up_camera(resolution, 1.0f, Vector3.Zero, Matrix.CreateLookAt(Vector3.Zero, Vector3.Forward, Vector3.Up));
    }
    public GBufferCamera(Vector3 camera_offset, float res_scale = 1.0f) {
        set_up_camera(resolution, res_scale, camera_offset, Matrix.CreateLookAt(Vector3.Zero, Vector3.Forward, Vector3.Up));
    }
    public GBufferCamera(Vector3 camera_offset, Matrix orientation, float res_scale = 1.0f) {
        set_up_camera(resolution, res_scale, camera_offset, orientation);
    }
    public GBufferCamera(Vector2i resolution, float res_scale = 1.0f) {
        set_up_camera(resolution, res_scale, Vector3.Zero, Matrix.CreateLookAt(Vector3.Zero, Vector3.Forward, Vector3.Up));
    }
    public GBufferCamera(Vector2i resolution, Vector3 camera_offset, float res_scale = 1.0f) {
        set_up_camera(resolution, res_scale, camera_offset, Matrix.CreateLookAt(Vector3.Zero, Vector3.Forward, Vector3.Up));
    }
    public GBufferCamera(Vector2i resolution, Vector3 camera_offset, Matrix orientation, float res_scale = 1.0f) {
        set_up_camera(resolution, res_scale, camera_offset, orientation);
    }

    void set_up_camera(Vector2i resolution, float res_scale, Vector3 position, Matrix orientation) {
        _resolution = resolution;
        
        camera = new Camera(position, orientation);
        camera.enable_gbuffer(resolution.X, resolution.Y, res_scale);
        
        camera.linked_chunk_position = camera_position;
        
        if (resolution == -Vector2i.One)
            camera.gbuffer.CreateInPlace(State.graphics.PreferredBackBufferWidth, State.graphics.PreferredBackBufferHeight, res_scale);    
        else
            camera.gbuffer.CreateInPlace(resolution.X, resolution.Y, res_scale);
    }
}