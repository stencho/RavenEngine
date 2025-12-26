using System;
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
    private ControlBinds binds => parent_universe.binds;
    private Input input => parent_universe.input;
    
    public GBufferCamera gbuffer_camera;
    
    private static double camera_x_rot = 0.0;
    private static double camera_y_rot = 0.0;

    public FreeCamEntity() {
        gbuffer_camera = new GBufferCamera(this, State.resolution, State.super_res_scale);
        
        Components.AddComponent(this, gbuffer_camera);
    }

    public void Initialized() {
        
    }

    private Vector3 velocity = Vector3.Zero;
    private float accel = 0.5f;
    static double WrapMinusOneToOne(double x)
    {
        x = (x + 1.0) % 2.0;
        if (x < 0) x += 2.0;
        return x - 1.0;
    }
    static double WrapSymmetric(double x, double range)
    {
        double width = range * 2.0;
        return x - width * Math.Floor((x + range) / width);
    }
    public void Update() {
        var camera = Components.Get<GBufferCamera>("Camera").camera;
        
        Vector3 movement = Vector3.Zero;
        velocity = Vector3.LerpPrecise(velocity, Vector3.Zero, 0.2f);

        if (binds.pressed("forward")) movement += Vector3.Cross(Vector3.Up, camera.orientation.Right);
        if (binds.pressed("backward")) movement -= Vector3.Cross(Vector3.Up, camera.orientation.Right);
        if (binds.pressed("left")) movement += camera.orientation.Left;
        if (binds.pressed("right")) movement += camera.orientation.Right;
        if (binds.pressed("up")) movement += Vector3.Up;
        if (binds.pressed("down")) movement += Vector3.Down;
        
        if (movement != Vector3.Zero) {
            movement = Vector3.Normalize(movement);
            velocity += movement * accel;
        }

       // velocity = Vector3.Clamp(velocity, -Vector3.One, Vector3.One);
        
       position.MoveAndSlide(velocity * 0.05f);
    }

    public void AfterCollision() {
    }

    public void UpdateGraphics() {
        var camera = Components.Get<GBufferCamera>("Camera").camera;
        if (State.engine_binds.pressed("click_right")) {
            State.input_main_thread.mouse_lock = true;
            float ar_h = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height /
                         GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            float ar_w = (float)GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width /
                         (float)GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

            camera_x_rot += 2 * ((State.input_main_thread.mouse_delta.Y / (GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height)));
            camera_y_rot += 2 * ((State.input_main_thread.mouse_delta.X / (GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width)) * ar_w);
            if (camera_x_rot > 1f)  camera_x_rot = 1f; if (camera_x_rot < -1f) camera_x_rot = -1f;
            camera_y_rot = WrapSymmetric(camera_y_rot, Math.PI);
            
            camera.orientation = Matrix.CreateRotationY((float)camera_y_rot);
            camera.orientation *= Matrix.CreateFromAxisAngle(camera.orientation.Right, (float)camera_x_rot);
        } else {
            State.input_main_thread.mouse_lock = false;
        }
    }

}