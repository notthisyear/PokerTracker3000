using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using PokerTracker3000.Common;
using PokerTracker3000.GameSession;
using PokerTracker3000.GameSession.Sound;

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
            new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.AffectsRender, ConditionChanged));

        private static void ConditionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is EditSoundCondition control && control.IsLoaded && e.NewValue is SoundCondition c)
                control.ReloadSettings(c);
        }

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

        #region Read-only dependency properties
        public int SelectedInputColumn
        {
            get => (int)GetValue(s_selectedInputColumnProperty);
            private set => SetValue(s_selectedInputColumnPropertyKey, value);
        }
        private static readonly DependencyPropertyKey s_selectedInputColumnPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(SelectedInputColumn),
            typeof(int),
            typeof(EditSoundCondition),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));
        private static readonly DependencyProperty s_selectedInputColumnProperty = s_selectedInputColumnPropertyKey.DependencyProperty;

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

        public bool ShowConditionValueEditField
        {
            get => (bool)GetValue(s_showConditionValueEditFieldProperty);
            private set => SetValue(s_showConditionValueEditFieldPropertyKey, value);
        }
        private static readonly DependencyPropertyKey s_showConditionValueEditFieldPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(ShowConditionValueEditField),
            typeof(bool),
            typeof(EditSoundCondition),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));
        private static readonly DependencyProperty s_showConditionValueEditFieldProperty = s_showConditionValueEditFieldPropertyKey.DependencyProperty;

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
        public ObservableCollection<string> AvailableConditionVariables { get; } = [];

        public ObservableCollection<string> AvailableConditionCheckTypes { get; } = [];

        public ObservableCollection<string> AvailableGameEventTypes { get; } = [];

        public NavigationOnlyRelay ConditionVariableNavigator { get; } = new();

        public NavigationOnlyRelay ConditionCheckNavigator { get; } = new();

        public NavigationOnlyRelay GameEventNavigator { get; } = new();

        public ConditionVariable SelectedConditionVariable { get; private set; }

        public ConditionCheckType SelectedConditionCheckType { get; private set; }

        public GameEventBus.EventType SelectedGameEventType { get; private set; }

        public ButtonOptionModel ConditionValueButton { get; } = new(string.Empty, type: ButtonOptionModel.OptionType.Info);
        #endregion

        #region Private fields
        private readonly List<ConditionVariable> _conditionVariables = [];
        private readonly List<ConditionCheckType> _conditionCheckTypes = [];
        private readonly List<GameEventBus.EventType> _gameEventTypes = [];

        private readonly Lock _initLock = new();
        private bool _isLoaded = false;
        #endregion

        public EditSoundCondition()
        {
            InitializeComponent();
            Loaded += ControlLoaded;

            conditionVariableScroller.Loaded += ConditionVariableScrollerLoaded;
            conditionCheckScroller.Loaded += ConditionCheckScrollerLoaded;
            gameEventScroller.Loaded += GameEventScrollerLoaded;
        }

        #region Protected methods
        protected override void HandleNavigation(object? sender, NavigationEventArgs e)
        {
            if (!IsActive || NavigationManager == default)
                return;

            var isLeftOrRight = (e.Direction == InputEvent.NavigationDirection.Left) || (e.Direction == InputEvent.NavigationDirection.Right);
            if (isLeftOrRight || ConditionValueButton.IsSelected || RemoveButtonModel.IsSelected)
            {
                SelectedInputColumn = NavigationManager.Navigate(NavigationId, SelectedInputColumn, e.Direction);

                // Mark the buttons as either selected or not
                ConditionValueButton.IsSelected = !IsGameEventType && SelectedInputColumn == 2;
                RemoveButtonModel.IsSelected = SelectedInputColumn == 3;
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

        protected override void HandleButton(object? sender, ButtonEventArgs e)
        {
            if (!IsActive || !IsLoaded)
                return;

            if (RemoveButtonModel.IsSelected && e.ButtonEvent == InputEvent.ButtonEventType.Select)
            {
                RemoveButtonModel.IsSelected = false;
                RemoveButtonModel.ButtonAction?.Invoke();
                e.Handled = true;
            }
            else if (ConditionValueButton.IsSelected)
            {
                if (e.ButtonEvent == InputEvent.ButtonEventType.Select)
                {
                    ShowConditionValueEditField = !ShowConditionValueEditField;
                    if (ShowConditionValueEditField)
                    {
                        CurrentValue = Condition.ConditionValue;
                        conditionValueBox.Focus();
                        conditionValueBox.CaretIndex = CurrentValue.Length;
                    }
                    e.Handled = true;
                }
                else if (e.ButtonEvent == InputEvent.ButtonEventType.GoBack && ShowConditionValueEditField)
                {
                    ShowConditionValueEditField = false;
                    // Note: This is for the case where a validation error causes up to not update the condition, 
                    //       hence not retriggering the condition so that CurrentValue gets updated in ReloadSettings
                    CurrentValue = Condition.ConditionValue;
                    e.Handled = true;
                }
            }
        }

        protected override NavigationManager.Node[] GetNavigationNodes()
        {
            // Two or three scrollers, and two or one button
            var numberOfElements = 4;
            var navigationNodes = new NavigationManager.Node[numberOfElements];

            for (var i = 0; i < numberOfElements; i++)
                navigationNodes[i] = new(i, 0);

            return navigationNodes;
        }
        #endregion

        #region Loaded methods
        private void ConditionVariableScrollerLoaded(object sender, RoutedEventArgs e)
        {
            conditionVariableScroller.Loaded -= ConditionVariableScrollerLoaded;

            VerifyIsLoaded();

            while (_conditionVariables[conditionVariableScroller.CurrentSelectedIndex] != Condition.ConditionVariable)
                ConditionVariableNavigator.RaiseEvent(InputEvent.NavigationDirection.Down);

            conditionVariableScroller.SelectedIndexChanged += ConditionVariableScrollerSelectedIndexChanged;
        }

        private void ConditionCheckScrollerLoaded(object sender, RoutedEventArgs e)
        {
            conditionCheckScroller.Loaded -= ConditionCheckScrollerLoaded;

            VerifyIsLoaded();

            while (_conditionCheckTypes[conditionCheckScroller.CurrentSelectedIndex] != Condition.ConditionCheckType)
                ConditionCheckNavigator.RaiseEvent(InputEvent.NavigationDirection.Down);

            conditionCheckScroller.SelectedIndexChanged += ConditionCheckScrollerSelectedIndexChanged;
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

            gameEventScroller.SelectedIndexChanged += GameEventScrollerSelectedIndexChanged;
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

            PopulateOptionsList(AvailableConditionVariables, _conditionVariables);
            PopulateOptionsList(AvailableConditionCheckTypes, _conditionCheckTypes);
            PopulateOptionsList(AvailableGameEventTypes, _gameEventTypes);

            SelectedConditionVariable = Condition.ConditionVariable;
            SelectedConditionCheckType = Condition.ConditionCheckType;
            CurrentValue = Condition.ConditionValue;
            ConditionValueButton.Name = CurrentValue;
            IsGameEventType = SelectedConditionVariable == ConditionVariable.GameEvent;

            ControlLoadedBase();

            ValidateCurrentSettings(false);
        }
        #endregion

        private void ReloadSettings(SoundCondition condition)
        {
            conditionVariableScroller.SelectedIndexChanged -= ConditionVariableScrollerSelectedIndexChanged;
            conditionCheckScroller.SelectedIndexChanged -= ConditionCheckScrollerSelectedIndexChanged;
            gameEventScroller.SelectedIndexChanged -= GameEventScrollerSelectedIndexChanged;

            while (_conditionVariables[conditionVariableScroller.CurrentSelectedIndex] != condition.ConditionVariable)
                ConditionVariableNavigator.RaiseEvent(InputEvent.NavigationDirection.Down);

            while (_conditionCheckTypes[conditionCheckScroller.CurrentSelectedIndex] != condition.ConditionCheckType)
                ConditionCheckNavigator.RaiseEvent(InputEvent.NavigationDirection.Down);

            if (condition is EventCondition eventCondition)
            {
                SelectedGameEventType = eventCondition.Value;
                IsGameEventType = true;
                while (_gameEventTypes[gameEventScroller.CurrentSelectedIndex] != SelectedGameEventType)
                    GameEventNavigator.RaiseEvent(InputEvent.NavigationDirection.Down);
            }
            else
            {
                IsGameEventType = false;
            }

            SelectedConditionVariable = condition.ConditionVariable;
            SelectedConditionCheckType = condition.ConditionCheckType;
            CurrentValue = condition.ConditionValue;
            ConditionValueButton.Name = CurrentValue;
            IsGameEventType = SelectedConditionVariable == ConditionVariable.GameEvent;

            ValidateCurrentSettings(false);

            conditionVariableScroller.SelectedIndexChanged += ConditionVariableScrollerSelectedIndexChanged;
            conditionCheckScroller.SelectedIndexChanged += ConditionCheckScrollerSelectedIndexChanged;
            gameEventScroller.SelectedIndexChanged += GameEventScrollerSelectedIndexChanged;
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

        #region Scroller selected changed callbacks
        private void ConditionVariableScrollerSelectedIndexChanged(object sender, RoutedEventArgs e)
        {
            SelectedConditionVariable = _conditionVariables[conditionVariableScroller.CurrentSelectedIndex];
            IsGameEventType = SelectedConditionVariable == ConditionVariable.GameEvent;

            ConditionValueButton.Name = Condition.ConditionValue;

            ValidateCurrentSettings();
        }

        private void ConditionCheckScrollerSelectedIndexChanged(object sender, RoutedEventArgs e)
        {
            SelectedConditionCheckType = _conditionCheckTypes[conditionCheckScroller.CurrentSelectedIndex];
            ValidateCurrentSettings();
        }

        private void GameEventScrollerSelectedIndexChanged(object sender, RoutedEventArgs e)
        {
            SelectedGameEventType = _gameEventTypes[gameEventScroller.CurrentSelectedIndex];
            ValidateCurrentSettings();
        }
        #endregion
    }
}
