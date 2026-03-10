using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using PokerTracker3000.GameSession;
using PokerTracker3000.GameSession.Sound;
using static PokerTracker3000.Interfaces.IInputRelay;
using InputEvent = PokerTracker3000.Input.UserInputEvent;

namespace PokerTracker3000.WpfComponents.EditGameOptions
{
    public partial class BuiltInSoundEffectEditor : SoundEffectEditorBase
    {
        #region Dependency properties
        public BuiltInSoundEffect SoundEffect
        {
            get => (BuiltInSoundEffect)GetValue(SoundEffectProperty);
            set => SetValue(SoundEffectProperty, value);
        }
        public static readonly DependencyProperty SoundEffectProperty = DependencyProperty.Register(
            nameof(SoundEffect),
            typeof(BuiltInSoundEffect),
            typeof(BuiltInSoundEffectEditor),
            new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.AffectsRender));
        #endregion

        #region Public properties
        public ObservableCollection<string> AvailableBuiltInSoundEffectTypes { get; } = [];

        public BuiltInSoundEffectType SelectedBuiltInSoundEffect { get; private set; }
        #endregion

        #region Private fields
        private readonly List<BuiltInSoundEffectType> _builtInSoundEffectTypes = [];
        #endregion

        public BuiltInSoundEffectEditor()
        {
            InitializeComponent();
            Loaded += ControlLoaded;
        }

        private void ControlLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= ControlLoaded;

            if (NavigationRelay == default || SoundEffect == default)
                return;

            ControlLoadedBase();

            PopulateOptionsList(AvailableBuiltInSoundEffectTypes, _builtInSoundEffectTypes);

            while (_builtInSoundEffectTypes[builtInSoundEffectScroller.CurrentSelectedIndex] != SoundEffect.BuiltInEffect)
                ScrollerNavRelay.RaiseEvent(InputEvent.NavigationDirection.Down);

            builtInSoundEffectScroller.SelectedIndexChanged += (s, e) =>
            {
                SelectedBuiltInSoundEffect = _builtInSoundEffectTypes[builtInSoundEffectScroller.CurrentSelectedIndex];
            };
        }

        protected override void HandleNavigation(object? sender, NavigationEventArgs e)
        {
            if (!IsActive)
                return;

            if (!SelectedElementMap.TryGetValue(SelectedElementIndex, out var element))
                return;

            // A scroller was selected
            if (element is ScrollerSelectable)
            {
                var oldIndex = builtInSoundEffectScroller.CurrentSelectedIndex;
                ScrollerNavRelay.RaiseEvent(e.Direction);
                var changed = oldIndex != builtInSoundEffectScroller.CurrentSelectedIndex;
                if (changed)
                {
                    RaiseNewValidContentEvent();
                    e.Handled = true;
                    return;
                }
            }

            // Deselect the old element, if applicable
            element?.IsSelected = false;

            SelectedElementIndex = NavigationManager.Navigate(NavigationId, SelectedElementIndex, e.Direction);

            // If the new element index points to a selectable element
            if (SelectedElementMap.TryGetValue(SelectedElementIndex, out element))
                element?.IsSelected = true;
            e.Handled = true;
        }

        protected override void HandleButton(object? sender, ButtonEventArgs e)
        {
            if (!IsActive)
                return;

            // If a button was selected, invoke its action.
            if (e.ButtonEvent == InputEvent.ButtonEventType.Select &&
                SelectedElementMap.TryGetValue(SelectedElementIndex, out var element) &&
                element is ButtonOptionModel button)
            {
                button.ButtonAction?.Invoke();
                e.Handled = true;
            }
        }

        protected override NavigationManager.Node[] GetNavigationNodes()
        {
            // One scroller and two buttons
            var navigationNodes = new NavigationManager.Node[3];
            SelectedElementMap.Clear();

            // The scroller
            SelectedElementMap.Add(0, ScrollerSelectable);
            navigationNodes[0] = new(0, 0);

            // Add the two buttons
            SelectedElementMap.Add(1, TestEffectModel);
            navigationNodes[1] = new(0, 1);

            SelectedElementMap.Add(2, RemoveEffectModel);
            navigationNodes[2] = new(1, 1);

            return navigationNodes;
        }
    }
}
