using System.Collections.Generic;
using CSScripting;
using Microsoft.Xna.Framework.Input;

namespace Raven.Engine.Controls;

public static class InputBinds {
    public static HashSet<Bind> global_binds = new();
    
    
    //ENUMS
    public enum InputType { Keyboard, Mouse, XInput }
    public enum BindType { Digital, Analog }

    public enum PressedState { Released, Pressed, Held, Tapped, JustPressed, JustReleased }
    
    
    //STATE
    public class BindState {
        private double pressed_at = 0;
        public double PressedAt => pressed_at;
        
        public double PressedForMs => Clock.game_run_time_ms - pressed_at;

        public bool CheckedInThread { get; set; } = false;

        private PressedState state = PressedState.Released;
        public PressedState GetStateAndUpdateJustState() {
                if (state == PressedState.JustPressed)
                    state = PressedState.Pressed;
                else if (state == PressedState.JustReleased)
                    state = PressedState.Released;
                else if (state == PressedState.Tapped) 
                    state = PressedState.Released;
                return state;
        }
        public BindState() {}

        internal void press(double delta) {
            pressed_at = Clock.game_run_time_ms;
            state = PressedState.JustPressed;
            CheckedInThread = false;
        }
        
        internal void release() {
            pressed_at = 0;
            state = PressedState.JustReleased;
        }

        internal void update() {
            if (state == PressedState.Pressed) {
                if (PressedForMs > gvars.get_int("bind_tap_time")) {
                    state = PressedState.Held;
                }
            }
            else if (state == PressedState.JustReleased) {
                if (PressedForMs < gvars.get_int("bind_tap_time")) {
                    state = PressedState.Tapped;
                }
            }
        }
    }

    
    //BIND TYPES
    public class Bind {
        public string Name { get; set; }
        public BindState BindState { get; set; }
        public List<IBind> Binds { get; set; } 
    }
    
    public interface IBind {
        public InputType InputType { get;  }
        public BindType BindType { get; }
    }
    
    public abstract class DigitalBind : IBind {
        public InputType InputType { get; }
        public BindType BindType => BindType.Digital;
        public bool Pressed { get; internal set; }
    }
    
    public abstract class AnalogBind : IBind {
        public InputType InputType { get; }
        public BindType BindType => BindType.Analog;
    }
    
    
    //DIGITAL
    public class KeyBind : DigitalBind {
        public InputType InputType => InputType.Keyboard;

        private Keys key;
        public Keys Key => key;
        
        public KeyBind(Keys key) {
            this.key = key;
        }
    }

    public class MouseBind : DigitalBind {
        public InputType InputType => InputType.Mouse;

        private MouseWatcher.MouseButtons mouse_button;
        public MouseWatcher.MouseButtons MouseButton => mouse_button;
        
        public MouseBind(MouseWatcher.MouseButtons mouse_button) {
            this.mouse_button = mouse_button;
        }
    }

    public class XInputBind : DigitalBind {
        public InputType InputType => InputType.XInput;

        private Input.XInputButtons button;
        public Input.XInputButtons Button => button;
        
        public XInputBind(Input.XInputButtons button) {
            this.button = button;
        }
    }

    
    //ANALOG
    public class XInputAnalogBind : AnalogBind {
        InputType InputType => InputType.XInput;

        private Input.XInputAxis axis;
        
        public XInputAnalogBind(Input.XInputAxis axis) {
            this.axis = axis;
        }
    }

    public static void AddGlobalBind(Bind bind) => global_binds.Add(bind);
    public static void AddMultipleGlobalBinds(params Bind[] binds) => global_binds.AddItems(binds); 
    public static void RemoveGlobalBind(Bind bind) => global_binds.Remove(bind);

    
    //WATCHER
    [InputWatcher]
    public class BindWatcher {
        public static partial class Manager {
            public static void Update() {
                
            }
        }
        
        private static KeyboardWatcher Keyboard = new KeyboardWatcher();
        private static MouseWatcher Mouse = new MouseWatcher();

        public HashSet<Bind> binds = new();

        public void AddBind(Bind bind) => binds.Add(bind);
        public void AddMultipleBinds(params Bind[] binds) => binds.AddItems(binds); 
        public void RemoveBind(Bind bind) => binds.Remove(bind);

        public BindWatcher() {
            
        }
        
        public void Update() {
            foreach (Bind b in binds) {
                b.BindState.update();
            }
            
            Mouse.ResetMouseDelta();
        }
        
    }
}