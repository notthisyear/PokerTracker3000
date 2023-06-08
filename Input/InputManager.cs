using System.Windows.Input;

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

            public enum ButtonPressed
            {
                None,
                Start,
                Select,
                Accept,
                GoBack
            }

            public bool IsButtonPressed { get; init; } = false;

            public bool IsNavigation { get; init; } = false;

            public ButtonPressed Button { get; init; } = ButtonPressed.None;

            public NavigationDirection Direction { get; init; } = NavigationDirection.None;

            public bool IsEmpty => IsButtonPressed == false && IsNavigation == false;

            public static UserInputEvent GetNavigationEvent(NavigationDirection direction)
                => new()
                {
                    IsNavigation = true,
                    Direction = direction
                };

            public static UserInputEvent GetStartButtonEvent()
                => new()
                {
                    IsButtonPressed = true,
                    Button = ButtonPressed.Start
                };

            public static UserInputEvent GetEmptyEvent()
                => new();
        }

        public static UserInputEvent GetUserInputEventFromKeyboard(KeyEventArgs e)
            => e.Key switch
            {
                Key.Enter => UserInputEvent.GetStartButtonEvent(),
                Key.Left => UserInputEvent.GetNavigationEvent(UserInputEvent.NavigationDirection.Left),
                Key.Right => UserInputEvent.GetNavigationEvent(UserInputEvent.NavigationDirection.Right),
                Key.Up => UserInputEvent.GetNavigationEvent(UserInputEvent.NavigationDirection.Up),
                Key.Down => UserInputEvent.GetNavigationEvent(UserInputEvent.NavigationDirection.Down),
                _ => UserInputEvent.GetEmptyEvent()
            };
    }
}
