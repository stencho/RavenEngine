using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Raven.Engine.Collision;
using Raven.Engine.Collision.Shapes2D;
using Raven.Engine.Collision.Shapes3D;
using Raven.Graphics;

namespace Raven.Engine.Controls;

public class Input {
    // STATE
    KeyboardState ks; public KeyboardState keyboard_state => ks;
    KeyboardState ksp; public KeyboardState keyboard_state_prev => ksp;
    
    GamePadState[] xs = new GamePadState[4];
    public GamePadState[] xinput_state => xs;
    GamePadState[] xsp = new GamePadState[4];
    public GamePadState[] xinput_state_prev => xsp;
    
    // KEYBOARD
    Keys[] pressed_keys;
    Keys[] pressed_keys_previous;
    
    // MOUSE
    public Vector2i mouse_position => MouseWatcher.Manager.Position;
    public Vector2 mouse_position_float => new Vector2(MouseWatcher.Manager.Position.X, MouseWatcher.Manager.Position.Y);

    public bool mouse_in_bounds => is_mouse_in_bounds();

    Vector2i window_center = Vector2i.Zero;
    
    private MouseWatcher mouse = new MouseWatcher();
    
    bool is_mouse_in_bounds() {
        return (mouse_position.X > 0
                && mouse_position.Y > 0
                && mouse_position.X < State.resolution.X
                && mouse_position.Y < State.resolution.Y);
    }
   
    internal void Update() {
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
        

        window_center.X = (State.window.ClientBounds.Width / 2);
        window_center.Y = (State.window.ClientBounds.Height / 2);
        
        mouse.ResetMouseDelta();
    }
    
    #region is/was pressed
    public bool was_pressed(Keys k) { return ksp.IsKeyDown(k); }
    public bool is_pressed(Keys k) { return ks.IsKeyDown(k); }
    public bool just_pressed(Keys k) { return is_pressed(k) && !was_pressed(k); }
    public bool just_released(Keys k) { return !is_pressed(k) && was_pressed(k); }

    public bool is_pressed(MouseWatcher.MouseButtons button) => mouse.is_pressed(button);
    public bool was_pressed(MouseWatcher.MouseButtons button) => mouse.was_pressed(button);
    public bool just_pressed(MouseWatcher.MouseButtons button) => mouse.is_pressed(button) && !mouse.was_pressed(button);
    public bool just_released(MouseWatcher.MouseButtons button) => !mouse.was_pressed(button) && mouse.was_pressed(button);
    
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
    public float get_axis(XInputAxis axis) { return 0f; }
    #endregion
    
}
