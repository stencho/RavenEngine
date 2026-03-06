using Microsoft.Xna.Framework.Input;

namespace Raven.Engine.Controls;

[InputWatcher]
public partial class XInputWatcher {
    public static partial class Manager {
        private static GamePadState gamepad_state;
        public static GamePadState GamePadState => gamepad_state;
        
        private static GamePadState gamepad_state_prev;
        public static GamePadState GamePadStatePrevious => gamepad_state_prev;

        
    }
    
    #region enums

    public enum XInputAxis {
        LeftStickUp, LeftStickDown, LeftStickLeft, LeftStickRight,
        RightStickUp, RightStickDown, RightStickLeft, RightStickRight,
        TriggerL, TriggerR
    }

    public enum XInputStick {
        Left,
        Right
    }

    public enum XInputButtons {
        A, B, X, Y,
        LB, RB,
        DPadUp, DPadDown, DPadLeft, DPadRight,
        Start, Back,
        LStick, RStick
    }

    #endregion

}