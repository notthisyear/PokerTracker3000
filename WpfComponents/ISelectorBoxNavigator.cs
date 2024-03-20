using System;

using InputEvent = PokerTracker3000.Input.InputManager.UserInputEvent;

namespace PokerTracker3000.WpfComponents
{
    public interface ISelectorBoxNavigator
    {
        public event EventHandler<InputEvent.NavigationDirection>? Navigate;
    }
}
