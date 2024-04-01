using System;
using CommunityToolkit.Mvvm.ComponentModel;
using PokerTracker3000.Interfaces;

using InputEvent = PokerTracker3000.Input.UserInputEvent;

namespace PokerTracker3000.WpfComponents.EditGameOptions
{
    public class OptionModel : ObservableObject, IInputRelay
    {
        #region Public properties

        #region Private fields
        private bool _isSelected = false;
        private bool _isHighlighted = false;
        #endregion

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public bool IsHighlighted
        {
            get => _isHighlighted;
            set => SetProperty(ref _isHighlighted, value);
        }

        public string Text { get; init; } = string.Empty;

        public event EventHandler<InputEvent.NavigationDirection>? Navigate;
        public event EventHandler<IInputRelay.ButtonEventArgs>? ButtonEvent;

        public void FireNavigationEvent(InputEvent.NavigationDirection direction)
            => Navigate?.Invoke(this, direction);
        #endregion
    }
}
