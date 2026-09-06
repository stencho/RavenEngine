using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Raven.Engine.Controls;


public partial class XInputWatcher {
    private GamePadState gamepad_state;
    public GamePadState GamePadState => gamepad_state;
        
    private GamePadState gamepad_state_previous;
    public GamePadState GamePadStatePrevious => gamepad_state_previous;

    private PlayerIndex player_index; 
    public PlayerIndex PlayerIndex => player_index;

    public XInputDigital[] pressed_buttons = [];
    public XInputDigital[] pressed_buttons_previous = [];

    private List<XInputDigital> buttons_down_this_frame = new List<XInputDigital>();
    
    public float stick_analog_to_digital_threshold = 0.25f;
    public float trigger_analog_to_digital_threshold = 0.5f;
    
    public float stick_deadzone = 0.1f;
    
    public bool is_pressed(XInputDigital x) { return pressed_buttons.Contains(x); }
    public bool was_pressed(XInputDigital x) { return pressed_buttons_previous.Contains(x); }
    public bool just_pressed(XInputDigital x) { return is_pressed(x) && !was_pressed(x); }
    public bool just_released(XInputDigital x) { return !is_pressed(x) && was_pressed(x); }

    static readonly Lock state_lock = new Lock();
    
    public void Update() {
        gamepad_state_previous = gamepad_state;

        fucked_up_array_access_during_state_update_which_wont_go_away:
        try {
            using (state_lock.EnterScope()) {
                gamepad_state = GamePad.GetState(player_index);
            }
        } catch (InvalidOperationException) {
            goto fucked_up_array_access_during_state_update_which_wont_go_away;
        }

        buttons_down_this_frame.Clear();
        
        if (gamepad_state.Buttons.A == ButtonState.Pressed) buttons_down_this_frame.Add(XInputDigital.A);
        if (gamepad_state.Buttons.B == ButtonState.Pressed) buttons_down_this_frame.Add(XInputDigital.B);
        if (gamepad_state.Buttons.X == ButtonState.Pressed) buttons_down_this_frame.Add(XInputDigital.X);
        if (gamepad_state.Buttons.Y == ButtonState.Pressed) buttons_down_this_frame.Add(XInputDigital.Y);
        
        if (gamepad_state.Buttons.LeftStick == ButtonState.Pressed) buttons_down_this_frame.Add(XInputDigital.LeftStick);
        if (gamepad_state.Buttons.RightStick == ButtonState.Pressed) buttons_down_this_frame.Add(XInputDigital.RightStick);
        
        if (gamepad_state.Buttons.LeftShoulder == ButtonState.Pressed) buttons_down_this_frame.Add(XInputDigital.LeftShoulder);
        if (gamepad_state.Buttons.RightShoulder == ButtonState.Pressed) buttons_down_this_frame.Add(XInputDigital.RightShoulder);
        
        if (gamepad_state.DPad.Up == ButtonState.Pressed) buttons_down_this_frame.Add(XInputDigital.DPadUp);
        if (gamepad_state.DPad.Down == ButtonState.Pressed) buttons_down_this_frame.Add(XInputDigital.DPadDown);
        if (gamepad_state.DPad.Left == ButtonState.Pressed) buttons_down_this_frame.Add(XInputDigital.DPadLeft);
        if (gamepad_state.DPad.Right == ButtonState.Pressed) buttons_down_this_frame.Add(XInputDigital.DPadRight);

        if (gamepad_state.Buttons.Start == ButtonState.Pressed) buttons_down_this_frame.Add(XInputDigital.Start);
        if (gamepad_state.Buttons.Back == ButtonState.Pressed) buttons_down_this_frame.Add(XInputDigital.Back);
        
        if (gamepad_state.Buttons.BigButton == ButtonState.Pressed) buttons_down_this_frame.Add(XInputDigital.Guide);
        
        if (gamepad_state.Triggers.Left >= trigger_analog_to_digital_threshold) buttons_down_this_frame.Add(XInputDigital.LeftTrigger);
        if (gamepad_state.Triggers.Right >= trigger_analog_to_digital_threshold) buttons_down_this_frame.Add(XInputDigital.RightTrigger);

        if (gamepad_state.ThumbSticks.Left.X >= stick_analog_to_digital_threshold) buttons_down_this_frame.Add(XInputDigital.LeftStickRight);
        if (gamepad_state.ThumbSticks.Left.X <= -stick_analog_to_digital_threshold) buttons_down_this_frame.Add(XInputDigital.LeftStickLeft);
        if (gamepad_state.ThumbSticks.Right.X >= stick_analog_to_digital_threshold) buttons_down_this_frame.Add(XInputDigital.RightStickRight);
        if (gamepad_state.ThumbSticks.Right.X <= -stick_analog_to_digital_threshold) buttons_down_this_frame.Add(XInputDigital.RightStickLeft);
        
        if (gamepad_state.ThumbSticks.Left.Y <= -stick_analog_to_digital_threshold) buttons_down_this_frame.Add(XInputDigital.LeftStickDown);
        if (gamepad_state.ThumbSticks.Left.Y >= stick_analog_to_digital_threshold) buttons_down_this_frame.Add(XInputDigital.LeftStickUp);
        if (gamepad_state.ThumbSticks.Right.Y <= -stick_analog_to_digital_threshold) buttons_down_this_frame.Add(XInputDigital.RightStickDown);
        if (gamepad_state.ThumbSticks.Right.Y >= stick_analog_to_digital_threshold) buttons_down_this_frame.Add(XInputDigital.RightStickUp);

        pressed_buttons_previous = pressed_buttons;
        pressed_buttons = buttons_down_this_frame.ToArray();
    }
}
    
#region enums

public enum XInputAnalog { LeftStickX, LeftStickY, RightStickX, RightStickY, TriggerL, TriggerR }

public enum XInputStick { Left, Right }

public enum XInputDigital {
    A, B, X, Y,
    LeftShoulder, RightShoulder,
    LeftTrigger, RightTrigger,
    LeftStick, RightStick,
    DPadUp, DPadDown, DPadLeft, DPadRight,
    Start, Back,
    Guide,
        
    LeftStickUp, LeftStickDown, LeftStickLeft, LeftStickRight, 
    RightStickUp, RightStickDown, RightStickLeft, RightStickRight, 
}
    
#endregion