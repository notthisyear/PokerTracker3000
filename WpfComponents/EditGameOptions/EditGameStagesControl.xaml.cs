using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using PokerTracker3000.GameSession;
using PokerTracker3000.Interfaces;

using InputEvent = PokerTracker3000.Input.InputManager.UserInputEvent;

namespace PokerTracker3000.WpfComponents.EditGameOptions
{
    public partial class EditGameStagesControl : UserControl, IInputRelay
    {
        #region Dependency properties
        public GameSessionManager SessionManager
        {
            get { return (GameSessionManager)GetValue(SessionManagerProperty); }
            set { SetValue(SessionManagerProperty, value); }
        }
        public static readonly DependencyProperty SessionManagerProperty = DependencyProperty.Register(
            nameof(SessionManager),
            typeof(GameSessionManager),
            typeof(EditGameStagesControl),
            new FrameworkPropertyMetadata(default));

        public GameStage SelectedStage
        {
            get => (GameStage)GetValue(s_selectedStageProperty);
            private set => SetValue(s_selectedStagePropertyKey, value);
        }
        private static readonly DependencyPropertyKey s_selectedStagePropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(SelectedStage),
            typeof(GameStage),
            typeof(EditGameStagesControl),
            new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.AffectsRender));
        private static readonly DependencyProperty s_selectedStageProperty = s_selectedStagePropertyKey.DependencyProperty;
        #endregion

        #region Public properties
        public OptionModel PauseModel { get; } = new() { Text = "Break stage" };

        public OptionModel SmallBlindModel { get; } = new() { Text = "Small blind" };

        public OptionModel BigBlindModel { get; } = new() { Text = "Big blind" };

        public OptionModel StageLengthModel { get; } = new() { Text = "Stage length" };

        public PlayerEditOption AddStageModel { get; } = new(PlayerEditOption.EditOption.AddStage, PlayerEditOption.OptionType.Success);

        public PlayerEditOption RemoveStageModel { get; } = new(PlayerEditOption.EditOption.RemoveStage, PlayerEditOption.OptionType.Cancel);
        #endregion

        #region Events
        public event EventHandler<InputEvent.NavigationDirection>? Navigate;
        public event EventHandler<IInputRelay.ButtonEventArgs>? ButtonEvent;
        #endregion

        #region Private fields
        private int _navigationId;
        private int _selectedElementIndex;
        private readonly Dictionary<int,
            (Func<InputEvent.NavigationDirection, bool> navigateAction,
            Action<IInputRelay.ButtonEventArgs> buttonPressAction)> _actionMap = [];
        #endregion

        public EditGameStagesControl()
        {
            InitializeComponent();
            Loaded += ControlLoadedEvent;
        }

        private void ControlLoadedEvent(object sender, RoutedEventArgs e)
        {
            Loaded -= ControlLoadedEvent;
            if (SessionManager == default)
                return;

            _navigationId = SessionManager.NavigationManager.RegisterNavigation(
                [
                new(0, 0),
                new(0, 1),
                new(0, 2),
                new(0, 3),
                new(0, 4), // Add stage button
                new(1, 0), // Pause option
                new(1, 1), // Stage length option
                new(1, 2), // Small blind option
                new(1, 3), // Big blind option
                new(1, 4), // Remove stage option
                ]);

            _actionMap.Add(0, (e =>
            {
                Navigate?.Invoke(sender, e);
                return true;
            },
            e => { }
            ));
            _actionMap.Add(4, (_ =>
                {
                    AddStageModel.IsSelected = !AddStageModel.IsSelected;
                    return true;
                },
                e =>
                {
                    if (e.ButtonEvent == InputEvent.ButtonEventType.Select)
                    {
                        SessionManager.AddStage();
                        Navigate?.Invoke(this, InputEvent.NavigationDirection.Down);
                        e.Handled = true;
                    }
                }
            ));
            _actionMap.Add(5, (e =>
                {
                    var didNavigateAway = HandleCommonNavigationOnSelectableOption(PauseModel);
                    if (!didNavigateAway && (e == InputEvent.NavigationDirection.Left || e == InputEvent.NavigationDirection.Right))
                        SelectedStage.IsPause = !SelectedStage.IsPause;
                    return didNavigateAway;
                },
                e =>
                {
                    if (e.ButtonEvent == InputEvent.ButtonEventType.Select && PauseModel.IsSelected)
                        SelectedStage.IsPause = !SelectedStage.IsPause;
                    else
                        HandleCommonButtonPressOnSelectableOption(PauseModel, e);
                }
            ));
            _actionMap.Add(6, (_ => HandleCommonNavigationOnSelectableOption(StageLengthModel), _ => { }));
            _actionMap.Add(7, (e =>
                {
                    var didNavigateAway = HandleCommonNavigationOnSelectableOption(SmallBlindModel);
                    if (!didNavigateAway)
                        SmallBlindModel.FireNavigationEvent(e);
                    return didNavigateAway;
                },
                e =>
                {
                    if (e.ButtonEvent == InputEvent.ButtonEventType.Select && SmallBlindModel.IsSelected)
                        SmallBlindModel.IsSelected = false;
                    else
                        HandleCommonButtonPressOnSelectableOption(SmallBlindModel, e);
                }
            ));
            _actionMap.Add(8, (e =>
            {
                var didNavigateAway = HandleCommonNavigationOnSelectableOption(BigBlindModel);
                if (!didNavigateAway)
                    BigBlindModel.FireNavigationEvent(e);
                return didNavigateAway;
            },
                e =>
                {
                    if (e.ButtonEvent == InputEvent.ButtonEventType.Select && BigBlindModel.IsSelected)
                        BigBlindModel.IsSelected = false;
                    else
                        HandleCommonButtonPressOnSelectableOption(BigBlindModel, e);
                }
            ));
            _actionMap.Add(9, (_ =>
            {
                RemoveStageModel.IsSelected = !RemoveStageModel.IsSelected;
                return true;
            },
            _ => { }
            ));

            _selectedElementIndex = 5;
            _actionMap[_selectedElementIndex].navigateAction(InputEvent.NavigationDirection.None);

            if (SessionManager.TryGetStage(0, out var stage))
                SelectedStage = stage!;

            SessionManager.Navigate += (s, e) =>
            {
                var isUpOrDown = e == InputEvent.NavigationDirection.Up || e == InputEvent.NavigationDirection.Down;
                if (SelectedIndexIsStageSelector() && isUpOrDown)
                {
                    _ = _actionMap[0].navigateAction.Invoke(e);
                }
                else
                {
                    if (!_actionMap[SelectedIndexIsStageSelector() ? 0 : _selectedElementIndex].navigateAction.Invoke(e))
                        return;

                    var newIdx = SessionManager.NavigationManager.Navigate(_navigationId, _selectedElementIndex, e);
                    _selectedElementIndex = newIdx;

                    _ = _actionMap[SelectedIndexIsStageSelector() ? 0 : _selectedElementIndex].navigateAction.Invoke(e);
                }
            };
            SessionManager.ButtonEvent += (s, e) => _actionMap[_selectedElementIndex].buttonPressAction.Invoke(e);
        }

        private bool SelectedIndexIsStageSelector()
            => _selectedElementIndex < 4;

        private void StageSelectorSelectedIndexChanged(object sender, RoutedEventArgs e)
        {
            if (e is not SelectedIndexChangedEventArgs args)
                return;

            if (SessionManager.TryGetStage(args.NewIndex, out var stage))
                SelectedStage = stage!;
        }

        private static bool HandleCommonNavigationOnSelectableOption(OptionModel option)
        {
            if (option.IsSelected)
                return false;

            option.IsHighlighted = !option.IsHighlighted;
            return true;
        }

        private static void HandleCommonButtonPressOnSelectableOption(OptionModel option, IInputRelay.ButtonEventArgs e)
        {
            if (e.ButtonEvent == InputEvent.ButtonEventType.Select)
            {
                option.IsSelected = true;
            }
            else if (option.IsSelected && e.ButtonEvent == InputEvent.ButtonEventType.GoBack)
            {
                option.IsSelected = false;
                e.Handled = true;
            }
        }
    }
}
