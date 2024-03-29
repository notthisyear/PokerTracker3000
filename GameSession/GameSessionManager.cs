using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using PokerTracker3000.Common.FileUtilities;
using PokerTracker3000.Interfaces;

using InputEvent = PokerTracker3000.Input.InputManager.UserInputEvent;

namespace PokerTracker3000.GameSession
{
    public enum TableLayout
    {
        TwoPlayers,
        FourPlayers,
        SixPlayers,
        EightPlayers,
        TenPlayers,
        TwelvePlayers
    };

    public class GameSessionManager : ObservableObject, IInputRelay
    {
        #region Public properties

        #region Backing fields
        private PlayerSpot? _selectedSpot;
        private bool _tableFull = false;
        private decimal _totalAmountInPot = 0;
        private SideMenuViewModel.GameEditOption _currentGameEditOption = SideMenuViewModel.GameEditOption.None;
        #endregion

        public List<PlayerSpot> PlayerSpots { get; } = [];

        public List<PlayerEditOption> AddOnOrBuyInOptions { get; } =
        [
            new(PlayerEditOption.EditOption.Add1000, type: PlayerEditOption.OptionType.Success, isSelected: true),
            new(PlayerEditOption.EditOption.Add100, type: PlayerEditOption.OptionType.Success),
            new(PlayerEditOption.EditOption.Add10, type: PlayerEditOption.OptionType.Success),
            new(PlayerEditOption.EditOption.Add1, type: PlayerEditOption.OptionType.Success),
            new(PlayerEditOption.EditOption.Ok),
            new(PlayerEditOption.EditOption.Remove1000, type: PlayerEditOption.OptionType.Cancel),
            new(PlayerEditOption.EditOption.Remove100, type: PlayerEditOption.OptionType.Cancel),
            new(PlayerEditOption.EditOption.Remove10, type: PlayerEditOption.OptionType.Cancel),
            new(PlayerEditOption.EditOption.Remove1, type: PlayerEditOption.OptionType.Cancel)
        ];

        public List<PlayerEditOption> SpotOptions { get; } =
        [
            new(PlayerEditOption.EditOption.ChangeName, isSelected: true),
            new(PlayerEditOption.EditOption.ChangeImage),
            new(PlayerEditOption.EditOption.Move),
            new(PlayerEditOption.EditOption.Load),
            new(PlayerEditOption.EditOption.Save),
        ];

        public PlayerSpot? SelectedSpot
        {
            get => _selectedSpot;
            private set => SetProperty(ref _selectedSpot, value);
        }

        public decimal TotalAmountInPot
        {
            get => _totalAmountInPot;
            private set => SetProperty(ref _totalAmountInPot, value);
        }

        public bool TableFull
        {
            get => _tableFull;
            private set => SetProperty(ref _tableFull, value);
        }

        public SideMenuViewModel.GameEditOption CurrentGameEditOption
        {
            get => _currentGameEditOption;
            set => SetProperty(ref _currentGameEditOption, value);
        }

        public GameClock Clock { get; }

        public MainWindowFocusManager FocusManager { get; }

        public GameStagesManager StageManager { get; }

        public NavigationManager NavigationManager { get; }

        public GameSettings GameSettings { get; }
        #endregion

        #region Events
        public event EventHandler<int>? LayoutMightHaveChangedEvent;
        public event EventHandler<InputEvent.NavigationDirection>? Navigate;
        public event EventHandler<IInputRelay.ButtonEventArgs>? ButtonEvent;
        #endregion

        #region Private fields
        private readonly string _pathToDefaultPlayerImage;
        private const int NumberOfPlayerSpots = 12;
        private int _nextPlayerId = 0;
        private bool _moveInProgress = false;
        private TableLayout _currentTableLayout;
        private readonly PlayerEditOption _addOnOrBuyInOption;
        private readonly PlayerEditOption _removeOrEliminateOption;
        private readonly int _playerOptionNavigationId;
        private readonly int _addOnOrBuyInNavigationId;

        private record PlayerConfiguration(int SpotIndex, PlayerModel.PlayerInformation PlayerInformation);
        private record TableConfiguration(List<PlayerConfiguration> PlayerConfigurations);
        #endregion

        public GameSessionManager(string pathToDefaultPlayerImage, MainWindowFocusManager focusManager)
        {
            FocusManager = focusManager;
            GameSettings = new();

            _pathToDefaultPlayerImage = pathToDefaultPlayerImage;
            for (var i = 0; i < NumberOfPlayerSpots; i++)
                PlayerSpots.Add(new() { SpotIndex = i });

            NavigationManager = new();

            _addOnOrBuyInOption = new(PlayerEditOption.EditOption.AddOn, PlayerEditOption.OptionType.Success);
            _removeOrEliminateOption = new(PlayerEditOption.EditOption.Eliminate, PlayerEditOption.OptionType.Cancel);
            SpotOptions.Add(_addOnOrBuyInOption);
            SpotOptions.Add(_removeOrEliminateOption);

            _playerOptionNavigationId = NavigationManager.RegisterNavigation(
                [
                    new(X: 0, Y: 0),
                    new(X: 1, Y: 0),
                    new(X: 0, Y: 1),
                    new(X: 1, Y: 1),
                    new(X: 0, Y: 2),
                    new(X: 1, Y: 2),
                    new(X: 0, Y: 3),
                ]);
            _addOnOrBuyInNavigationId = NavigationManager.RegisterNavigation(
                [
                    new(X: 0, Y: 0),
                    new(X: 1, Y: 0),
                    new(X: 2, Y: 0),
                    new(X: 3, Y: 0),
                    new(X: 4, Y: 0),
                    new(X: 0, Y: 5),
                    new(X: 1, Y: 5),
                    new(X: 2, Y: 5),
                    new(X: 3, Y: 5),
                ]);

            RegisterFocusManagerCallbacks();
            InitializeSpots(8);

            Clock = new();
            StageManager = new(Clock, GameSettings);
        }

        #region Public methods
        public void SetTableLayout(TableLayout tableLayout)
        {
            _currentTableLayout = tableLayout;
        }

        public void AddPlayerToSpot()
        {
            var targetSpot = PlayerSpots.FirstOrDefault(x => !x.HasPlayerData);
            if (targetSpot == default)
                return;

            targetSpot.AddPlayer(_nextPlayerId++, _pathToDefaultPlayerImage);
            var numberOfActivePlayers = PlayerSpots.Where(x => x.HasPlayerData).Count();
            LayoutMightHaveChangedEvent?.Invoke(this, numberOfActivePlayers);
            TableFull = numberOfActivePlayers == NumberOfPlayerSpots;
        }

        public void ConsolidateLayout()
        {
            for (var i = 0; i < NumberOfPlayerSpots; i++)
            {
                if (PlayerSpots[i].HasPlayerData)
                    continue;

                for (var j = i + 1; j < NumberOfPlayerSpots; j++)
                {
                    if (PlayerSpots[j].HasPlayerData)
                    {
                        PlayerSpots[i].Swap(PlayerSpots[j], moveInAction: false);
                        break;
                    }
                }
            }

            LayoutMightHaveChangedEvent?.Invoke(this, PlayerSpots.Where(x => x.HasPlayerData).Count());
        }

        public bool TrySaveGameSettings(string filePath, out string resultMessage)
            => GameSettings.TrySave(StageManager, filePath, out resultMessage);

        public bool TrySaveTableConfiguration(string filePath, out string resultMessage)
        {
            resultMessage = string.Empty;
            var configs = new List<PlayerConfiguration>();
            foreach (var spot in PlayerSpots)
            {
                if (spot.HasPlayerData)
                    configs.Add(new(spot.SpotIndex, spot.PlayerData!.Information));
            }

            if (configs.Count == 0)
            {
                resultMessage = "Save failed - no players";
                return false;
            }

            var tableConfiguration = new TableConfiguration(configs);
            var (success, path, e) = tableConfiguration.SerializeWriteToJsonFile(filePath);
            resultMessage = success ? $"Configuration saved to '{Path.GetFileName(path)}'!" : $"Save failed - {e!.Message}";
            return success;
        }
        #endregion

        #region Private methods
        private void InitializeSpots(int numberOfSpots)
        {
            for (var i = 0; i < numberOfSpots; i++)
                PlayerSpots[i].AddPlayer(_nextPlayerId++, _pathToDefaultPlayerImage);
        }

        private void RegisterFocusManagerCallbacks()
        {
            FocusManager.RegisterPlayerSpots(PlayerSpots);
            FocusManager.RegisterSpotNavigationCallback((int currentSpotIdx, InputEvent.NavigationDirection direction) =>
            {
                // Note: The navigation is set-up in such a way that if no
                //       available spot is found in the requested navigation
                //       direction, the current spot index is returned
                var newSpotIndex = NavigationManager.Navigate(_currentTableLayout, currentSpotIdx, direction,
                    _moveInProgress ? default : (int nextSpotIdx) => PlayerSpots.First(x => x.SpotIndex == nextSpotIdx).HasPlayerData);

                if (_moveInProgress)
                {
                    var currentSpot = PlayerSpots.First(x => x.SpotIndex == currentSpotIdx);
                    PlayerSpots.First(x => x.SpotIndex == newSpotIndex).Swap(currentSpot);
                }
                return newSpotIndex;
            });
            FocusManager.RegisterSpotSelectedCallback((PlayerSpot activeSpot, int currentSpotIdx) =>
            {
                SetOptionsFor(activeSpot.IsEliminated);
            });
            FocusManager.RegisterPlayerOptionsCallback((PlayerSpot activeSpot, InputEvent.NavigationDirection direction) => NavigateOptions(_playerOptionNavigationId, SpotOptions, direction));
            FocusManager.RegisterPlayerOptionSelectCallback((PlayerSpot activeSpot) =>
            {
                switch (GetSelectedOptionIn(SpotOptions).Option)
                {
                    case PlayerEditOption.EditOption.ChangeName:
                        SelectedSpot = activeSpot;
                        return MainWindowFocusManager.FocusArea.EditNameBox;

                    case PlayerEditOption.EditOption.ChangeImage:
                        activeSpot.ChangeImage();
                        // TODO: When there's a nice image picker dialog, this line
                        //       will have to change
                        return MainWindowFocusManager.FocusArea.PlayerInfo;

                    case PlayerEditOption.EditOption.Save:
                        activeSpot.SavePlayer();
                        return MainWindowFocusManager.FocusArea.PlayerInfo;

                    case PlayerEditOption.EditOption.Eliminate:
                        activeSpot.IsEliminated = true;
                        SetOptionsFor(eliminatedPlayer: true);
                        return MainWindowFocusManager.FocusArea.PlayerInfo;

                    case PlayerEditOption.EditOption.Remove:
                        if (!_removeOrEliminateOption.IsAvailable)
                            return MainWindowFocusManager.FocusArea.PlayerInfo;
                        activeSpot.RemovePlayer();
                        var newSpotIdx = activeSpot.SpotIndex + 1;
                        var newSpotToFocus = activeSpot;
                        while (!newSpotToFocus.HasPlayerData)
                            newSpotToFocus = PlayerSpots[newSpotIdx++ % NumberOfPlayerSpots];
                        newSpotToFocus.IsSelected = true;

                        if (TableFull)
                            TableFull = false;

                        return MainWindowFocusManager.FocusArea.Players;

                    case PlayerEditOption.EditOption.Move:
                        _moveInProgress = true;
                        activeSpot.IsBeingMoved = true;
                        return MainWindowFocusManager.FocusArea.MovementInProgress;

                    case PlayerEditOption.EditOption.AddOn:
                        SelectedSpot = activeSpot;
                        SelectedSpot.BuyInOrAddOnAmount = GameSettings.DefaultAddOnAmount;
                        return MainWindowFocusManager.FocusArea.AddOnOrBuyInBox;

                    case PlayerEditOption.EditOption.BuyIn:
                        SetOptionsFor(eliminatedPlayer: false);
                        SelectedSpot = activeSpot;
                        SelectedSpot.BuyInOrAddOnAmount = GameSettings.DefaultBuyInAmount;
                        return MainWindowFocusManager.FocusArea.AddOnOrBuyInBox;

                    default:
                        return MainWindowFocusManager.FocusArea.None;
                };
            });
            FocusManager.RegisterBuyInOrAddOnBoxNavigationCallback((InputEvent.NavigationDirection direction) => NavigateOptions(_addOnOrBuyInNavigationId, AddOnOrBuyInOptions, direction));
            FocusManager.RegisterEditMenuLostFocusCallback(() => SelectedSpot = default);
            FocusManager.RegisterBuyInOrAddOnOptionSelectedCallback(() =>
            {
                if (SelectedSpot == default || SelectedSpot.PlayerData == default)
                    return false;

                var currentOption = GetSelectedOptionIn(AddOnOrBuyInOptions);
                if (currentOption.Option == PlayerEditOption.EditOption.Ok)
                {
                    SelectedSpot.PlayerData.Information.MoneyInThePot += SelectedSpot.BuyInOrAddOnAmount;
                    TotalAmountInPot += SelectedSpot.BuyInOrAddOnAmount;
                    SelectedSpot.BuyInOrAddOnAmount = 0;
                    return true;
                }

                var amountToAdd = currentOption.Option switch
                {
                    PlayerEditOption.EditOption.Add1000 => 1000,
                    PlayerEditOption.EditOption.Add100 => 100,
                    PlayerEditOption.EditOption.Add10 => 10,
                    PlayerEditOption.EditOption.Add1 => 1,
                    PlayerEditOption.EditOption.Remove1000 => -1000,
                    PlayerEditOption.EditOption.Remove100 => -100,
                    PlayerEditOption.EditOption.Remove10 => -10,
                    PlayerEditOption.EditOption.Remove1 => -1,
                    _ => 0,
                };
                SelectedSpot.BuyInOrAddOnAmount += amountToAdd;
                return false;
            });
            FocusManager.RegisterMovementDoneCallback((PlayerSpot spot) =>
            {
                _moveInProgress = false;
                spot.IsBeingMoved = false;
            });
            FocusManager.RegisterSideMenuEditOptionNavigationCallback((InputEvent.NavigationDirection direction) => Navigate?.Invoke(this, direction));
            FocusManager.RegisterSideMenuEditOptionActionCallback((InputEvent.ButtonEventType eventType) =>
            {
                switch (eventType)
                {
                    case InputEvent.ButtonEventType.Start:
                        CurrentGameEditOption = SideMenuViewModel.GameEditOption.None;
                        return true;

                    case InputEvent.ButtonEventType.GoBack:
                        {
                            IInputRelay.ButtonEventArgs eventArgs = new() { ButtonEvent = eventType };
                            ButtonEvent?.Invoke(this, eventArgs);
                            if (!eventArgs.Handled)
                                CurrentGameEditOption = SideMenuViewModel.GameEditOption.None;
                            return !eventArgs.Handled;
                        }

                    case InputEvent.ButtonEventType.Select:
                        if (CurrentGameEditOption == SideMenuViewModel.GameEditOption.GameStages)
                        {
                            if (StageManager.Stages.Count == 0)
                            {
                                StageManager.AddStage(1, false, 1, 2);
                            }
                            else
                            {
                                ButtonEvent?.Invoke(this, new() { ButtonEvent = eventType });
                            }
                        }
                        else if (CurrentGameEditOption == SideMenuViewModel.GameEditOption.ChangeDefaultAddOnAmount ||
                                 CurrentGameEditOption == SideMenuViewModel.GameEditOption.ChangeDefaultBuyInAmount ||
                                 CurrentGameEditOption == SideMenuViewModel.GameEditOption.ChangeDefaultStageLength)
                        {
                            ButtonEvent?.Invoke(this, new() { ButtonEvent = eventType });
                        }
                        return true;

                    default:
                        return false;
                }
            });
        }

        private void NavigateOptions(int navigationId, List<PlayerEditOption> options, InputEvent.NavigationDirection direction)
        {
            var currentOption = GetSelectedOptionIn(options);
            var currentOptionIndex = options.IndexOf(currentOption);

            var newIndex = NavigationManager.Navigate(navigationId, currentOptionIndex, direction);

            currentOption.IsSelected = false;
            options[newIndex].IsSelected = true;
        }

        private void SetOptionsFor(bool eliminatedPlayer)
        {
            _addOnOrBuyInOption.ChangeEditOption(eliminatedPlayer ? PlayerEditOption.EditOption.BuyIn : PlayerEditOption.EditOption.AddOn);
            _removeOrEliminateOption.ChangeEditOption(eliminatedPlayer ? PlayerEditOption.EditOption.Remove : PlayerEditOption.EditOption.Eliminate);
            if (eliminatedPlayer)
                _removeOrEliminateOption.IsAvailable = PlayerSpots.Where(x => x.HasPlayerData).Count() > 1;
            else
                _removeOrEliminateOption.IsAvailable = true;
        }

        private static PlayerEditOption GetSelectedOptionIn(List<PlayerEditOption> options)
            => options.First(x => x.IsSelected);
        #endregion
    }
}
