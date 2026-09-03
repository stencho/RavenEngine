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
    
    static readonly Lock state_lock = new Lock();
    
    public void Update() {
        fucked_up_array_access_during_state_update_which_wont_go_away:
        try {
            using (state_lock.EnterScope()) {
                keyboard_state = Keyboard.GetState();
            }
        } catch (InvalidOperationException) {
            goto fucked_up_array_access_during_state_update_which_wont_go_away;
        }

        

        pressed_keys_previous = pressed_keys;
        pressed_keys = keyboard_state.GetPressedKeys();
        
        keyboard_state_prev = keyboard_state;
    }
    

    public string state_info() {
        string s = $"[KEYBOARD] ";
        var c = false;

        s += "keys :: ";
        foreach (Keys key in pressed_keys) {
            if (c) s += ", "; else c = true;
            s += $"{Enum.GetName(typeof(Keys), key)}";
        }
        
        return s + "\n\n";
    }
}