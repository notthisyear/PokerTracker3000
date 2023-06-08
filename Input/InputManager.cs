using System;
using System.Windows.Input;

namespace PokerTracker3000.Input
{
    public class InputManager
    {
        public event EventHandler? StartButtonPressed;

        public void HandleKeyPressed(KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                StartButtonPressed?.Invoke(this, EventArgs.Empty);
        }
    }
}
