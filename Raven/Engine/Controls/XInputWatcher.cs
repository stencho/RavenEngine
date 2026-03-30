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
    
    public void Update() {
        gamepad_state_previous = gamepad_state; 
        gamepad_state = GamePad.GetState(player_index);
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