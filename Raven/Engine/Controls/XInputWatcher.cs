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

    public Dictionary<XInputAnalog, float> analog_values = new Dictionary<XInputAnalog, float>();
    public Dictionary<XInputAnalog, float> old_analog_values = new Dictionary<XInputAnalog, float>();
    
    private List<XInputDigital> buttons_down_this_frame = new List<XInputDigital>();
    
    public float analog_to_digital_threshold = 0.25f;
    
    public float stick_deadzone = 0.1f;
    
    public bool is_pressed(XInputDigital x) { return pressed_buttons.Contains(x); }
    public bool was_pressed(XInputDigital x) { return pressed_buttons_previous.Contains(x); }
    public bool just_pressed(XInputDigital x) { return is_pressed(x) && !was_pressed(x); }
    public bool just_released(XInputDigital x) { return !is_pressed(x) && was_pressed(x); }

    public float value(XInputAnalog x) => analog_values[x];
    public float had_value(XInputAnalog x) => old_analog_values[x];
    
    static readonly Lock state_lock = new Lock();

    public XInputWatcher() {
        analog_values.Add(XInputAnalog.LeftStickLeft, 0f);
        analog_values.Add(XInputAnalog.LeftStickRight, 0f);
        
        analog_values.Add(XInputAnalog.LeftStickUp, 0f);
        analog_values.Add(XInputAnalog.LeftStickDown, 0f);
        
        analog_values.Add(XInputAnalog.RightStickLeft, 0f);
        analog_values.Add(XInputAnalog.RightStickRight, 0f);
        
        analog_values.Add(XInputAnalog.RightStickUp, 0f);
        analog_values.Add(XInputAnalog.RightStickDown, 0f);
        
        analog_values.Add(XInputAnalog.TriggerL, 0f);
        analog_values.Add(XInputAnalog.TriggerR, 0f);
        
        foreach (var a in analog_values) {
            old_analog_values.Add(a.Key, a.Value);
        }
    }
    
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
        
        pressed_buttons_previous = pressed_buttons;
        pressed_buttons = buttons_down_this_frame.ToArray();

        foreach (var a in analog_values) {
            old_analog_values[a.Key] = a.Value;
        }
        
        analog_values[XInputAnalog.LeftStickLeft] =   float.Clamp(-gamepad_state.ThumbSticks.Left.X, 0, 1);
        analog_values[XInputAnalog.LeftStickRight] =  float.Clamp(gamepad_state.ThumbSticks.Left.X, 0, 1);
        analog_values[XInputAnalog.LeftStickUp] =     float.Clamp(gamepad_state.ThumbSticks.Left.Y, 0, 1);
        analog_values[XInputAnalog.LeftStickDown] =   float.Clamp(-gamepad_state.ThumbSticks.Left.Y, 0, 1);
        
        analog_values[XInputAnalog.RightStickLeft] =  float.Clamp(-gamepad_state.ThumbSticks.Right.X, 0, 1);
        analog_values[XInputAnalog.RightStickRight] = float.Clamp(gamepad_state.ThumbSticks.Right.X, 0, 1);
        analog_values[XInputAnalog.RightStickUp] =    float.Clamp(gamepad_state.ThumbSticks.Right.Y, 0, 1);
        analog_values[XInputAnalog.RightStickDown] =  float.Clamp(-gamepad_state.ThumbSticks.Right.Y, 0, 1);
        
        analog_values[XInputAnalog.TriggerL] = gamepad_state.Triggers.Left;
        analog_values[XInputAnalog.TriggerR] = gamepad_state.Triggers.Right;
    }
}
    
#region enums

public enum XInputAnalog {
    LeftStickLeft, LeftStickRight,  LeftStickUp,  LeftStickDown, 
    RightStickLeft, RightStickRight, RightStickUp, RightStickDown, 
    TriggerL, TriggerR
}

public enum XInputDigital {
    A, B, X, Y,
    LeftShoulder, RightShoulder,
    LeftStick, RightStick,
    DPadUp, DPadDown, DPadLeft, DPadRight,
    Start, Back,
    Guide
}
    
#endregion