using System.Collections.Generic;
using System.Windows.Input;
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

            public bool IsEmpty => IsButtonEvent == false && IsNavigationEvent == false;

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

            internal static UserInputEvent GetEmptyEvent()
                => new();
        }

        private readonly Dictionary<Key, Dictionary<ButtonAction, UserInputEvent>> _keyboardEvents = [];

        public void RegisterKeyboardEvent(Key keyboardKey, NavigationDirection direction, ButtonAction action = ButtonAction.Down, bool allowRepeat = false)
        {
            AddKeyboardEvent(keyboardKey, GetNavigationEvent(direction, action, allowRepeat));
        }

        public void RegisterKeyboardEvent(Key keyboardKey, ButtonEventType button, ButtonAction action = ButtonAction.Down, bool allowRepeat = false)
        {
            AddKeyboardEvent(keyboardKey, GetButtonEvent(button, action, allowRepeat));
        }

        public UserInputEvent GetUserInputEventFromKeyboard(KeyEventArgs keyEvent)
        {
            if (!_keyboardEvents.TryGetValue(keyEvent.Key, out var inputEvents))
                return GetEmptyEvent();

            var action = keyEvent.IsDown ? ButtonAction.Down : (keyEvent.IsUp ? ButtonAction.Up : ButtonAction.None);
            if (inputEvents.TryGetValue(action, out var e) && e.Matches(keyEvent))
                return e;

            return GetEmptyEvent();
        }

        private void AddKeyboardEvent(Key keyboardKey, UserInputEvent newEvent)
        {
            if (!_keyboardEvents.ContainsKey(keyboardKey))
                _keyboardEvents.Add(keyboardKey, []);

            _ = _keyboardEvents[keyboardKey].TryAdd(newEvent.Action, newEvent);
        }
    }
}
