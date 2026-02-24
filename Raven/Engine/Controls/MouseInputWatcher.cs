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

[InputWatcher]
public partial class MouseWatcher {
    public static partial class Manager {
        private static MouseState mouse_state;
        public static MouseState MouseState => mouse_state;
        
        private static MouseState mouse_state_prev;
        public static MouseState MouseStatePrevious => mouse_state_prev;
        
        private static Vector2i mouse_delta = Vector2i.Zero;
        public static Vector2i MouseDelta => mouse_delta;
        public static Vector2 MouseDeltaF => mouse_delta.ToVector2();
        
        public static Vector2i Position => new Vector2i(mouse_state.X, mouse_state.Y);
        
        public static bool show_mouse_cursor { get; set; } = false;
        
        static bool mouse_locked = false;
        static bool mouse_locked_p = false;
        public static bool MouseLock{ get; set; } = false;
    
        private static Point mouse_lock_stored_position;

        public static Collision2D.Shape2D MouseCollisionObject => _mouse_coll_obj;
        static Collision2D.Shape2D _mouse_coll_obj;

        public static void StartPolling() {
            TickRateWatcher.poll_rate = gvars.get_int("input_polling_rate");
            StartControlPollingThread("MouseWatcher", UpdateDeltas);
            _mouse_coll_obj = new Circle2D(Position.ToVector2(), 1f);
        }
        
        public static void UpdateDeltas() {
            mouse_state_prev = mouse_state;
            mouse_state = Mouse.GetState();
            
            State.game.IsMouseVisible = show_mouse_cursor;
            
            mouse_locked_p = mouse_locked;
            mouse_locked = MouseLock;
            
            mouse_delta = Vector2i.Zero;

            var window_center = new Vector2i(
                (State.window.ClientBounds.Width / 2),
                (State.window.ClientBounds.Height / 2)
            );

            if (mouse_locked && !mouse_locked_p) {
                mouse_lock_stored_position = Manager.MouseState.Position;
            
                Mouse.SetPosition(window_center.X, window_center.Y);        
            
                mouse_delta = Vector2i.Zero;
            
            } else if (mouse_locked) {    
                mouse_delta = window_center
                              - (Vector2i.UnitX * Manager.MouseState.X)
                              - (Vector2i.UnitY * Manager.MouseState.Y);

                Mouse.SetPosition(window_center.X, window_center.Y);
            } else if (!mouse_locked) {
                if (mouse_locked_p) {
                    Mouse.SetPosition(mouse_lock_stored_position.X, mouse_lock_stored_position.Y);
                
                    mouse_delta = Vector2i.Zero;
                } else {
                    mouse_delta = ((Vector2i.UnitX * -(Manager.MouseStatePrevious.X - Manager.MouseState.X)) +
                                   (Vector2i.UnitY * -(Manager.MouseStatePrevious.Y - Manager.MouseState.Y)));
                }
            }

            mousewatchers.ForEach(mf => 
                mf.UpdateDeltas(mouse_delta));
            
            _mouse_coll_obj.SetPosition(Position.ToVector2());
        }
    }
    
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
    
    public int wheel_value = 0;
    public int wheel_delta = 0;
    private int old_delta = 0;
    
    public MouseWatcher() => Manager.Add(this);
    ~MouseWatcher() => Manager.Remove(this);
    
    private MouseState mouse_state_current;
    private MouseState mouse_state_previous;

    internal void UpdateDeltas(Vector2i delta) {
        mouse_state_current = Manager.MouseState;

        mouse_delta += delta;

        old_delta = wheel_delta;
        wheel_delta = mouse_state_current.ScrollWheelValue - wheel_value;
        wheel_value = mouse_state_current.ScrollWheelValue;
    }

    public void ResetMouseDelta() {
        wheel_delta = 0;
        wheel_value = 0;
        mouse_delta = Vector2i.Zero;
        mouse_state_previous = mouse_state_current;
    }
    
    public bool is_pressed(MouseButtons mb) {
        if (!State.is_active) return false;
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
                return wheel_delta > 0;

            case MouseButtons.ScrollDown:
                return wheel_delta < 0;

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
                return old_delta > 0;

            case MouseButtons.ScrollDown:
                return old_delta < 0;

            default: return false;
        }
    }
    public bool just_pressed(MouseButtons mb) {
        return is_pressed(mb) && !was_pressed(mb);
    }
    public bool just_released(MouseButtons mb) {
        return !is_pressed(mb) && was_pressed(mb);
    }
}