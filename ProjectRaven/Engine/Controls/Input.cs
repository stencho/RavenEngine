using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using ProjectRaven.Engine.Collision;
using ProjectRaven.Engine.Collision.Shapes2D;
using ProjectRaven.Engine.Collision.Shapes3D;

namespace ProjectRaven.Engine.Controls;

public class Input {
    // STATE
    KeyboardState ks; public KeyboardState keyboard_state => ks;
    KeyboardState ksp; public KeyboardState keyboard_state_prev => ksp;
    
    internal MouseState mouse_state;
    internal MouseState mouse_state_prev;
    
    GamePadState[] xs = new GamePadState[4];
    public GamePadState[] xinput_state => xs;
    GamePadState[] xsp = new GamePadState[4];
    public GamePadState[] xinput_state_prev => xsp;
    
    // KEYBOARD
    Keys[] pressed_keys;
    Keys[] pressed_keys_previous;
    
    // MOUSE
    public Vector2i mouse_position => new Vector2i(mouse_state.Position.X, mouse_state.Position.Y);
    public Vector2 mouse_position_float => new Vector2(mouse_state.Position.X, mouse_state.Position.Y);

    public bool mouse_in_bounds => is_mouse_in_bounds();

    Vector2i window_center = Vector2i.Zero;

    volatile static bool mouse_locked = false;
    volatile static bool mouse_locked_p = false;
    public static bool mouse_lock { get; set; } = false;
    public static bool mouse_cursor { get; set; } = false;
    
    public Collision2D.Shape2D mouse_collision_object => _mouse_coll_obj;
    Collision2D.Shape2D _mouse_coll_obj;

    public int old_wheel_value = 0;
    public int wheel_delta = 0;
    int old_delta = 0;
    
    public Vector2 mouse_delta => _mouse_delta;
    Vector2 _mouse_delta;
    
    
    bool is_mouse_in_bounds() {
        return (mouse_position.X > 0
                && mouse_position.Y > 0
                && mouse_position.X < State.resolution.X
                && mouse_position.Y < State.resolution.Y);
    }
   
    internal void Update() {
        mouse_state = Mouse.GetState();
        ksp = ks;
        ks = Keyboard.GetState();
        
        pressed_keys_previous = pressed_keys;
        pressed_keys = ks.GetPressedKeys();
        
        xsp[0] = xs[0];
        xsp[1] = xs[1];
        xsp[2] = xs[2];
        xsp[3] = xs[3];

        xs[0] = GamePad.GetState(PlayerIndex.One);
        xs[1] = GamePad.GetState(PlayerIndex.Two);
        xs[2] = GamePad.GetState(PlayerIndex.Three);
        xs[3] = GamePad.GetState(PlayerIndex.Four);
        
        State.game.IsMouseVisible = mouse_cursor;

        window_center.X = (State.window.ClientBounds.Width / 2);
        window_center.Y = (State.window.ClientBounds.Height / 2);
        
        _mouse_delta = Vector2.Zero;
                
        mouse_locked_p = mouse_locked;
        mouse_locked = mouse_lock;
        
        _mouse_coll_obj = new Circle2D(mouse_position_float, 1f);
        
        if (mouse_locked && !mouse_locked_p) {                    
            Mouse.SetPosition(window_center.X, window_center.Y);

        } else if (mouse_locked) {    
            _mouse_delta = (window_center.ToVector2())
                              - (Vector2.UnitX * mouse_state.X)
                              - (Vector2.UnitY * mouse_state.Y);

            Mouse.SetPosition(window_center.X, window_center.Y);
        } else if (!mouse_locked) {
            _mouse_delta = ((Vector2.UnitX * -(mouse_state_prev.X - mouse_state.X)) + (Vector2.UnitY * -(mouse_state_prev.Y - mouse_state.Y)));
        }

        old_delta = wheel_delta;
        wheel_delta = mouse_state.ScrollWheelValue - old_wheel_value;
        old_wheel_value = mouse_state.ScrollWheelValue;
        
        mouse_locked_p = mouse_locked;
        mouse_state_prev = mouse_state;

    }
    
    
    #region is/was pressed
    public bool was_pressed(Keys k) { return ksp.IsKeyDown(k); }
    public bool is_pressed(Keys k) { return ks.IsKeyDown(k); }
    public bool just_pressed(Keys k) { return is_pressed(k) && !was_pressed(k); }
    public bool just_released(Keys k) { return !is_pressed(k) && was_pressed(k); }

    public bool is_pressed(MouseButtons mb) {
        if (!State.is_active) return false;
        switch (mb) {
            case MouseButtons.Left:
                return mouse_state.LeftButton == ButtonState.Pressed;

            case MouseButtons.Right:
                return mouse_state.RightButton == ButtonState.Pressed;

            case MouseButtons.Middle:
                return mouse_state.MiddleButton == ButtonState.Pressed;

            case MouseButtons.Mouse4:
                return mouse_state.XButton1 == ButtonState.Pressed;

            case MouseButtons.Mouse5:
                return mouse_state.XButton2 == ButtonState.Pressed;

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
                return mouse_state_prev.LeftButton == ButtonState.Pressed;

            case MouseButtons.Right:
                return mouse_state_prev.RightButton == ButtonState.Pressed;

            case MouseButtons.Middle:
                return mouse_state_prev.MiddleButton == ButtonState.Pressed;

            case MouseButtons.Mouse4:
                return mouse_state_prev.XButton1 == ButtonState.Pressed;

            case MouseButtons.Mouse5:
                return mouse_state_prev.XButton2 == ButtonState.Pressed;

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
    
    public bool is_pressed(XInputButtons test_button, PlayerIndex player) {
        switch (test_button) {
            case XInputButtons.A:
                return xs[(int)player].Buttons.A == ButtonState.Pressed;
            case XInputButtons.B:
                return xs[(int)player].Buttons.B == ButtonState.Pressed;
            case XInputButtons.X:
                return xs[(int)player].Buttons.X == ButtonState.Pressed;
            case XInputButtons.Y:
                return xs[(int)player].Buttons.Y == ButtonState.Pressed;

            case XInputButtons.LB:
                return xs[(int)player].Buttons.LeftShoulder == ButtonState.Pressed;
            case XInputButtons.RB:
                return xs[(int)player].Buttons.RightShoulder == ButtonState.Pressed;

            case XInputButtons.Start:
                return xs[(int)player].Buttons.Start == ButtonState.Pressed;
            case XInputButtons.Back:
                return xs[(int)player].Buttons.Back == ButtonState.Pressed;

            case XInputButtons.LStick:
                return xs[(int)player].Buttons.LeftStick == ButtonState.Pressed;
            case XInputButtons.RStick:
                return xs[(int)player].Buttons.RightStick == ButtonState.Pressed;

            case XInputButtons.DPadUp:
                return xs[(int)player].DPad.Up == ButtonState.Pressed;
            case XInputButtons.DPadDown:
                return xs[(int)player].DPad.Down == ButtonState.Pressed;
            case XInputButtons.DPadLeft:
                return xs[(int)player].DPad.Left == ButtonState.Pressed;
            case XInputButtons.DPadRight:
                return xs[(int)player].DPad.Right == ButtonState.Pressed;
        }
        return false;
    }

    public bool was_pressed(XInputButtons test_button, PlayerIndex player) {
        switch (test_button) {
            case XInputButtons.A:
                return xsp[(int)player].Buttons.A == ButtonState.Pressed;
            case XInputButtons.B:
                return xsp[(int)player].Buttons.B == ButtonState.Pressed;
            case XInputButtons.X:
                return xsp[(int)player].Buttons.X == ButtonState.Pressed;
            case XInputButtons.Y:
                return xsp[(int)player].Buttons.Y == ButtonState.Pressed;

            case XInputButtons.LB:
                return xsp[(int)player].Buttons.LeftShoulder == ButtonState.Pressed;
            case XInputButtons.RB:
                return xsp[(int)player].Buttons.RightShoulder == ButtonState.Pressed;

            case XInputButtons.Start:
                return xsp[(int)player].Buttons.Start == ButtonState.Pressed;
            case XInputButtons.Back:
                return xsp[(int)player].Buttons.Back == ButtonState.Pressed;

            case XInputButtons.LStick:
                return xsp[(int)player].Buttons.LeftStick == ButtonState.Pressed;
            case XInputButtons.RStick:
                return xsp[(int)player].Buttons.RightStick == ButtonState.Pressed;

            case XInputButtons.DPadUp:
                return xsp[(int)player].DPad.Up == ButtonState.Pressed;
            case XInputButtons.DPadDown:
                return xsp[(int)player].DPad.Down == ButtonState.Pressed;
            case XInputButtons.DPadLeft:
                return xsp[(int)player].DPad.Left == ButtonState.Pressed;
            case XInputButtons.DPadRight:
                return xsp[(int)player].DPad.Right == ButtonState.Pressed;
        }
        return false;
    }

    public bool just_pressed(XInputButtons test_button, PlayerIndex player) {
        return is_pressed(test_button, player) && !was_pressed(test_button, player);
    }
    public bool just_released(XInputButtons test_button, PlayerIndex player) {
        return is_pressed(test_button, player) && !was_pressed(test_button, player);
    }

    #endregion
    
    #region enums
    public enum MouseButtons {
        Left,
        Right,
        Middle,
        Mouse4,
        Mouse5,
        ScrollUp,
        ScrollDown
    }

    public enum MouseAxis {
        X, Y
    }

    public enum XInputAxis {
        LSX, LSY, RSX, RSY,
        TriggerL, TriggerR
    }

    public enum XInputStick {
        Left,
        Right
    }

    public enum XInputButtons {
        A, B, X, Y,
        LB, RB,
        DPadUp, DPadDown, DPadLeft, DPadRight,
        Start, Back,
        LStick, RStick
    }

    #endregion
    
    #region analog controls
    public float get_axis(MouseAxis axis) { return 0f; }
    public float get_axis(XInputAxis axis) { return 0f; }
    #endregion
    
}

public class picker_raycasts {
    public Raycasting.raycast crosshair_ray;
    public Raycasting.raycast mouse_pick_ray;

    public Line3D gjk_crosshair_ray = new Line3D();
    public Line3D gjk_mouse_pick_ray = new Line3D();

    public void update() {
        crosshair_ray = new Raycasting.raycast(State.camera.position, State.camera.direction);

        gjk_crosshair_ray.A = State.camera.position;
        gjk_crosshair_ray.B = State.camera.direction;

        //mouse picker stuff
        Vector3 n = new Vector3(State.input_main_thread.mouse_position.X, State.input_main_thread.mouse_position.Y, 0);
        Vector3 f = new Vector3(State.input_main_thread.mouse_position.X, State.input_main_thread.mouse_position.Y, 1);

        Vector3 near = State.viewport.Unproject(n, State.camera.projection, State.camera.view, Matrix.Identity);
        Vector3 far = State.viewport.Unproject(f, State.camera.projection, State.camera.view, Matrix.Identity);

        Vector3 d = far - near;
        d.Normalize();

        mouse_pick_ray = new Raycasting.raycast(near, d);

        gjk_mouse_pick_ray.A = near;
        gjk_mouse_pick_ray.B = d;
    }
}