using System.Collections.Generic;
using PokerTracker3000.GamepadInput;
using PokerTracker3000.GamepadInput.XInput.Enumerations;

namespace PokerTracker3000.Input.GamepadInput
{
    internal class GamepadInternalEvent
    {
        public uint GamepadId { get; init; }

        public GamepadEventType EventType { get; init; }

        public GamepadBatteryLevel GamepadBatteryLevel { get; init; }

        public List<(GamepadDigitalInput button, GamepadDigitalInputState state)> DigitalInput { get; } = [];

        public List<(GamepadAnalogInput trigger, bool isStick, float valueXOrSingle, float valueY)> AnalogInput { get; } = [];

        public static GamepadInternalEvent CreateDisconnectedEvent(uint gamepadId)
            => new() { GamepadId = gamepadId, EventType = GamepadEventType.Disconnected };

        public static GamepadInternalEvent CreateBatteryLevelEvent(uint gamepadId, BatteryLevel batteryLevel)
            => new()
            {
                GamepadId = gamepadId,
                EventType = GamepadEventType.BatteryLevel,
                GamepadBatteryLevel = batteryLevel switch
                {
                    BatteryLevel.BATTERY_LEVEL_EMPTY => GamepadBatteryLevel.Empty,
                    BatteryLevel.BATTERY_LEVEL_LOW => GamepadBatteryLevel.Low,
                    BatteryLevel.BATTERY_LEVEL_MEDIUM => GamepadBatteryLevel.Medium,
                    BatteryLevel.BATTERY_LEVEL_FULL => GamepadBatteryLevel.Full,
                    BatteryLevel.Undefined => GamepadBatteryLevel.None,
                    _ => GamepadBatteryLevel.None
                }
            };

        public static GamepadInternalEvent CreateInputEvent(uint gamepadId)
            => new() { GamepadId = gamepadId, EventType = GamepadEventType.Input };

        public static GamepadInternalEvent CreateInputEvent(uint gamepadId, GamepadState oldState, GamepadState newState, DeadzoneSetting leftStickDeadzoneSetting, DeadzoneSetting rightStickDeadzoneSetting)
        {
            var gamepadEvent = new GamepadInternalEvent() { GamepadId = gamepadId, EventType = GamepadEventType.Input };

            AddButtonEventIfEvent(GamepadDigitalInput.DPadUp, oldState.DPadUpPressed, newState.DPadUpPressed, gamepadEvent.DigitalInput);
            AddButtonEventIfEvent(GamepadDigitalInput.DPadDown, oldState.DPadDownPressed, newState.DPadDownPressed, gamepadEvent.DigitalInput);
            AddButtonEventIfEvent(GamepadDigitalInput.DPadLeft, oldState.DPadLeftPressed, newState.DPadLeftPressed, gamepadEvent.DigitalInput);
            AddButtonEventIfEvent(GamepadDigitalInput.DPadRight, oldState.DPadRightPressed, newState.DPadRightPressed, gamepadEvent.DigitalInput);
            AddButtonEventIfEvent(GamepadDigitalInput.StartButton, oldState.StartButtonPressed, newState.StartButtonPressed, gamepadEvent.DigitalInput);
            AddButtonEventIfEvent(GamepadDigitalInput.BackButton, oldState.BackButtonPressed, newState.BackButtonPressed, gamepadEvent.DigitalInput);
            AddButtonEventIfEvent(GamepadDigitalInput.LeftShoulderButton, oldState.LeftShoulderPressed, newState.LeftShoulderPressed, gamepadEvent.DigitalInput);
            AddButtonEventIfEvent(GamepadDigitalInput.RightSholderButton, oldState.RightShoulderPressed, newState.RightShoulderPressed, gamepadEvent.DigitalInput);
            AddButtonEventIfEvent(GamepadDigitalInput.LeftStickButton, oldState.LeftThumbPressed, newState.LeftThumbPressed, gamepadEvent.DigitalInput);
            AddButtonEventIfEvent(GamepadDigitalInput.RightStickButton, oldState.RightThumbPressed, newState.RightThumbPressed, gamepadEvent.DigitalInput);
            AddButtonEventIfEvent(GamepadDigitalInput.AButton, oldState.AButtonPressed, newState.AButtonPressed, gamepadEvent.DigitalInput);
            AddButtonEventIfEvent(GamepadDigitalInput.BButton, oldState.BButtonPressed, newState.BButtonPressed, gamepadEvent.DigitalInput);
            AddButtonEventIfEvent(GamepadDigitalInput.XButton, oldState.XButtonPressed, newState.XButtonPressed, gamepadEvent.DigitalInput);
            AddButtonEventIfEvent(GamepadDigitalInput.YButton, oldState.YButtonPressed, newState.YButtonPressed, gamepadEvent.DigitalInput);

            if (newState.LeftAnalogTrigger > 0)
                gamepadEvent.AnalogInput.Add((GamepadAnalogInput.LeftTrigger, false, AsPercent(newState.LeftAnalogTrigger), 0.0f));

            if (newState.RightAnalogTrigger > 0)
                gamepadEvent.AnalogInput.Add((GamepadAnalogInput.RightTrigger, false, AsPercent(newState.RightAnalogTrigger), 0.0f));

            if (!leftStickDeadzoneSetting.IsXWithinDeadzone(newState.LeftStickX) || !leftStickDeadzoneSetting.IsYWithinDeadzone(newState.LeftStickY))
                gamepadEvent.AnalogInput.Add((GamepadAnalogInput.LeftStick, true, AsPercent(newState.LeftStickX), AsPercent(newState.LeftStickY)));

            if (!rightStickDeadzoneSetting.IsXWithinDeadzone(newState.RightStickX) || !rightStickDeadzoneSetting.IsYWithinDeadzone(newState.RightStickY))
                gamepadEvent.AnalogInput.Add((GamepadAnalogInput.RightStick, true, AsPercent(newState.RightStickX), AsPercent(newState.RightStickY)));

            return gamepadEvent;
        }

        private static void AddButtonEventIfEvent(GamepadDigitalInput input, bool oldState, bool newState, List<(GamepadDigitalInput, GamepadDigitalInputState)> eventList)
        {
            if (newState || (oldState != newState))
            {
                eventList.Add((input,
                    (newState == oldState) ? GamepadDigitalInputState.Held :
                    (newState ? GamepadDigitalInputState.Pressed : GamepadDigitalInputState.Released)));
            }
        }

        private static float AsPercent(byte v)
            => v / (float)byte.MaxValue;

        private static float AsPercent(short v)
            => v < 0 ? -(v / (float)(short.MinValue)) : v / (float)short.MaxValue;
    }
}
