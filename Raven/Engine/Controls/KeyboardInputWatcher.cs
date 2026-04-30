using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using CSScripting;
using Microsoft.Xna.Framework.Input;

namespace Raven.Engine.Controls;

public partial class KeyboardWatcher {
    
    private KeyboardState keyboard_state;
    public KeyboardState KeyboardState => keyboard_state;
        
    private KeyboardState keyboard_state_prev;
    public KeyboardState KeyboardStatePrevious => keyboard_state_prev;
        
    public Keys[] pressed_keys = [];
    public Keys[] pressed_keys_previous = [];
    
    public bool is_pressed(Keys k) { return pressed_keys.Contains(k); }
    public bool was_pressed(Keys k) { return pressed_keys_previous.Contains(k); }
    public bool just_pressed(Keys k) { return is_pressed(k) && !was_pressed(k); }
    public bool just_released(Keys k) { return !is_pressed(k) && was_pressed(k); }

    public KeyboardWatcher() { }

    private static volatile bool GETTING_STATE = false; 
    
    public void Update() {
        keyboard_state_prev = keyboard_state;
        while (true) {
            if (Interlocked.CompareExchange(ref GETTING_STATE, true, false)) {
                keyboard_state = Keyboard.GetState();
                break;
            }
        }

        Interlocked.Exchange(ref GETTING_STATE, false);

        pressed_keys_previous = pressed_keys;
        pressed_keys = keyboard_state.GetPressedKeys();
    }
    
    public void UpdateEnd() {
    }

    public string state_info() {
        string s = $"[KEYBOARD]\n";
        var c = false;

        s += "keys :: ";
        foreach (Keys key in pressed_keys) {
            if (c) s += ", "; else c = true;
            s += $"{Enum.GetName(typeof(Keys), key)}";
        }
        
        return s + "\n\n";
    }
}