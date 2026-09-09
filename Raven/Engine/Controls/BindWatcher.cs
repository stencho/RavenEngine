global using BindList = (string bind, object[] bind_data)[];

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using CSScripting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Raven.Console;
using Raven.Engine;
using Raven.Engine.Controls;
using Raven.UI;

namespace Raven.Engine.Controls;

//WATCHER
public class BindWatcher {
    public KeyboardWatcher Keyboard = new KeyboardWatcher();
    public MouseWatcher Mouse = new MouseWatcher();
    public XInputWatcher XInput = new XInputWatcher();
    
    public Dictionary<string, InputBinds.Bind> binds = new();

    public static volatile bool global_enable = true;
    static volatile bool global_enable_prev = true;
    
    public enum UIFocusConsideration { DoesntCare, NeedsNoFocus, NeedsFocus}
    public UIFocusConsideration cares_about_UI_focus = UIFocusConsideration.DoesntCare;

    public IUIForm requires_focus_on_specific_form = null;
    
    bool allow_press = true;
    
    public void AddBind(InputBinds.Bind bind) {
        if (binds.ContainsKey(bind.Name)) {
            //bind by this name already exists, add inputs to it instead
            foreach (InputBinds.IInput i in bind.Inputs) {
                if (!binds[bind.Name].Inputs.Contains(i)) 
                    binds[bind.Name].Inputs.Add(i); 
            }
        } else {
            binds.Add(bind.Name, bind);
        }
    }

    public void AddBind(string name, Keys key) => AddBind(new InputBinds.Bind(name, new InputBinds.KeyInput(key)));
    public void AddBind(string name, MouseWatcher.MouseButtons mouse_button) => AddBind(new InputBinds.Bind(name, new InputBinds.MouseInput(mouse_button)));
    public void AddBind(string name, XInputDigital digital) => AddBind(new InputBinds.Bind(name, new InputBinds.XInputInput(digital)));
    public void AddBind(string name, XInputAnalog analog) => AddBind(new InputBinds.Bind(name, new InputBinds.XInputAnalogInput(analog)));
    
    public void AddMultipleBinds(params InputBinds.Bind[] binds) => binds.ForEach(AddBind);

    /// <summary>
    /// inputs should be an array of 'Keys', 'MouseButtons', 'XInputButtons', and 'XInputAxis' objects
    /// </summary>
    /// <param name="binds"></param>
    public void AddMultipleBinds(params (string name, object[] inputs)[] binds) {
        foreach (var b in binds) {
            foreach (var i in b.inputs) {
                switch (i) {
                    case Keys key:
                        AddBind(b.name, key);
                        break;
                    case MouseWatcher.MouseButtons mouse_button:
                        AddBind(b.name, mouse_button);
                        break;
                    case XInputDigital xinput_button:
                        AddBind(b.name, xinput_button);
                        break;
                    case XInputAnalog xinput_axis:
                        AddBind(b.name, xinput_axis);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }    
        }
    }
    
    public void RemoveBind(InputBinds.Bind bind) => binds.Remove(bind.Name);

    public BindWatcher(params (string bind_name, object[] bind_data)[] binds) {
        AddMultipleBinds(binds);
    }

    public string state_info() {
        string s = $"[BINDS]\n";
        var c = false;

        s += $"[FOCUS RULE] {cares_about_UI_focus} [ALLOW PRESS] {allow_press}\n";
        
        foreach (var b in binds.Values) {
            //if (!b.released()) {
                if (c) s += "\n"; else c = true;
                s += $"[{b.Name}] P:{b.pressed()} D:{b.digital_state} A:{value(b.Name):F2} -> ";
                var cc = false;
                foreach (var i in b.Inputs) {
                    if (cc) s += " | "; else cc = true;
                    switch (i.InputType) {
                        case InputBinds.InputType.Keyboard:
                            InputBinds.KeyInput k = i as InputBinds.KeyInput;
                            s += $"[{k.Key}] {Keyboard.is_pressed(k.Key)}";
                            break;
                        
                        case InputBinds.InputType.Mouse:
                            var m = i as InputBinds.MouseInput;
                            s += $"[{m.MouseButton}] {Mouse.is_pressed(m.MouseButton)}";
                            break;
                        
                        case InputBinds.InputType.XInput:
                            var xi = i as InputBinds.XInputInput;
                            s += $"[{xi.Digital}] {XInput.is_pressed(xi.Digital)}";
                            break;
                        
                        case InputBinds.InputType.XInputAnalog:
                            var xia = i as InputBinds.XInputAnalogInput;     
                            s += $"[{xia.Analog}] {XInput.value(xia.Analog):F2}";
                            break; 
                        
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                 
            //}
        }
        return s + "\n\n";
    }

    
    public void Update() {
        Mouse.UpdateDeltas();
        Keyboard.Update();
        XInput.Update();

        // Make sure that allow_press isn't re-enabled until ALL binds are released
        if (!allow_press) {
            bool all_released = true;
            foreach (var bind in binds.Values) {
                foreach (var d_bind in bind.Inputs) {
                    switch (d_bind.InputType) {
                        case InputBinds.InputType.Keyboard:
                            var k = d_bind as InputBinds.KeyInput;
                            if (Keyboard.is_pressed(k.Key)) all_released = false;
                            break;

                        case InputBinds.InputType.Mouse:
                            var m = d_bind as InputBinds.MouseInput;
                            if (Mouse.is_pressed(m.MouseButton)) all_released = false;
                            break;

                        case InputBinds.InputType.XInput:
                            var x = d_bind as InputBinds.XInputInput;
                            if (XInput.is_pressed(x.Digital)) all_released = false;
                            break;

                        case InputBinds.InputType.XInputAnalog:
                            var xa = d_bind as InputBinds.XInputAnalogInput;
                            if (XInput.value(xa.Analog) > XInput.analog_to_digital_threshold) all_released = false;
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
            if (all_released) allow_press = true;
        }
        
        // Disable activating binds if focus rules aren't met
        if (cares_about_UI_focus == UIFocusConsideration.NeedsNoFocus) {
            if (State.UI.focused_window_at_update_time != null) allow_press = false;
        }
        if (cares_about_UI_focus == UIFocusConsideration.NeedsFocus) {
            if (requires_focus_on_specific_form == null && State.UI.focused_window_at_update_time == null) allow_press = false;
            if (requires_focus_on_specific_form != null && State.UI.focused_window_at_update_time != requires_focus_on_specific_form) allow_press = false;
        }

        // Update binds
        foreach (var bind in binds.Values) {
            bind.update();
            
            foreach (var d_bind in bind.Inputs) {
                switch (d_bind.InputType) {
                    case InputBinds.InputType.Keyboard:
                        var k = d_bind as InputBinds.KeyInput;
                        if (allow_press && Keyboard.just_pressed(k.Key) && bind.released()) {
                            bind.press();
                            bind.JustPressed?.Invoke();
                            bind.ActiveInput = d_bind;
                            goto next_bind;
                        }

                        if (Keyboard.just_released(k.Key) && bind.pressed()) {
                            bind.release();
                            bind.JustReleased?.Invoke();
                            bind.ActiveInput = null;
                            goto next_bind;
                        }
                        break;
                    
                    case InputBinds.InputType.Mouse:
                        var m = d_bind as InputBinds.MouseInput;
                        if (allow_press && Mouse.just_pressed(m.MouseButton) && bind.released()) {
                            bind.press();
                            bind.JustPressed?.Invoke();
                            bind.ActiveInput = d_bind;
                            goto next_bind;
                        }

                        if (Mouse.just_released(m.MouseButton) && bind.pressed()) {
                            bind.release();
                            bind.JustReleased?.Invoke();
                            bind.ActiveInput = null;
                            goto next_bind;
                        }
                        break;
                    
                    case InputBinds.InputType.XInput:
                        var x = d_bind as InputBinds.XInputInput;
                        if (allow_press && XInput.just_pressed(x.Digital) && bind.released()) {
                            bind.press();    
                            bind.JustPressed?.Invoke();    
                            bind.ActiveInput = d_bind;
                            goto next_bind;
                        }
                        
                        if (XInput.just_released(x.Digital) && bind.pressed()) {
                            bind.release();    
                            bind.JustReleased?.Invoke();    
                            bind.ActiveInput = null;
                            goto next_bind;
                        }
                        break;
                    
                    case InputBinds.InputType.XInputAnalog:
                        var xa = d_bind as InputBinds.XInputAnalogInput;
                        bind.update_analog_state(XInput.analog_values[xa.Analog]);
                        
                        if (allow_press && XInput.had_value(xa.Analog) < XInput.analog_to_digital_threshold && XInput.value(xa.Analog) > XInput.analog_to_digital_threshold && bind.released()) {
                            bind.press();
                            bind.JustPressed?.Invoke();
                            bind.ActiveInput = d_bind;
                            goto next_bind;
                        }

                        if (XInput.had_value(xa.Analog) > XInput.analog_to_digital_threshold && XInput.value(xa.Analog) < XInput.analog_to_digital_threshold && bind.pressed()) {
                            bind.release();
                            bind.JustReleased?.Invoke();
                            bind.ActiveInput = null;
                            goto next_bind;
                        }
                        break;
                    
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            next_bind: continue;
        }
    }
    
    private bool bind_enabled(string bind_name) {
        if (!global_enable) {
            if (binds[bind_name].AlwaysEnabled) {
                return true;
            } else {
                return false;
            }
        } else {
            return true;
        }
    }

    public void force_enable(string bind_name) {
        binds[bind_name].AlwaysEnabled = true;
    }
    
    public bool pressed(string bind_name) {
        if (bind_enabled(bind_name))
            return binds[bind_name].pressed();
        return false;
    }

    public float value(string bind_name) {
        if (bind_enabled(bind_name)) {
            return binds[bind_name].analog_value();
        }
        return 0f;
    }
    
    public bool just_pressed(string bind_name) {
        if (bind_enabled(bind_name)) 
            return binds[bind_name].just_pressed();
        return false;
    }
    public bool just_released(string bind_name) {
        if (bind_enabled(bind_name))
            return binds[bind_name].just_released();
        return false;
    }

    public bool held(string bind_name) {
        if (bind_enabled(bind_name)) return binds[bind_name].digital_state == InputBinds.PressedState.Held;
        return false;
    }

    public bool held_repeat(string bind_name) {
        if (bind_enabled(bind_name)) {
            return binds[bind_name].HeldBlip;
        }

        return false;
    }
    
    public bool double_pressed(string bind_name) {
        if (bind_enabled(bind_name)) return binds[bind_name].digital_state == InputBinds.PressedState.DoublePressed;
        return false;
    }
    
    public bool tapped(string bind_name) {
        if (bind_enabled(bind_name)) return binds[bind_name].digital_state == InputBinds.PressedState.Tapped;
        return false;
    }
    
    public bool double_tapped(string bind_name) {
        if (bind_enabled(bind_name)) return binds[bind_name].digital_state == InputBinds.PressedState.DoubleTapped;
        return false;
    }

    public Vector2i mouse_delta = Vector2i.Zero; 
    private static Point mouse_lock_stored_position;

    public bool MouseLocked => MouseWatcher.MouseLocked;
    public bool MouseLockedPrevious => MouseWatcher.MouseLockedPrevious;
    
    private bool center_mouse_at_frame_end = false;
        
    private MouseState mouse_state;
    public MouseState MouseState => mouse_state;
        
    private MouseState mouse_state_prev;
    public MouseState MouseStatePrevious => mouse_state_prev;
    
}

public static class InputBinds {
    //ENUMS
    public enum InputType { Keyboard, Mouse, XInput, XInputAnalog }
    public enum BindType { Digital, Analog }

    public enum PressedState { Released, Pressed, Held, Tapped, DoubleTapped, DoublePressed, JustPressed, JustReleased }
    
    //BIND TYPES
    public class Bind {
        public string Name { get; set; } = "UNNAMED";
        
        public List<IInput> Inputs { get; set; } = new();

        public bool AlwaysEnabled { get; set; } = false;

        public PressedState digital_state = PressedState.Released;

        
        private float analog_state = 0f;
        public float AnalogState => analog_state;
        
        public IInput ActiveInput { get; set; }
        
        public Action JustPressed { get; set; }
        public Action JustReleased { get; set; }
        
        public Bind(string name, IInput input) {
            Name = name; 
            Inputs.Add(input);

        }

        public Bind(string name, params IInput[] binds) {
            Name = name;
            Inputs.AddRange(binds);
        }

        public void add_input(IInput input) {
            Inputs.Add(input);
        }

        public bool pressed() {
            if (digital_state != PressedState.Released && digital_state != PressedState.JustReleased && digital_state != PressedState.Tapped)
                return true;
            return false;
        }
        
        public bool released() {
            return !pressed();
        }

        public bool just_pressed() {
            if (digital_state == PressedState.JustPressed)
                return true;
            return false;
        }

        public bool just_released() {
            if (digital_state == PressedState.JustReleased)
                return true;
            return false;
        }

        internal void update_analog_state(float value) {
            analog_state = value;
        }

        public float analog_value() {
            var value = 0f; 
            
            if (analog_state > 0f) value = analog_state;
            else if (pressed() || just_pressed()) value = 1f;
            
            return value;
        }
        
        private double pressed_at = 0;
        public double PressedAt => pressed_at;

        private bool double_tap_eligible = false;
        private double double_tap_timer = 0;
        private double double_tap_timer_second_press = 0;

        private double held_at = 0;
        public double HeldAt => held_at;

        private double held_blip_timer = 0;
        private bool held_blip = false;

        public bool HeldBlip => held_blip;

        private double held_blip_repeat_time => gvars.get_int("i_hold_repeat_time");
        
        public double PressedForMs => digital_state != PressedState.Released ? Clock.game_run_time_ms - pressed_at : 0;
        
        internal void press() {
            pressed_at = Clock.game_run_time_ms;
            
            if (double_tap_eligible && pressed_at - double_tap_timer < gvars.get_int("i_bind_tap_time")) {
                double_tap_timer_second_press = Clock.game_run_time_ms;
                digital_state = PressedState.DoublePressed;
            } else {
                digital_state = PressedState.JustPressed;
            }

            analog_state = 1f;
        }
        
        internal void release() {
            digital_state = PressedState.JustReleased;
            analog_state = 0f;
        }
        
        internal void update() {
            held_blip = false;
            if (digital_state == PressedState.Held) {
                if (Clock.game_run_time_ms - held_blip_timer > held_blip_repeat_time) {
                    held_blip = true;
                    held_blip_timer = Clock.game_run_time_ms;
                }
            }
            
            if (digital_state == PressedState.Tapped) {
                digital_state = PressedState.Released;
            }

            if (digital_state == PressedState.DoubleTapped) {
                digital_state = PressedState.Released;
            }

            if (pressed_at - double_tap_timer > gvars.get_int("i_bind_tap_time")) {
                double_tap_eligible = false;
            }
            
            if (digital_state == PressedState.Pressed) {
                if (PressedForMs > gvars.get_int("i_bind_tap_time")) {
                    digital_state = PressedState.Held;
                    held_at = Clock.game_run_time_ms;
                }
            } else if (digital_state == PressedState.JustReleased) {
                if (PressedForMs < gvars.get_int("i_bind_tap_time")) {
                    if (!double_tap_eligible) {
                        double_tap_eligible = true;
                        double_tap_timer = Clock.game_run_time_ms;
                     
                        digital_state = PressedState.Tapped;
                    } else {
                        digital_state = PressedState.DoubleTapped;
                        double_tap_eligible = false;
                    }
                }
                pressed_at = 0;
                
            } else if (digital_state == PressedState.DoublePressed) {
                digital_state = PressedState.JustPressed;
                
            } else if (digital_state == PressedState.JustPressed) {
                digital_state = PressedState.Pressed;
            }
            
            if (digital_state == PressedState.JustReleased) {
                digital_state = PressedState.Released;
            }

        }
    }

    
    public interface IInput {
        public InputType InputType { get;  }
        public BindType BindType { get; }
    }
    
    public abstract class DigitalInput : IInput {
        public InputType InputType { get; }
        public BindType BindType => BindType.Digital;

        public DigitalInput(InputType input_type) {
            this.InputType = input_type;
        }
    }
    
    public abstract class AnalogInput : IInput {
        public InputType InputType => InputType.XInputAnalog;
        public BindType BindType => BindType.Analog;
        
    }
    
    
    //DIGITAL
    public class KeyInput : DigitalInput {
        public InputType InputType => InputType.Keyboard;

        private Keys key;
        public Keys Key => key;
        
        public KeyInput(Keys key) : base(InputType.Keyboard) {
            this.key = key;
        }
    }

    public class MouseInput : DigitalInput {
        public InputType InputType => InputType.Mouse;

        private MouseWatcher.MouseButtons mouse_button;
        public MouseWatcher.MouseButtons MouseButton => mouse_button;
        
        public MouseInput(MouseWatcher.MouseButtons mouse_button): base(InputType.Mouse) {
            this.mouse_button = mouse_button;
        }
    }

    public class XInputInput : DigitalInput {
        public InputType InputType => InputType.XInput;

        private XInputDigital digital;
        public XInputDigital Digital => digital;
        
        public XInputInput(XInputDigital digital): base(InputType.XInput) {
            this.digital = digital;
        }
    }
    
    //ANALOG
    public class XInputAnalogInput : AnalogInput {
        InputType InputType => InputType.XInputAnalog;

        private XInputAnalog analog;
        public XInputAnalog Analog => analog;
        
        public XInputAnalogInput(XInputAnalog analog) {
            this.analog = analog;
        }
    }

}


