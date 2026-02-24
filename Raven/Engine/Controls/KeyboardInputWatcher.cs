using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using CSScripting;
using Microsoft.Xna.Framework.Input;

namespace Raven.Engine.Controls;

[InputWatcher]
public partial class KeyboardWatcher {
    public static partial class Manager {
        private static KeyboardState keyboard_state;
        public static KeyboardState KeyboardState => keyboard_state;
        
        private static KeyboardState keyboard_state_prev;
        public static KeyboardState KeyboardStatePrevious => keyboard_state_prev;
        
        static Keys[] pressed_keys;
        static Keys[] pressed_keys_previous;
        
        public static void StartPolling() {
            TickRateWatcher.poll_rate = gvars.get_int("input_polling_rate");
            StartControlPollingThread("KeyboardWatcher", UpdateKeys);
        }

        public static void UpdateKeys() {
            keyboard_state_prev = keyboard_state;
            keyboard_state = Keyboard.GetState();
            
            pressed_keys_previous = pressed_keys;
            pressed_keys = keyboard_state.GetPressedKeys();
        }
    }
    
    private static KeyboardState keyboard_state;
    private static KeyboardState keyboard_state_prev;
    
    public bool is_pressed(Keys k) { return keyboard_state.IsKeyDown(k); }
    public bool was_pressed(Keys k) { return keyboard_state.IsKeyDown(k); }
    public bool just_pressed(Keys k) { return is_pressed(k) && !was_pressed(k); }
    public bool just_released(Keys k) { return !is_pressed(k) && was_pressed(k); }

    public void Update() {
        keyboard_state_prev = keyboard_state;
        keyboard_state = Manager.KeyboardState;
    }
    
}