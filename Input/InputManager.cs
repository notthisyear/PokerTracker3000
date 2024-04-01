using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using PokerTracker3000.GamepadInput;
using PokerTracker3000.Input.GamepadInput;

using static PokerTracker3000.Input.UserInputEvent;

namespace PokerTracker3000.Input
{
    public class InputManager : ObservableObject
    {
        #region Public properties

        #region Private fields
        private bool _showControllerConnectedInfo = false;
        private bool _lastControllerConnectedWasDisconnected = false;
        private string _controllerInfo = string.Empty;
        #endregion

        public bool ShowControllerConnectedInfo
        {
            get => _showControllerConnectedInfo;
            private set => SetProperty(ref _showControllerConnectedInfo, value);
        }

        public bool LastControllerConnectedWasDisconnected
        {
            get => _lastControllerConnectedWasDisconnected;
            private set => SetProperty(ref _lastControllerConnectedWasDisconnected, value);
        }

        public string ControllerInfo
        {
            get => _controllerInfo;
            private set => SetProperty(ref _controllerInfo, value);
        }
        #endregion

        #region Private fields
        private readonly Dictionary<Key, Dictionary<ButtonAction, UserInputEvent>> _keyboardEvents = [];
        private readonly Dictionary<GamepadDigitalInput, Dictionary<ButtonAction, UserInputEvent>> _gamepadEvents = [];
        private readonly GamepadManager _gamepadManager;
        private readonly Action<UserInputEvent> _inputEventHandler;
        #endregion

        public InputManager(Action<UserInputEvent> inputEventHandler)
        {
            _inputEventHandler = inputEventHandler;

            _gamepadManager = new();
            _gamepadManager.GamepadConnected += (s, e) =>
            {
                ControllerInfo = $"{(e.IsWireless ? "Wireless" : "Wired")} controller connected on slot {e.Id + 1}";
                Task.Run(async () =>
                {
                    await Task.Delay(4000);
                    ShowControllerConnectedInfo = false;
                });
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

            var action = buttonEvent.State == GamepadDigitalInputState.Pressed ? ButtonAction.Down :
                (buttonEvent.State == GamepadDigitalInputState.Released ? ButtonAction.Up : ButtonAction.None);

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
                LastControllerConnectedWasDisconnected = true;
                ControllerInfo = $"{(gamepad.IsWireless ? "Wireless" : "Wired")} controller on slot {gamepad.Id + 1} disconnected";
                ShowControllerConnectedInfo = true;
                Task.Run(async () =>
                {
                    await Task.Delay(4000);
                    ShowControllerConnectedInfo = false;
                });
            }
        }

        private void NewBatteryLevelReceived(object? sender, GamepadBatteryLevel e)
        {
            // TODO: Implement
        }
    }
}
