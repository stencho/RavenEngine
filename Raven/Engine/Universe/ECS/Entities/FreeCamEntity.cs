using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Raven.Engine.Universes;
using Raven.Engine;
using Raven.Engine.Components;
using Raven.Engine.Controls;
using Raven.Graphics.Drawing3D;

namespace Raven.RPG.Entities;

public partial class FreeCamEntity : Entity {
    public string name { get; set; } = "FreeCam";
    
    public ChunkPosition position { get; set; }
    public ChunkPosition position_stable { get; set; }
   
    public ComponentManager Components { get; set; } 
    
    public Universe parent { get; set; }
    
    public Threads.ThreadRequestPacket update_packet { get; set; }
    
    private ControlBinds binds => parent.binds;
    private Input input => parent.input;
    
    public GBufferCamera gbuffer_camera;
    
    private static double camera_x_rot = 0.0;
    private static double camera_y_rot = 0.0;

    public FreeCamEntity(Universe universe) {
        parent = universe;
        Components = new ComponentManager(this);
        Components.AddComponent(gbuffer_camera = new GBufferCamera(State.resolution));
        
        update_packet_init();
    }

    void MovementFinalized() {
        var camera = Components.GetComponent<GBufferCamera>("Camera").camera;
        camera.position = position.offset;
    }

    public void Initialized() {
        position.movement_finalized = MovementFinalized;
    }


    private Vector3 velocity = Vector3.Zero;
    private float accel = 0.1f;
        
    public void Update() {
        var camera = Components.GetComponent<GBufferCamera>("Camera").camera;
    }

    public void AfterCollision() {
    }

    public void UpdateGraphics() {
        var camera = Components.GetComponent<GBufferCamera>("Camera").camera;
        Vector3 movement = Vector3.Zero;
        
        if (binds.pressed("forward")) movement += Vector3.Transform(Vector3.Forward, Matrix.CreateRotationY((float)camera_y_rot));
        if (binds.pressed("backward")) movement -= Vector3.Transform(Vector3.Forward, Matrix.CreateRotationY((float)camera_y_rot));
        if (binds.pressed("left")) movement += camera.orientation.Left;
        if (binds.pressed("right")) movement += camera.orientation.Right;
        if (binds.pressed("up")) movement += Vector3.Up;
        if (binds.pressed("down")) movement += Vector3.Down;

        if (movement == Vector3.Zero) velocity = Vector3.LerpPrecise(velocity, Vector3.Zero, 0.2f);
        velocity += movement * accel;
        velocity = Vector3.Clamp(velocity, -Vector3.One, Vector3.One);
        
        position.wants_movement += velocity * 0.04f;
        if (binds.pressed("click_right")) {
            Input.mouse_lock = true;
            camera_x_rot += ((input.mouse_delta.Y * 50  / GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height) );
            camera_y_rot += ((input.mouse_delta.X * 50  / GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width) );
            if (camera_x_rot > 1f)  camera_x_rot = 1f;
            if (camera_x_rot < -1f) camera_x_rot = -1f;
            camera.orientation = Matrix.CreateRotationY((float)camera_y_rot);
            camera.orientation *= Matrix.CreateFromAxisAngle(camera.orientation.Right, (float)camera_x_rot);
        } else {
            Input.mouse_lock = false;
        }
    }

}