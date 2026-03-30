using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using CSScripting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Raven.Engine.Collision;
using Raven.Engine.Collision.Shapes2D;
using Raven.Engine.Collision.Shapes3D;

namespace Raven.Engine.Controls;

public partial class MouseWatcher {
    public static Collision2D.Shape2D MouseCollisionObject => _mouse_coll_obj;
    static Collision2D.Shape2D _mouse_coll_obj = new Point2D(Vector2.Zero);
    
    public enum MouseButtons {
        Left,
        Right,
        Middle,
        Mouse4,
        Mouse5,
        ScrollUp,
        ScrollDown
    }

    private Vector2i mouse_delta = Vector2i.Zero;
    public Vector2i MouseDelta => mouse_delta;
    public Vector2 MouseDeltaF => mouse_delta.ToVector2();
    
    public Vector2 MouseDeltaSensitivityAspectRatioCorrection => mouse_delta_aspect_ratio_and_sensitivity();
    
    private Vector2 mouse_delta_sensitivity = Vector2.Zero;
    public Vector2 MouseDeltaSensitivity => mouse_delta_sensitivity;
    
    private int mouse_wheel_delta = 0;
    private int mouse_wheel_horizontal_delta = 0;
   
    public int MouseWheelDelta => mouse_wheel_delta;
    public int MouseWheelHorizontalDelta => mouse_wheel_horizontal_delta;

    public static Vector2 MouseSensitivity => gvars.get_vector2("mouse_sensitivity");
    
    private MouseState mouse_state_current;
    private MouseState mouse_state_previous;

    public static Vector2i Position => global_pos;
    private static Vector2i global_pos = Vector2i.Zero; 
    
    public static bool show_mouse_cursor { get; set; } = false;
        
    bool mouse_locked = false;
    bool mouse_locked_p = false;

    private static bool mouse_locked_global_status = false;
    public static bool MouseLocked => mouse_locked_global_status;
    
    private static bool mouse_locked_global_status_previous = false;
    public static bool MouseLockedPrevious => mouse_locked_global_status_previous;
    
    private static Vector2i mouse_lock_stored_position;
    
    public static bool mouse_in_bounds => Math2D.point_within_square(Vector2i.Zero, State.window.ClientBounds.Size.ToVector2i(), Position);

    Vector2 mouse_delta_aspect_ratio_and_sensitivity() {
        
        Vector2 res = State.resolution.ToVector2();
        var mouse_delta_final = mouse_delta.ToVector2();
        mouse_delta_final /= res;
            
        if (res.X < res.Y) {
            float ar = res.Y / res.X;
            mouse_delta_final.Y *= ar;
        } else {
            float ar = res.X / res.Y;
            mouse_delta_final.X *= ar;
        }

        return mouse_delta_final * MouseSensitivity;
    }

    public bool leave_mouse_centered_after_mouse_lock_released { get; set; } = false;
    
    public void UpdateDeltas() {
        if (!mouse_locked && mouse_locked_p) {
            Interlocked.Exchange(ref mouse_locked_global_status, false);
        } else if (!mouse_locked && !mouse_locked_p && !mouse_locked_global_status ) {
            Interlocked.Exchange(ref mouse_locked_global_status_previous, false);
        }
        
        mouse_state_previous = mouse_state_current;
        mouse_state_current = Mouse.GetState();
        
        mouse_delta = mouse_state_current.Position.ToVector2i() - mouse_state_previous.Position.ToVector2i();
        mouse_wheel_delta = mouse_state_current.ScrollWheelValue - mouse_state_previous.ScrollWheelValue;
        
        
        mouse_wheel_horizontal_delta = mouse_state_current.HorizontalScrollWheelValue -
                                       mouse_state_previous.HorizontalScrollWheelValue;
        
        global_pos = mouse_state_current.Position.ToVector2i();
        _mouse_coll_obj.SetPosition(mouse_state_current.Position.ToVector2());
        
        State.game.IsMouseVisible = show_mouse_cursor;
        
        var window_center = new Vector2i(
            (State.window.ClientBounds.Width / 2),
            (State.window.ClientBounds.Height / 2)
        );
        
        if (mouse_locked && !mouse_locked_p) {
            mouse_lock_stored_position = Position;
            
            Mouse.SetPosition(window_center.X, window_center.Y);        
            
            mouse_delta = Vector2i.Zero;
            
        } else if (mouse_locked) {    
            mouse_delta = window_center
                          - (Vector2i.UnitX * Position.X)
                          - (Vector2i.UnitY * Position.Y);

            Mouse.SetPosition(window_center.X, window_center.Y);
        } else if (!mouse_locked) {
            if (mouse_locked_p) {
                if (leave_mouse_centered_after_mouse_lock_released) 
                    Mouse.SetPosition(mouse_lock_stored_position.X, mouse_lock_stored_position.Y);
                else
                    Mouse.SetPosition(window_center.X, window_center.Y);
                
                mouse_delta = Vector2i.Zero;
            } 
        }

        mouse_locked_p = mouse_locked;
        mouse_locked = false;
    }
    
    
    public void LockMouse() {
        if (Interlocked.CompareExchange(ref mouse_locked_global_status, true, false)) {
            mouse_locked = true;
        } else {
            Interlocked.Exchange(ref mouse_locked_global_status_previous, true);
        }
    }

    
    public bool is_pressed(MouseButtons mb) {
        if (!State.is_active || !mouse_in_bounds) return false;
        switch (mb) {
            case MouseButtons.Left:
                return mouse_state_current.LeftButton == ButtonState.Pressed;

            case MouseButtons.Right:
                return mouse_state_current.RightButton == ButtonState.Pressed;

            case MouseButtons.Middle:
                return mouse_state_current.MiddleButton == ButtonState.Pressed;

            case MouseButtons.Mouse4:
                return mouse_state_current.XButton1 == ButtonState.Pressed;

            case MouseButtons.Mouse5:
                return mouse_state_current.XButton2 == ButtonState.Pressed;

            case MouseButtons.ScrollUp:
                return mouse_wheel_delta > 0;

            case MouseButtons.ScrollDown:
                return mouse_wheel_delta < 0;

            default: return false;
        }
    }
    
    public bool was_pressed(MouseButtons mb) {
        switch (mb) {
            case MouseButtons.Left:
                return mouse_state_previous.LeftButton == ButtonState.Pressed;

            case MouseButtons.Right:
                return mouse_state_previous.RightButton == ButtonState.Pressed;

            case MouseButtons.Middle:
                return mouse_state_previous.MiddleButton == ButtonState.Pressed;

            case MouseButtons.Mouse4:
                return mouse_state_previous.XButton1 == ButtonState.Pressed;

            case MouseButtons.Mouse5:
                return mouse_state_previous.XButton2 == ButtonState.Pressed;

            case MouseButtons.ScrollUp:
                return mouse_wheel_delta > 0;

            case MouseButtons.ScrollDown:
                return mouse_wheel_delta < 0;

            default: return false;
        }
    }
    public bool just_pressed(MouseButtons mb) {
        return is_pressed(mb) && !was_pressed(mb);
    }
    public bool just_released(MouseButtons mb) {
        return !is_pressed(mb) && was_pressed(mb);
    }
    
    public string state_info() {
        string s = $"[MOUSE]\n";
        s += $"position :: {MouseWatcher.Position.ToXString()}\n";
        s += $"locked :: {MouseWatcher.mouse_locked_global_status}\n";
        s += $"locked prev :: {MouseWatcher.mouse_locked_global_status_previous}\n";
        
        s += $"in bounds :: {mouse_in_bounds} \n";
        s += $"buttons :: ";
        var c = false;
        foreach (var mb in Enum.GetValues(typeof(MouseButtons))) {
            if (is_pressed((MouseButtons)mb)) {
                if (c) s += ", "; else c = true;
                s += Enum.GetName(typeof(MouseButtons), mb);
            }
        }

        s += $"\ndelta :: {mouse_delta.ToXString()}";
        return s + "\n\n";
    }
}