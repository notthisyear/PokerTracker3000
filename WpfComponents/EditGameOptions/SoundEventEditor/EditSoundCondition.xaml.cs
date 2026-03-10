using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using PokerTracker3000.Common;
using PokerTracker3000.GameSession;
using PokerTracker3000.GameSession.Sound;
using PokerTracker3000.Interfaces;

using ButtonEventArgs = PokerTracker3000.Interfaces.IInputRelay.ButtonEventArgs;
using InputEvent = PokerTracker3000.Input.UserInputEvent;
using NavigationEventArgs = PokerTracker3000.Interfaces.IInputRelay.NavigationEventArgs;
using SoundCondition = PokerTracker3000.GameSession.Sound.Condition;

namespace PokerTracker3000.WpfComponents.EditGameOptions
{
    public partial class EditSoundCondition : ConditionAndEffectEditorBase
    {
        #region Dependency properties
        public SoundCondition Condition
        {
            get { return (SoundCondition)GetValue(ConditionProperty); }
            set { SetValue(ConditionProperty, value); }
        }
        public static readonly DependencyProperty ConditionProperty = DependencyProperty.Register(
            nameof(Condition),
            typeof(SoundCondition),
            typeof(EditSoundCondition),
            new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.AffectsRender));

        public string CurrentValue
        {
            get { return (string)GetValue(CurrentValueProperty); }
            set { SetValue(CurrentValueProperty, value); }
        }
        public static readonly DependencyProperty CurrentValueProperty = DependencyProperty.Register(
            nameof(CurrentValue),
            typeof(string),
            typeof(EditSoundCondition),
            new FrameworkPropertyMetadata(string.Empty,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault |
                FrameworkPropertyMetadataOptions.AffectsRender, CurrentValueChangedCallback));

        private static void CurrentValueChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is EditSoundCondition c)
                c.ValidateCurrentSettings();
        }

        public int SelectedInputColumn
        {
            get { return (int)GetValue(SelectedInputColumnProperty); }
            set { SetValue(SelectedInputColumnProperty, value); }
        }
        public static readonly DependencyProperty SelectedInputColumnProperty = DependencyProperty.Register(
            nameof(SelectedInputColumn),
            typeof(int),
            typeof(EditSoundCondition),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));

        #region Read-only dependency properties
        public bool IsGameEventType
        {
            get => (bool)GetValue(s_isGameEventTypeProperty);
            private set => SetValue(s_isGameEventTypePropertyKey, value);
        }
        private static readonly DependencyPropertyKey s_isGameEventTypePropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(IsGameEventType),
            typeof(bool),
            typeof(EditSoundCondition),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));
        private static readonly DependencyProperty s_isGameEventTypeProperty = s_isGameEventTypePropertyKey.DependencyProperty;

        public bool IsValid
        {
            get => (bool)GetValue(s_isValidProperty);
            private set => SetValue(s_isValidPropertyKey, value);
        }
        private static readonly DependencyPropertyKey s_isValidPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(IsValid),
            typeof(bool),
            typeof(EditSoundCondition),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));
        private static readonly DependencyProperty s_isValidProperty = s_isValidPropertyKey.DependencyProperty;

        public string ValidationInfo
        {
            get => (string)GetValue(s_validationInfoProperty);
            private set => SetValue(s_validationInfoPropertyKey, value);
        }
        private static readonly DependencyPropertyKey s_validationInfoPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(ValidationInfo),
            typeof(string),
            typeof(EditSoundCondition),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsRender));
        private static readonly DependencyProperty s_validationInfoProperty = s_validationInfoPropertyKey.DependencyProperty;
        #endregion

        #endregion

        #region Public properties
        public ButtonOptionModel RemoveConditionModel { get; } = new("Remove", type: ButtonOptionModel.OptionType.Cancel);

        public ObservableCollection<string> AvailableConditionVariables { get; } = [];

        public ObservableCollection<string> AvailableConditionCheckTypes { get; } = [];

        public ObservableCollection<string> AvailableGameEventTypes { get; } = [];

        public NavigationOnlyRelay ConditionVariableNavigator { get; } = new();

        public NavigationOnlyRelay ConditionCheckNavigator { get; } = new();

        public NavigationOnlyRelay GameEventNavigator { get; } = new();

        public ConditionVariable SelectedConditionVariable { get; private set; }

        public ConditionCheckType SelectedConditionCheckType { get; private set; }

        public GameEventBus.EventType SelectedGameEventType { get; private set; }
        #endregion

        #region Private fields
        private readonly List<ConditionVariable> _conditionVariables = [];
        private readonly List<ConditionCheckType> _conditionCheckTypes = [];
        private readonly List<GameEventBus.EventType> _gameEventTypes = [];

        private readonly Lock _initLock = new();
        private bool _isLoaded = false;
        private int _navigationId;
        // Note: During Unloaded, the dependency property that hold the NavigationManager is no longer accessible
        private NavigationManager? _cachedNavigationManager;
        #endregion

        public EditSoundCondition()
        {
            InitializeComponent();
            Loaded += ControlLoaded;

            conditionVariableScroller.Loaded += ConditionVariableScrollerLoaded;
            conditionCheckScroller.Loaded += ConditionCheckScrollerLoaded;
            gameEventScroller.Loaded += GameEventScrollerLoaded;
        }

        private void ConditionVariableScrollerLoaded(object sender, RoutedEventArgs e)
        {
            conditionVariableScroller.Loaded -= ConditionVariableScrollerLoaded;

            VerifyIsLoaded();

            while (_conditionVariables[conditionVariableScroller.CurrentSelectedIndex] != Condition.ConditionVariable)
                ConditionVariableNavigator.RaiseEvent(InputEvent.NavigationDirection.Down);

            conditionVariableScroller.SelectedIndexChanged += (s, e) =>
            {
                SelectedConditionVariable = _conditionVariables[conditionVariableScroller.CurrentSelectedIndex];
                var wasGameEventType = IsGameEventType;
                IsGameEventType = SelectedConditionVariable == ConditionVariable.GameEvent;

                if (wasGameEventType != IsGameEventType)
                    NavigationManager.ReplaceNavigation(_navigationId, GetNavigationNodes());

                ValidateCurrentSettings();
            };
        }

        private void ConditionCheckScrollerLoaded(object sender, RoutedEventArgs e)
        {
            conditionCheckScroller.Loaded -= ConditionCheckScrollerLoaded;

            VerifyIsLoaded();

            while (_conditionCheckTypes[conditionCheckScroller.CurrentSelectedIndex] != Condition.ConditionCheckType)
                ConditionCheckNavigator.RaiseEvent(InputEvent.NavigationDirection.Down);

            conditionCheckScroller.SelectedIndexChanged += (s, e) =>
            {
                SelectedConditionCheckType = _conditionCheckTypes[conditionCheckScroller.CurrentSelectedIndex];
                ValidateCurrentSettings();
            };
        }

        private void GameEventScrollerLoaded(object sender, RoutedEventArgs e)
        {
            gameEventScroller.Loaded -= GameEventScrollerLoaded;

            VerifyIsLoaded();

            if (Condition is EventCondition condition)
            {
                SelectedGameEventType = condition.Value;
                IsGameEventType = true;
                while (_gameEventTypes[gameEventScroller.CurrentSelectedIndex] != SelectedGameEventType)
                    GameEventNavigator.RaiseEvent(InputEvent.NavigationDirection.Down);
            }

            gameEventScroller.SelectedIndexChanged += (s, e) =>
            {
                SelectedGameEventType = _gameEventTypes[gameEventScroller.CurrentSelectedIndex];
                ValidateCurrentSettings();
            };
        }

        private void ControlLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= ControlLoaded;
            lock (_initLock)
                _isLoaded = true;

            if (NavigationRelay == default ||
                NavigationManager == default ||
                Condition == default ||
                Condition.ConditionVariable == ConditionVariable.None ||
                Condition.ConditionCheckType == ConditionCheckType.None)
            {
                return;
            }

            Unloaded += ControlUnloaded;

            PopulateOptionsList(AvailableConditionVariables, _conditionVariables);
            PopulateOptionsList(AvailableConditionCheckTypes, _conditionCheckTypes);
            PopulateOptionsList(AvailableGameEventTypes, _gameEventTypes);

            SelectedConditionVariable = Condition.ConditionVariable;
            SelectedConditionCheckType = Condition.ConditionCheckType;
            CurrentValue = Condition.ConditionValue;
            IsGameEventType = SelectedConditionVariable == ConditionVariable.GameEvent;

            ValidateCurrentSettings(false);

            _cachedNavigationManager = NavigationManager;
            _navigationId = NavigationManager.RegisterNavigation(GetNavigationNodes());

            WeakEventManager<IInputRelay, NavigationEventArgs>.
                AddHandler(NavigationRelay, nameof(NavigationRelay.Navigate), HandleNavigate);

            WeakEventManager<IInputRelay, ButtonEventArgs>.
                AddHandler(NavigationRelay, nameof(NavigationRelay.ButtonEvent), HandleButtonPress);
        }

        private void ValidateCurrentSettings(bool fireEventIfValid = true)
        {
            var (isValid, validationInfo) = SoundCondition.IsValid(SelectedConditionVariable, SelectedConditionCheckType, CurrentValue);
            IsValid = isValid;
            ValidationInfo = validationInfo.ToLower();
            if (isValid && fireEventIfValid)
                RaiseNewValidContentEvent();
        }

        private void VerifyIsLoaded()
        {
            bool loaded;
            lock (_initLock)
                loaded = _isLoaded;

            if (!loaded)
                throw new InvalidOperationException("EditSoundCondition not loaded");
        }

        private void HandleNavigate(object? sender, NavigationEventArgs e)
        {
            if (!IsActive || NavigationManager == default)
                return;

            var isLeftOrRight = (e.Direction == InputEvent.NavigationDirection.Left) || (e.Direction == InputEvent.NavigationDirection.Right);
            if (isLeftOrRight || RemoveConditionModel.IsSelected)
            {
                SelectedInputColumn = NavigationManager.Navigate(_navigationId, SelectedInputColumn, e.Direction);

                // Mark the button as either selected or not
                RemoveConditionModel.IsSelected = IsGameEventType ? (SelectedInputColumn == 3) : (SelectedInputColumn == 2);
            }
            else
            {
                // Forward the navigation event to the appropiate scroller
                if (SelectedInputColumn == 0)
                    ConditionVariableNavigator.RaiseEvent(e.Direction);
                else if (SelectedInputColumn == 1)
                    ConditionCheckNavigator.RaiseEvent(e.Direction);
                else if ((SelectedInputColumn == 2) && IsGameEventType)
                    GameEventNavigator.RaiseEvent(e.Direction);
            }

            e.Handled = true;
        }

        private void HandleButtonPress(object? sender, ButtonEventArgs e)
        {
            if (e.ButtonEvent == InputEvent.ButtonEventType.Select && RemoveConditionModel.IsSelected)
            {
                RemoveConditionModel.IsSelected = false;
                RaiseRemoveEvent();
                e.Handled = true;
            }
        }

        private NavigationManager.Node[] GetNavigationNodes()
        {
            // Two or three scrollers, and two buttons
            var numberOfScrollers = IsGameEventType ? 3 : 2;
            var navigationNodes = new NavigationManager.Node[numberOfScrollers + 1];

            for (var i = 0; i < numberOfScrollers; i++)
                navigationNodes[i] = new(i, 0);

            // Remove sound condition button
            navigationNodes[numberOfScrollers] = new(numberOfScrollers, 0);
            return navigationNodes;
        }

        private void ControlUnloaded(object sender, RoutedEventArgs e)
        {
            Unloaded -= ControlUnloaded;
            _cachedNavigationManager?.RemoveNavigation(_navigationId);
        }
    }
}
