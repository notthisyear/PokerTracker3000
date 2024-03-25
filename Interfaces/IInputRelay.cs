using System;

using InputEvent = PokerTracker3000.Input.InputManager.UserInputEvent;

namespace PokerTracker3000.Interfaces
{
    public interface IInputRelay
    {
        public event EventHandler<InputEvent.NavigationDirection>? Navigate;

        public event EventHandler<InputEvent.ButtonEventType>? ButtonEvent;
    }
}
