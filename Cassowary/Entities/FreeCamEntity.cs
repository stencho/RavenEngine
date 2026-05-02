using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Raven.Console;
using Raven.Engine;
using Raven.Engine.Components;
using Raven.Engine.Controls;
using Raven.Graphics.Drawing2D;
using Raven.Graphics.Drawing3D;
using Raven.Graphics.Geometry2D;
using Raven.UI;

namespace Raven.Engine.Entities;

public partial class FreeCamEntity : Entity {
    public GBufferCamera gbuffer_camera;
    
    private static double camera_x_rot = 0.0;
    private static double camera_y_rot = 0.0;
    
    private BindWatcher binds;
    private MouseWatcher mouse;
    
    Vector2i pointer_tip = (Vector2i.One * 5) + (Vector2i.Right * 5);

    private Polygon2D cursor;
    
    internal static (string bind, object[] bind_data)[]
        bind_list = [
            ("forward", [Keys.W]),
            ("left", [Keys.A]),
            ("right", [Keys.D]),
            ("backward", [Keys.S]),
            ("up", [Keys.Space]),
            ("down", [Keys.C]),
            ("drop_cam", [Keys.Home]),
            
            ("shift", [Keys.LeftShift]),
            ("ctrl", [Keys.LeftControl]),
            
            ("click", [MouseWatcher.MouseButtons.Left]),
            ("click_right", [MouseWatcher.MouseButtons.Right]),
        ];
    public FreeCamEntity() {
        gbuffer_camera = new GBufferCamera(this, State.resolution, State.super_res_scale);
        
        Components.AddComponent(this, gbuffer_camera);
        
        binds = new BindWatcher(bind_list);
        mouse = new MouseWatcher();
        
        cursor = new Polygon2D(Vector2i.One * 32, 
            pointer_tip,
            pointer_tip + (Vector2i.One * 15),
            pointer_tip + (Vector2i.Down * 15) + (Vector2i.Right * 6), 
            pointer_tip + (Vector2i.Down * 21) 
        );

        cursor.regions[sdf_region.inner].color = UIColors.Foreground;
        cursor.regions[sdf_region.inner].secondary = UIColors.Foreground50Percent;
        cursor.regions[sdf_region.inner].pattern_type = sdf_pattern.DITHER;
        cursor.regions[sdf_region.inner].dither_resolution = 4;

        cursor.regions[sdf_region.inner_border].color = UIColors.Foreground;
        cursor.regions[sdf_region.outer_border].color = UIColors.Foreground;

        cursor.inner_border_width = 1f;
        cursor.outer_border_width = 0f;
    }


    public void Initialized() {
        var cam = Components.GetFirst<GBufferCamera>().camera;

        
        cam.gbuffer.Draw2DOnTop = () => {
            if (!binds.MouseLocked && !binds.MouseLockedPrevious) {
                cursor.render(MouseWatcher.Position - pointer_tip, Vector2i.One * 32);
            } else {
                Draw2D.fill_circle(MouseWatcher.Position, 3f, Color.White);
                Draw2D.circle(MouseWatcher.Position, 3f, 2f, Color.Black);
            }
            
        };
        
    }

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

    private Camera cam = null;
    
    public void Update() {
        binds.Update();
        var camera = Components.GetFirst<GBufferCamera>().camera;

        if (binds.just_pressed("drop_cam")) {
            if (cam != null) cam.Dispose();
            cam = new Camera(camera.position, camera.orientation);
            cam.enable_gbuffer(500,500 );
            cam.enable_gbuffer_draw_to_screen(0, 500, 500, 500);
        }
        
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

    private bool toggle_mouse_lock = true;
    private bool mouse_locked = false;
    
    public void UpdateGraphics() {
        mouse.UpdateDeltas();
        
        var camera = Components.GetFirst<GBufferCamera>().camera;

        if (toggle_mouse_lock) {
            if (!mouse_locked && mouse.just_pressed(MouseWatcher.MouseButtons.Right) && !State.UI.mouse_over_UI()) {
                mouse_locked = true;
            } else if (mouse_locked && mouse.just_pressed(MouseWatcher.MouseButtons.Right)) {
                mouse_locked = false;
            }
        } else {
            mouse_locked = mouse.is_pressed(MouseWatcher.MouseButtons.Right);
        }

        if (mouse_locked) {
            mouse.LockMouse();
            
            camera_y_rot += mouse.MouseDeltaSensitivityAspectRatioCorrection.X;
            camera_x_rot += mouse.MouseDeltaSensitivityAspectRatioCorrection.Y;
            
            if (camera_x_rot > 1f)  camera_x_rot = 1f; if (camera_x_rot < -1f) camera_x_rot = -1f;
            camera_y_rot = WrapSymmetric(camera_y_rot, Math.PI);
            
            camera.orientation = Matrix.CreateRotationY((float)camera_y_rot);
            camera.orientation *= Matrix.CreateFromAxisAngle(camera.orientation.Right, (float)camera_x_rot);
        }
    }
}