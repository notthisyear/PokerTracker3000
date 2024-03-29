namespace PokerTracker3000.GamepadInput
{
    public enum GamepadEventType
    {
        Disconnected,
        BatteryLevel,
        Input
    }

    public enum GamepadAnalogInput
    {
        LeftTrigger,
        RightTrigger,
        LeftStick,
        RightStick
    }

    public enum GamepadDigitalInput
    {
        None,
        DPadUp,
        DPadDown,
        DPadLeft,
        DPadRight,
        StartButton,
        BackButton,
        LeftStickButton,
        RightStickButton,
        LeftShoulderButton,
        RightSholderButton,
        AButton,
        BButton,
        XButton,
        YButton
    }

    public enum GamepadDigitalInputState
    {
        Pressed,
        Released,
        Held
    }


    public enum GamepadBatteryLevel
    {
        None,
        Empty,
        Low,
        Medium,
        Full
    }

    public enum InputEventType
    {
        Button,
        Trigger,
        Stick
    }
}
