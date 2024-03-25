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

        public PlayerEditOption AddStageModel { get; } = new(PlayerEditOption.EditOption.AddStage, PlayerEditOption.OptionType.Success);
        public PlayerEditOption RemoveStageModel { get; } = new(PlayerEditOption.EditOption.RemoveStage, PlayerEditOption.OptionType.Cancel);

        #region Events
        public event EventHandler<InputEvent.NavigationDirection>? Navigate;
        public event EventHandler<InputEvent.ButtonEventType>? ButtonEvent;
        #endregion

        #region Private fields
        private int _navigationId;
        private int _selectedElementIndex;
        private readonly Dictionary<int, FrameworkElement> _elementMap = [];
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

            _elementMap.Add(0, stageSelector);
            _elementMap.Add(1, stageSelector);
            _elementMap.Add(2, stageSelector);
            _elementMap.Add(3, stageSelector);
            _elementMap.Add(4, addStageButton);
            _elementMap.Add(5, pauseOption);
            _elementMap.Add(6, stageLengthOption);
            _elementMap.Add(7, smallBlindOption);
            _elementMap.Add(8, bigBlindOption);
            _elementMap.Add(9, removeStageButton);

            _selectedElementIndex = 5;
            pauseOption.IsEnabled = true;

            stageSelector.IsEnabled = false;
            smallBlindOption.IsEnabled = false;
            bigBlindOption.IsEnabled = false;
            stageLengthOption.IsEnabled = false;

            if (SessionManager.TryGetStage(0, out var stage))
                SelectedStage = stage!;

            SessionManager.Navigate += (s, e) =>
            {
                var isUpOrDown = e == InputEvent.NavigationDirection.Up || e == InputEvent.NavigationDirection.Down;
                if (_elementMap[_selectedElementIndex] == stageSelector && isUpOrDown)
                {
                    Navigate?.Invoke(s, e);
                }
                else
                {
                    var newIdx = SessionManager.NavigationManager.Navigate(_navigationId, _selectedElementIndex, e);
                    if (_elementMap[_selectedElementIndex] == addStageButton)
                        AddStageModel.IsSelected = false;
                    else if (_elementMap[_selectedElementIndex] == removeStageButton)
                        RemoveStageModel.IsSelected = false;
                    else
                        _elementMap[_selectedElementIndex].IsEnabled = false;

                    _selectedElementIndex = newIdx;

                    if (_elementMap[_selectedElementIndex] == addStageButton)
                        AddStageModel.IsSelected = true;
                    else if (_elementMap[_selectedElementIndex] == removeStageButton)
                        RemoveStageModel.IsSelected = true;
                    else
                        _elementMap[_selectedElementIndex].IsEnabled = true;

                }
            };
            SessionManager.ButtonEvent += (s, e) =>
            {
                if (e == InputEvent.ButtonEventType.Select && _elementMap[_selectedElementIndex] == addStageButton)
                {
                    SessionManager.AddStage();
                    Navigate?.Invoke(this, InputEvent.NavigationDirection.Down);
                }
            };
        }

        private void StageSelectorSelectedIndexChanged(object sender, RoutedEventArgs e)
        {
            if (e is not SelectedIndexChangedEventArgs args)
                return;

            if (SessionManager.TryGetStage(args.NewIndex, out var stage))
                SelectedStage = stage!;
        }
    }
}
