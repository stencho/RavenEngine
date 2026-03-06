using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using CSScripting;
using Microsoft.Xna.Framework.Input;
using Raven.Engine;
using Raven.Engine.Controls;

namespace Raven.Engine.Controls;

//WATCHER
public class BindWatcher {
    public KeyboardWatcher Keyboard = new KeyboardWatcher();
    public MouseWatcher Mouse = new MouseWatcher();
    
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
    
    public void AddBind(string name, Input.XInputButtons button) =>
        AddBind(new InputBinds.Bind(name, new InputBinds.XInputInput(button)));
    
    public void AddBind(string name, Input.XInputAxis axis) =>
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
                    case Input.XInputButtons xinput_button:
                        AddBind(b.name, xinput_button);
                        break;
                    case Input.XInputAxis xinput_axis:
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
                s += $"[{b.Name}] -> ";
                var cc = false;
                foreach (var i in b.Inputs) {
                    if (cc) s += " | "; else cc = true;
                    if (i.BindType == InputBinds.BindType.Digital) {
                        switch (i.InputType) {
                            case InputBinds.InputType.Keyboard:
                                InputBinds.KeyInput k = i as InputBinds.KeyInput;
                                s += $"[{k.Key}] {k.State.ToString()} {k.PressedForMs}";
                                break;
                            case InputBinds.InputType.Mouse:
                                var m = i as InputBinds.MouseInput;
                                s += $"[{m.MouseButton}] {m.State.ToString()} {m.PressedForMs}";
                                break;
                            case InputBinds.InputType.XInput:
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
        Keyboard.Update();
        
        foreach (var bind in binds.Values) {
            foreach (var d_bind in bind.Inputs) {
                switch (d_bind.InputType) {
                    case InputBinds.InputType.Keyboard:
                        var k = d_bind as InputBinds.KeyInput;
                        if (Keyboard.just_pressed(k.Key)) {
                            (d_bind as InputBinds.KeyInput)?.press();
                        } else if (Keyboard.just_released(k.Key)) {
                            (d_bind as InputBinds.KeyInput)?.release();
                        }
                        break;
                    case InputBinds.InputType.Mouse:
                        var mb = d_bind as InputBinds.MouseInput;
                        if (Mouse.just_pressed(mb.MouseButton)) {
                            (d_bind as InputBinds.MouseInput)?.press();
                        } else if (Mouse.just_released(mb.MouseButton)) {
                            (d_bind as InputBinds.MouseInput)?.release();
                        }
                        break;
                    
                    case InputBinds.InputType.XInput:
                        if (d_bind.BindType == InputBinds.BindType.Analog) {
                            
                        } else {
                            
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
    
    public void UpdateEnd() {
        foreach (var b in binds.Values) {
            foreach (var input in b.Inputs) {
                var bind = input as InputBinds.DigitalInput;
                bind.end_of_update();
            }
        }
        Mouse.ResetMouseDelta();
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
    
}

public static class InputBinds {
    //ENUMS
    public enum InputType { Keyboard, Mouse, XInput }
    public enum BindType { Digital, Analog, Delta /*, Absolute hehe could be fun to support tablets*/ }

    public enum PressedState { Released, Pressed, Held, Tapped, JustPressed, JustReleased }
    
    //BIND TYPES
    public class Bind {
        public string Name { get; set; } = "UNNAMED";
        
        public List<IInput> Inputs { get; set; } = new();

        public bool AlwaysEnabled { get; set; } = false;
        
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
            foreach (var b in Inputs) {
                if (b.BindType == BindType.Digital) {
                    //DIGITAL
                    var bind = b as DigitalInput;
                    if (bind.State != PressedState.Released && bind.State != PressedState.JustReleased && bind.State != PressedState.Tapped)
                        return true;
                } else {
                    //ANALOG
                    //todo implement analog-to-digital here
                }
            }
            return false;
        }
        
        public bool released() {
            return !pressed();
        }

        public bool just_pressed() {
            foreach (var b in Inputs) {
                if (b.BindType == BindType.Digital) {
                    var bind = b as DigitalInput;
                    if (bind.State == PressedState.JustPressed)
                        return true;
                } else {
                    
                }
            }
            return false;
        }

        public bool just_released() {
            foreach (var b in Inputs) {
                if (b.BindType == BindType.Digital) {
                    var bind = b as DigitalInput;
                    if (bind.State == PressedState.JustReleased)
                        return true;
                } else {
                    
                }
            }
            return false;
        }

        public float analog_value() {
            foreach (var b in Inputs) {
                if (b.BindType == BindType.Analog) {
                    //ANALOG
                    
                } else {
                    
                }
            }

            return 0f;
        }
        
    }

    
    public interface IInput {
        public InputType InputType { get;  }
        public BindType BindType { get; }
    }
    
    public abstract class DigitalInput : IInput {
        public InputType InputType { get; }
        public BindType BindType => BindType.Digital;

        private double pressed_at = 0;
        public double PressedAt => pressed_at;
        
        public double PressedForMs => state != PressedState.Released ? Clock.game_run_time_ms - pressed_at : 0;

        private PressedState state = PressedState.Released;
        public PressedState State => state;

        public DigitalInput(InputType input_type) {
            this.InputType = input_type;
        }
        
        internal void press() {
            pressed_at = Clock.game_run_time_ms;
            state = PressedState.JustPressed;
        }
        
        internal void release() {
            state = PressedState.JustReleased;
        }

        public void end_of_update() {
            if (state == PressedState.Tapped) {
                state = PressedState.Released;
            }
            
            if (state == PressedState.Pressed) {
                if (PressedForMs > gvars.get_int("bind_tap_time")) {
                    state = PressedState.Held;
                }
            } else if (state == PressedState.JustReleased) {
                if (PressedForMs < gvars.get_int("bind_tap_time")) {
                    state = PressedState.Tapped;
                }
                pressed_at = 0;
            } else if (state == PressedState.JustPressed) {
                state = PressedState.Pressed;
            }

            if (state == PressedState.JustReleased) {
                state = PressedState.Released;
            }
        }
    }
    
    public abstract class AnalogInput : IInput {
        public InputType InputType { get; }
        public BindType BindType => BindType.Analog;

        private float value = 0f;
        public float Value => value;
        
        internal void SetValue(float v) => value = v;
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

        private Input.XInputButtons button;
        public Input.XInputButtons Button => button;
        
        public XInputInput(Input.XInputButtons button): base(InputType.XInput) {
            this.button = button;
        }
    }

    
    //ANALOG
    public class XInputAnalogInput : AnalogInput {
        InputType InputType => InputType.XInput;

        private Input.XInputAxis axis;
        
        public XInputAnalogInput(Input.XInputAxis axis) {
            this.axis = axis;
        }
    }

}


