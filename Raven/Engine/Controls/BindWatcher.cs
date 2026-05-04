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

namespace Raven.Engine.Controls;

//WATCHER
public class BindWatcher {
    public KeyboardWatcher Keyboard = new KeyboardWatcher();
    public MouseWatcher Mouse = new MouseWatcher();
    public XInputWatcher XInput = new XInputWatcher();
    
    //TODO XINPUT WATCHER
    
    //private static XInputWatcher XInput = new XInputWatcher();

    public Dictionary<string, InputBinds.Bind> binds = new();

    public static volatile bool global_enable = true;
    static volatile bool global_enable_prev = true;
    
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

    public void AddBind(string name, Keys key) =>
        AddBind(new InputBinds.Bind(name, new InputBinds.KeyInput(key)));
    
    public void AddBind(string name, MouseWatcher.MouseButtons mouse_button) =>
        AddBind(new InputBinds.Bind(name, new InputBinds.MouseInput(mouse_button)));
    
    public void AddBind(string name, XInputWatcher.XInputButtons button) =>
        AddBind(new InputBinds.Bind(name, new InputBinds.XInputInput(button)));
    
    public void AddBind(string name, XInputWatcher.XInputAxis axis) =>
        AddBind(new InputBinds.Bind(name, new InputBinds.XInputAnalogInput(axis)));
    
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
                    case XInputWatcher.XInputButtons xinput_button:
                        AddBind(b.name, xinput_button);
                        break;
                    case XInputWatcher.XInputAxis xinput_axis:
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
        
        foreach (var b in binds.Values) {
            //if (!b.released()) {
                if (c) s += "\n"; else c = true;
                s += $"[{b.Name}] P:{b.pressed()} D:{b.DigitalState} A:{b.AnalogState} -> ";
                var cc = false;
                foreach (var i in b.Inputs) {
                    if (cc) s += " | "; else cc = true;
                    if (i.BindType == InputBinds.BindType.Digital) {
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
                                
                                if (i.BindType == InputBinds.BindType.Analog) {
                                    var xi = i as InputBinds.XInputAnalogInput;     
                                    s += $"[{xi.Axis}] IDK";
                                } else {
                                    var xi = i as InputBinds.XInputInput;
                                    s += $"[{xi.Button}] IDK";
                                }
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                        
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
        
        // go through each bind known to this bind watcher
        foreach (var bind in binds.Values) {
            // handle releasing digital state
            if (bind.pressed()) {
                // go through each of the inputs and check if any are pressed
                // if they are, skip to the next bind as this one is still pressed 
                foreach (var d_bind in bind.Inputs) {
                    switch (d_bind.InputType) {
                        case InputBinds.InputType.Keyboard:
                            var k = d_bind as InputBinds.KeyInput;
                            if (Keyboard.is_pressed(k.Key)) {
                                goto next;
                            }
                            break;
                        case InputBinds.InputType.Mouse:
                            var mb = d_bind as InputBinds.MouseInput;
                            if (Mouse.is_pressed(mb.MouseButton)) {
                                goto next;
                            }
                            break;

                        case InputBinds.InputType.XInput:
                            if (d_bind.BindType == InputBinds.BindType.Analog) {

                            } else {
                                //TODO XInputWatcher
                            }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                
                // made it through all inputs and none were pressed, so release the bind
                bind.release();
                bind.JustReleased?.Invoke();
                bind.ActiveInput = null;
                
            // handle pressing digital state    
            } else {
                // go through each of the inputs and check if they have just been pressed
                // if they have, press the bind
                foreach (var d_bind in bind.Inputs) {
                    switch (d_bind.InputType) {
                        case InputBinds.InputType.Keyboard:
                            var k = d_bind as InputBinds.KeyInput;
                            if (Keyboard.just_pressed(k.Key)) {
                                bind.press();
                                bind.ActiveInput = d_bind;
                                bind.JustPressed?.Invoke();
                                goto next;
                            }
                            break;
                        case InputBinds.InputType.Mouse:
                            var mb = d_bind as InputBinds.MouseInput;
                            if (Mouse.just_pressed(mb.MouseButton)) {
                                bind.press();
                                bind.ActiveInput = d_bind;
                                bind.JustPressed?.Invoke();
                                goto next;
                            }
                            break;

                        case InputBinds.InputType.XInput:
                            if (d_bind.BindType == InputBinds.BindType.Analog) {

                            } else {
                                //TODO XInputWatcher
                            }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            next: continue;
        }
    }
    
    public void UpdateEnd() {
        foreach (var b in binds.Values) {
            b.end_of_update();
        }
        
        Keyboard.UpdateEnd();
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
        if (bind_enabled(bind_name)) return binds[bind_name].DigitalState == InputBinds.PressedState.Held;
        return false;
    }
    
    public bool double_pressed(string bind_name) {
        if (bind_enabled(bind_name)) return binds[bind_name].DigitalState == InputBinds.PressedState.DoublePressed;
        return false;
    }
    
    public bool tapped(string bind_name) {
        if (bind_enabled(bind_name)) return binds[bind_name].DigitalState == InputBinds.PressedState.Tapped;
        return false;
    }
    
    public bool double_tapped(string bind_name) {
        if (bind_enabled(bind_name)) return binds[bind_name].DigitalState == InputBinds.PressedState.DoubleTapped;
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
    public enum InputType { Keyboard, Mouse, XInput }
    public enum BindType { Digital, Analog, Delta /*, Absolute hehe could be fun to support tablets*/ }

    public enum PressedState { Released, Pressed, Held, Tapped, DoubleTapped, DoublePressed, JustPressed, JustReleased }
    
    //BIND TYPES
    public class Bind {
        public string Name { get; set; } = "UNNAMED";
        
        public List<IInput> Inputs { get; set; } = new();

        public bool AlwaysEnabled { get; set; } = false;

        private PressedState digital_state = PressedState.Released;
        public PressedState DigitalState => digital_state;

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

        public float analog_value() {
            return 0f;
        }
        
        private double pressed_at = 0;
        public double PressedAt => pressed_at;

        private bool double_tap_eligible = false;
        private double double_tap_timer = 0;
        private double double_tap_timer_second_press = 0;
        
        
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
            //analog_state = highest value from an analog input ig
        }
        
        internal void end_of_update() {
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
        public InputType InputType => InputType.XInput;
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

        private XInputWatcher.XInputButtons button;
        public XInputWatcher.XInputButtons Button => button;
        
        public XInputInput(XInputWatcher.XInputButtons button): base(InputType.XInput) {
            this.button = button;
        }
    }

    
    //ANALOG
    public class XInputAnalogInput : AnalogInput {
        InputType InputType => InputType.XInput;

        private XInputWatcher.XInputAxis axis;
        public XInputWatcher.XInputAxis Axis => axis;
        
        public XInputAnalogInput(XInputWatcher.XInputAxis axis) {
            this.axis = axis;
        }
    }

}


