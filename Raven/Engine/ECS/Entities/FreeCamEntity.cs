using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Raven.Console;
using Raven.Engine.Worlds;
using Raven.Engine;
using Raven.Engine.Components;
using Raven.Engine.Controls;
using Raven.Graphics.Drawing2D;
using Raven.Graphics.Drawing3D;

namespace Raven.Engine.Entities;

public partial class FreeCamEntity : Entity {
    public GBufferCamera gbuffer_camera;
    
    private static double camera_x_rot = 0.0;
    private static double camera_y_rot = 0.0;
    
    private BindWatcher binds;

    internal static (string bind, object[] bind_data)[]
        bind_list = [
            ("forward", [Keys.W]),
            ("left", [Keys.A]),
            ("right", [Keys.D]),
            ("backward", [Keys.S]),
            ("up", [Keys.Space]),
            ("down", [Keys.C]),
            
            ("shift", [Keys.LeftShift]),
            ("ctrl", [Keys.LeftControl]),
            
            ("click", [MouseWatcher.MouseButtons.Left]),
            ("click_right", [MouseWatcher.MouseButtons.Right]),
        ];
    public FreeCamEntity() {
        gbuffer_camera = new GBufferCamera(this, State.resolution, State.super_res_scale);
        
        Components.AddComponent(this, gbuffer_camera);
        
        binds = new BindWatcher(bind_list);
    }

    public void Initialized() {}

    private Vector3 velocity = Vector3.Zero;
    private float accel = 20f;
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
        binds.Update();
        var camera = Components.GetFirst<GBufferCamera>().camera;
        
        Vector3 movement = Vector3.Zero;
        if (binds.pressed("forward")) movement += Vector3.Cross(Vector3.Up, camera.orientation.Right);
        if (binds.pressed("backward")) movement -= Vector3.Cross(Vector3.Up, camera.orientation.Right);
        if (binds.pressed("left")) movement += camera.orientation.Left;
        if (binds.pressed("right")) movement += camera.orientation.Right;
        if (binds.pressed("up")) movement += Vector3.Up;
        if (binds.pressed("down")) movement += Vector3.Down;
        
        velocity = Vector3.LerpPrecise(velocity, Vector3.Zero, 10f * (float)DELTA);
        
        if (movement != Vector3.Zero) {
            movement = Vector3.Normalize(movement);
            velocity += movement * (accel * (binds.pressed("shift") ? 15f : 2f) * (binds.pressed("ctrl") ? 25f : 2f) * (float)DELTA);
        } 
        
        MoveAndSlide(velocity);
        
        binds.UpdateEnd();
    }

    public void AfterCollision() {
    }

    public void UpdateGraphics() {
        var camera = Components.GetFirst<GBufferCamera>().camera;
        if (binds.Mouse.is_pressed(MouseWatcher.MouseButtons.Right)) {
            MouseWatcher.Manager.MouseLock = true;
            float ar_h = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height /
                         (float)GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            float ar_w = (float)GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width /
                         (float)GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

            camera_y_rot += ((binds.Mouse.MouseDeltaF.X / (GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width)) * ar_w);
            camera_x_rot += ((binds.Mouse.MouseDeltaF.Y / (GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height)));
            if (camera_x_rot > 1f)  camera_x_rot = 1f; if (camera_x_rot < -1f) camera_x_rot = -1f;
            camera_y_rot = WrapSymmetric(camera_y_rot, Math.PI);
            
            camera.orientation = Matrix.CreateRotationY((float)camera_y_rot);
            camera.orientation *= Matrix.CreateFromAxisAngle(camera.orientation.Right, (float)camera_x_rot);
        } else {
            MouseWatcher.Manager.MouseLock = false;
        }
        
    }
}