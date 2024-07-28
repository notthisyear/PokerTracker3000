using System.Windows.Input;
using DotXInput.GamepadInput;

namespace PokerTracker3000.Input
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
            InfoButton,
            SecondInfoButton
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
}
