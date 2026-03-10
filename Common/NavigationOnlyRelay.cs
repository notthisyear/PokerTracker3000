using System;
using PokerTracker3000.Interfaces;
using static PokerTracker3000.Interfaces.IInputRelay;

using InputEvent = PokerTracker3000.Input.UserInputEvent;

namespace PokerTracker3000.Common
{
    public sealed class NavigationOnlyRelay : IInputRelay
    {
        public event EventHandler<NavigationEventArgs>? Navigate;
        public event EventHandler<ButtonEventArgs>? ButtonEvent { add { } remove { } }

        public void RaiseEvent(InputEvent.NavigationDirection direction)
            => Navigate?.Invoke(this, new() { Direction = direction });
    }
}
