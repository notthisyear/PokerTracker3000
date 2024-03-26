using System;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using PokerTracker3000.Interfaces;

using InputEvent = PokerTracker3000.Input.InputManager.UserInputEvent;

namespace PokerTracker3000.WpfComponents
{
    public class ScrollDigit : ObservableObject, IInputRelay
    {
        #region Public properties

        #region Private fields
        private int _value = 0;
        private bool _isSelected = false;
        #endregion

        public int Value { get => _value; set => SetProperty(ref _value, value); }

        public bool IsSelected { get => _isSelected; set => SetProperty(ref _isSelected, value); }

        public int Index { get; init; }

        public bool IsHookedToScrollBox => _box != default;
        #endregion

        #region Events
        public event EventHandler<InputEvent.NavigationDirection>? Navigate;
        public event EventHandler<IInputRelay.ButtonEventArgs>? ButtonEvent;
        #endregion

        private ScrollingSelectorBox? _box = default;

        public void HookToScrollingSelectorBox(ScrollingSelectorBox box)
        {
            box.SelectedIndexChanged += BoxSelectedIndexChanged;
            _box = box;
        }

        #region Public methods
        public void FireNavigationEvent(InputEvent.NavigationDirection direction)
            => Navigate?.Invoke(this, direction);

        public void SyncScrollerToValue(ScrollingSelectorBox box)
        {
            while ((box.CurrentSelectedIndex - Value) > 0)
                Navigate?.Invoke(this, InputEvent.NavigationDirection.Up);

            while ((box.CurrentSelectedIndex - Value) < 0)
                Navigate?.Invoke(this, InputEvent.NavigationDirection.Down);
        }

        public void UnhookToScrollingSelectorBox()
        {
            if (_box != default)
                _box.SelectedIndexChanged -= BoxSelectedIndexChanged;
        }
        #endregion

        private void BoxSelectedIndexChanged(object sender, RoutedEventArgs e)
        {
            if (e is SelectedIndexChangedEventArgs args)
                Value = args.NewIndex;
        }
    }
}
