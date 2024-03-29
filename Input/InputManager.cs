﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using PokerTracker3000.GamepadInput;
using PokerTracker3000.Input.GamepadInput;
using static PokerTracker3000.Input.InputManager.UserInputEvent;

namespace PokerTracker3000.Input
{
    public class InputManager
    {
        public record UserInputEvent
        {
            public enum NavigationDirection
            {
                None,
                Left,
                Right,
                Up,
                Down
            }

            public enum ButtonEventType
            {
                None,
                Start,
                Select,
                GoBack,
                InfoButton
            }

            public enum ButtonAction
            {
                None,
                Down,
                Up
            };

            public bool IsButtonEvent { get; init; } = false;

            public bool IsNavigationEvent { get; init; } = false;

            public bool AllowRepeat { get; init; } = false;

            public ButtonEventType Button { get; init; } = ButtonEventType.None;

            public ButtonAction Action { get; init; } = ButtonAction.None;

            public NavigationDirection Direction { get; init; } = NavigationDirection.None;

            internal bool Matches(KeyEventArgs e)
                => e.IsRepeat == AllowRepeat;

            internal static UserInputEvent GetNavigationEvent(NavigationDirection direction, ButtonAction action, bool allowRepeat)
                => new()
                {
                    IsNavigationEvent = true,
                    Direction = direction,
                    Action = action,
                    AllowRepeat = allowRepeat
                };

            internal static UserInputEvent GetButtonEvent(ButtonEventType button, ButtonAction action, bool allowRepeat)
                => new()
                {
                    IsButtonEvent = true,
                    Button = button,
                    Action = action,
                    AllowRepeat = allowRepeat
                };

            internal static UserInputEvent GetNavigationEvent(NavigationDirection direction, GamepadDigitalInputState state)
                => new()
                {
                    IsNavigationEvent = true,
                    Direction = direction,
                    Action = state == GamepadDigitalInputState.Pressed ? ButtonAction.Down : ButtonAction.Up,
                };

            internal static UserInputEvent GetButtonEvent(ButtonEventType button, GamepadDigitalInputState state)
                => new()
                {
                    IsButtonEvent = true,
                    Button = button,
                    Action = state == GamepadDigitalInputState.Pressed ? ButtonAction.Down : ButtonAction.Up,

                };
        }

        private readonly Dictionary<Key, Dictionary<ButtonAction, UserInputEvent>> _keyboardEvents = [];
        private readonly Dictionary<GamepadDigitalInput, Dictionary<ButtonAction, UserInputEvent>> _gamepadEvents = [];
        private readonly GamepadManager _gamepadManager;
        private readonly Action<UserInputEvent> _inputEventHandler;

        public InputManager(Action<UserInputEvent> inputEventHandler)
        {
            _inputEventHandler = inputEventHandler;

            _gamepadManager = new();
            _gamepadManager.GamepadConnected += (s, e) =>
            {
                Debug.WriteLine($"Gamepad {e.Id} connected - IsWireless: {e.IsWireless}, CanGetBatteryLevel: {e.CanGetBatteryLevel}");
                e.GamepadDisconnected += LostGamepadConnection;
                if (e.CanGetBatteryLevel)
                    e.NewGamepadBatteryLevel += NewBatteryLevelReceived;

                e.NewGamepadInput += NewGamepadInput;
            };
        }

        public void RegisterKeyboardEvent(Key keyboardKey, NavigationDirection direction, ButtonAction action = ButtonAction.Down, bool allowRepeat = false)
        {
            AddKeyboardEvent(keyboardKey, GetNavigationEvent(direction, action, allowRepeat));
        }

        public void RegisterKeyboardEvent(Key keyboardKey, ButtonEventType button, ButtonAction action = ButtonAction.Down, bool allowRepeat = false)
        {
            AddKeyboardEvent(keyboardKey, GetButtonEvent(button, action, allowRepeat));
        }

        public void RegisterGamepadEvent(GamepadDigitalInput input, NavigationDirection direction, GamepadDigitalInputState state = GamepadDigitalInputState.Pressed)
        {
            if (state == GamepadDigitalInputState.Held)
                throw new NotImplementedException($"{GamepadDigitalInputState.Held} are not yet implemented");
            AddGamepadEvent(input, GetNavigationEvent(direction, state));
        }

        public void RegisterGamepadEvent(GamepadDigitalInput input, ButtonEventType button, GamepadDigitalInputState state = GamepadDigitalInputState.Pressed)
        {
            if (state == GamepadDigitalInputState.Held)
                throw new NotImplementedException($"{GamepadDigitalInputState.Held} are not yet implemented");
            AddGamepadEvent(input, GetButtonEvent(button, state));
        }

        public void OnKeyboardEvent(KeyEventArgs keyEvent)
        {
            if (!_keyboardEvents.TryGetValue(keyEvent.Key, out var inputEvents))
                return;

            var action = keyEvent.IsDown ? ButtonAction.Down : (keyEvent.IsUp ? ButtonAction.Up : ButtonAction.None);
            if (inputEvents.TryGetValue(action, out var e) && e.Matches(keyEvent))
                _inputEventHandler?.Invoke(e);
        }

        private void NewGamepadInput(object? sender, InputEvent inputEvent)
        {
            if (inputEvent == default || inputEvent.Type != InputEventType.Button || inputEvent is not ButtonEvent buttonEvent)
                return;

            if (!_gamepadEvents.TryGetValue(buttonEvent.Button, out var inputEvents))
                return;

            var action = buttonEvent.State == GamepadDigitalInputState.Pressed ? ButtonAction.Down : ButtonAction.Up;
            if (inputEvents.TryGetValue(action, out var e))
                Application.Current.Dispatcher.Invoke(() => _inputEventHandler?.Invoke(e));
        }

        private void AddKeyboardEvent(Key keyboardKey, UserInputEvent newEvent)
        {
            if (!_keyboardEvents.ContainsKey(keyboardKey))
                _keyboardEvents.Add(keyboardKey, []);

            _ = _keyboardEvents[keyboardKey].TryAdd(newEvent.Action, newEvent);
        }

        private void AddGamepadEvent(GamepadDigitalInput input, UserInputEvent newEvent)
        {
            if (!_gamepadEvents.ContainsKey(input))
                _gamepadEvents.Add(input, []);

            _ = _gamepadEvents[input].TryAdd(newEvent.Action, newEvent);
        }

        private void LostGamepadConnection(object? sender, EventArgs e)
        {
            if (sender is Gamepad gamepad)
            {
                if (gamepad.CanGetBatteryLevel)
                    gamepad.NewGamepadBatteryLevel -= NewBatteryLevelReceived;
                gamepad.NewGamepadInput -= NewGamepadInput;
                gamepad.GamepadDisconnected -= LostGamepadConnection;
                Debug.WriteLine($"Got disconnected event from gamepad {gamepad.Id}");
            }
        }

        private void NewBatteryLevelReceived(object? sender, GamepadBatteryLevel e)
        {
            Debug.WriteLine($"Got battery level {e}");
        }
    }
}
