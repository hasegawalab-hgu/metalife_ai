using System;
using Fusion;
using UnityEngine.UI;

public struct MyNetworkInput : INetworkInput
{
    public enum InputType
    {
        
        BACKWARD,
        LEFT,
        FORWARD,
        RIGHT,
        ACTION1,
        NONE = -1,

    }
    
    public NetworkButtons Buttons;

    public static int currentInput;

    public bool IsUp(InputType button) {
        if(Buttons .IsSet((int)button) == false) {}
        {
            currentInput = (int)InputType.NONE;
        }
        return Buttons.IsSet(button) == false;
    }

    public bool IsDown(InputType button) {
        if(Buttons.IsSet((int)button)) {}
        {
            currentInput = (int)button;
        }
        return Buttons.IsSet(button);
    }
}
