using System;

using InputEvent = PokerTracker3000.Input.InputManager.UserInputEvent;

namespace PokerTracker3000.Interfaces
{
    public interface IInputRelay
    {
        public class ButtonEventArgs : EventArgs
        {
            public InputEvent.ButtonEventType ButtonEvent { get; init; }
            public bool Handled { get; set; } = false;
        };


        public event EventHandler<InputEvent.NavigationDirection>? Navigate;

        public event EventHandler<ButtonEventArgs>? ButtonEvent;
    }
}
