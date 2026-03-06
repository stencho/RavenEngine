using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

        public enum KeyPressedOrReleased { PRESSED, RELEASED }
        static HashSet<(KeyPressedOrReleased, Keys)> pressed_key_diff = new();
        
        public static void StartPolling() {
            TickRateWatcher.poll_rate = gvars.get_int("input_polling_rate");
            StartControlPollingThread("KeyboardWatcher", UpdateKeys);
        }

        public static void UpdateKeys() {
            keyboard_state_prev = keyboard_state;
            keyboard_state = Keyboard.GetState();

            pressed_keys_previous = pressed_keys;
            pressed_keys = keyboard_state.GetPressedKeys();

            pressed_key_diff.Clear();

            foreach (var k in pressed_keys) {
                if (!pressed_keys_previous.Contains(k)) {
                    pressed_key_diff.Add((KeyPressedOrReleased.PRESSED, k));
                }
            }

            if (pressed_keys_previous != null) {
                foreach (var k in pressed_keys_previous) {
                    if (!pressed_keys.Contains(k)) {
                        pressed_key_diff.Add((KeyPressedOrReleased.RELEASED, k));
                    }
                }
            }

            foreach (KeyboardWatcher kw in keyboardwatchers) {
                foreach (var k in pressed_key_diff) {
                    if (!kw.pressed_key_diff.Contains(k)) {
                        kw.pressed_key_diff.Add(k);
                    }
                }
            }
        }
    }
    
    public HashSet<(Manager.KeyPressedOrReleased p_r, Keys key)> pressed_key_diff = new();

    public List<Keys> pressed_keys = new();
    public List<Keys> pressed_keys_previous = new();
    
    public bool is_pressed(Keys k) { return pressed_keys.Contains(k); }
    public bool was_pressed(Keys k) { return pressed_keys_previous.Contains(k); }
    public bool just_pressed(Keys k) { return is_pressed(k) && !was_pressed(k); }
    public bool just_released(Keys k) { return !is_pressed(k) && was_pressed(k); }

    public KeyboardWatcher() {
        Manager.Add(this);
    }

    ~KeyboardWatcher() {
        Manager.Remove(this);
    }
    
    public void Update() {
        foreach (var pkd in pressed_key_diff) {
            if (pkd.p_r == Manager.KeyPressedOrReleased.PRESSED) {
                pressed_keys.Add(pkd.key);
            } else {
                pressed_keys.Remove(pkd.key);
            }
        }
        
        pressed_key_diff.Clear();
    }

    public void UpdateEnd() {
        pressed_keys_previous.Clear();
        foreach (var pk in pressed_keys) pressed_keys_previous.Add(pk);
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