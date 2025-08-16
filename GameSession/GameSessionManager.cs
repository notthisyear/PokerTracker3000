using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using PokerTracker3000.Common;
using PokerTracker3000.Common.FileUtilities;
using PokerTracker3000.Common.Messages;
using PokerTracker3000.Interfaces;

using InputEvent = PokerTracker3000.Input.UserInputEvent;

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
        private decimal _averagePotSize = 0;
        private int _numberOfPlayers = 0;
        private int _numberOfPlayersNotEliminated = 0;
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
            set => SetProperty(ref _totalAmountInPot, value);
        }

        public decimal AveragePotSize
        {
            get => _averagePotSize;
            private set => SetProperty(ref _averagePotSize, value);
        }

        public int NumberOfPlayers
        {
            get => _numberOfPlayers;
            private set => SetProperty(ref _numberOfPlayers, value);
        }

        public int NumberOfPlayersNotEliminated
        {
            get => _numberOfPlayersNotEliminated;
            private set => SetProperty(ref _numberOfPlayersNotEliminated, value);
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

        public ChipManager ChipManager { get; }
        #endregion

        #region Events
        public event EventHandler<int>? LayoutMightHaveChangedEvent;
        public event EventHandler? StagesCollectionLoadedFromFile;
        public event EventHandler<InputEvent.NavigationDirection>? Navigate;
        public event EventHandler<IInputRelay.ButtonEventArgs>? ButtonEvent;
        #endregion

        #region Private fields
        private readonly string _pathToDefaultPlayerImage;
        private const int NumberOfPlayerSpots = 12;
        private bool _moveInProgress = false;
        private TableLayout _currentTableLayout;
        private readonly PlayerEditOption _setOrRemoveAsChipLeadOption;
        private readonly PlayerEditOption _addOnOrBuyInOption;
        private readonly PlayerEditOption _removeOrEliminateOption;
        private readonly int _playerOptionNavigationId;
        private readonly int _addOnOrBuyInNavigationId;
        private readonly IGameEventBus _eventBus;

        private record PlayerConfiguration(int SpotIndex, PlayerModel PlayerModel);

        private record TableConfiguration(List<PlayerConfiguration> PlayerConfigurations);
        #endregion

        public GameSessionManager(IGameEventBus eventBus,
            GameSettings settings,
            MainWindowFocusManager focusManager,
            GameStagesManager stagesManager,
            ChipManager chipManager,
            GameClock clock,
            string pathToDefaultPlayerImage)
        {
            _eventBus = eventBus;
            GameSettings = settings;
            FocusManager = focusManager;
            StageManager = stagesManager;
            ChipManager = chipManager;
            Clock = clock;
            _pathToDefaultPlayerImage = pathToDefaultPlayerImage;

            // TODO: This should be handled in some other way
            ChipManager.AddChip("#FFF8F8FF", "#FF000000", 1);
            ChipManager.AddChip("#FFDE4235", "#FFDC7633", 5);
            ChipManager.AddChip("#FF0A3E41", "#FF2DA38F", 25);
            ChipManager.AddChip("#FF412DA3", "#FF1C8086", 100);
            ChipManager.AddChip("#FF273243", "#FFEB674D", 500);


            for (var i = 0; i < NumberOfPlayerSpots; i++)
                PlayerSpots.Add(new() { SpotIndex = i });

            NavigationManager = new();

            _setOrRemoveAsChipLeadOption = new(PlayerEditOption.EditOption.SetAsChipLead);
            _addOnOrBuyInOption = new(PlayerEditOption.EditOption.AddOn, PlayerEditOption.OptionType.Success);
            _removeOrEliminateOption = new(PlayerEditOption.EditOption.Eliminate, PlayerEditOption.OptionType.Cancel);

            SpotOptions.Add(_setOrRemoveAsChipLeadOption);
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
                    new(X: 1, Y: 3),
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
            InitializeSpots(10);
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

            targetSpot.AddPlayer($"Player {targetSpot.SpotIndex + 1}", _pathToDefaultPlayerImage);
            NumberOfPlayers = PlayerSpots.Where(x => x.HasPlayerData).Count();
            NumberOfPlayersNotEliminated++;
            SetAveragePotSize();
            LayoutMightHaveChangedEvent?.Invoke(this, NumberOfPlayers);
            TableFull = NumberOfPlayers == NumberOfPlayerSpots;
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

        public bool TryLoadGameSettingsFromFile(string filePath, out string resultMessage)
        {
            var result = GameSettings.TryLoadFromFile(StageManager, filePath, out resultMessage);
            if (result)
                StagesCollectionLoadedFromFile?.Invoke(this, EventArgs.Empty);
            return result;
        }

        public bool TrySaveTableConfiguration(string filePath, out string resultMessage)
        {
            resultMessage = string.Empty;
            var configs = new List<PlayerConfiguration>();
            foreach (var spot in PlayerSpots)
            {
                if (spot.HasPlayerData)
                    configs.Add(new(spot.SpotIndex, spot.PlayerData!));
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

        public bool TryLoadTableConfigurationFromFile(string filePath, out string resultMessage)
        {
            FileTextReader reader = new(filePath);
            if (!reader.SuccessfulRead)
            {
                resultMessage = $"Reading table configuration failed - {reader.ReadException!.Message}";
                return false;
            }

            var (configuration, e) = reader.AllText.DeserializeJsonString<TableConfiguration>(convertSnakeCaseToPascalCase: true);
            if (e != default)
            {
                resultMessage = $"Reading table configuration failed - {e!.Message}";
                return false;
            }

            if (configuration!.PlayerConfigurations == default || configuration.PlayerConfigurations.Count == 0)
            {
                resultMessage = "Reading table configuration failed - no players found";
                return false;
            }

            TotalAmountInPot = 0;
            var totalNumberofPlayers = 0;
            foreach (var spot in PlayerSpots)
            {
                var playerDataOrNull = configuration!.PlayerConfigurations.FirstOrDefault(x => x.SpotIndex == spot.SpotIndex);
                if (playerDataOrNull == null)
                {
                    spot.RemovePlayer();
                    continue;
                }

                var playerData = playerDataOrNull!;
                if (!Path.Exists(playerData.PlayerModel.PathToImage))
                    playerData.PlayerModel.PathToImage = _pathToDefaultPlayerImage;

                spot.AddPlayer(playerData.PlayerModel);
                TotalAmountInPot += playerData.PlayerModel.MoneyInThePot;
                totalNumberofPlayers++;
            }
            NumberOfPlayers = totalNumberofPlayers;
            NumberOfPlayersNotEliminated = NumberOfPlayers;
            SetAveragePotSize();
            ConsolidateLayout();
            resultMessage = $"Table configuration read from '{Path.GetFileName(filePath)}'!";
            return true;
        }
        #endregion

        #region Private methods
        private void InitializeSpots(int numberOfSpots)
        {
            for (var i = 0; i < numberOfSpots; i++)
                PlayerSpots[i].AddPlayer($"Player {i + 1}", _pathToDefaultPlayerImage);
            NumberOfPlayers = numberOfSpots;
            NumberOfPlayersNotEliminated = NumberOfPlayers;
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
                SetOptionsFor(activeSpot.IsEliminated, activeSpot.PlayerData?.IsChipLead ?? false);
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

                    // TODO: Some sort of save/load feedback would be nice
                    case PlayerEditOption.EditOption.Save:
                        activeSpot.TrySavePlayer();
                        return MainWindowFocusManager.FocusArea.PlayerInfo;

                    case PlayerEditOption.EditOption.Load:
                        var currentAmountInSpot = activeSpot.HasPlayerData ? activeSpot.PlayerData!.MoneyInThePot : 0;
                        activeSpot.TryLoadPlayer();
                        if (activeSpot.HasPlayerData && activeSpot.PlayerData!.MoneyInThePot != currentAmountInSpot)
                            TotalAmountInPot += (activeSpot.PlayerData!.MoneyInThePot - currentAmountInSpot);
                        return MainWindowFocusManager.FocusArea.PlayerInfo;

                    case PlayerEditOption.EditOption.SetAsChipLead:
                        var currentChipLead = PlayerSpots.FirstOrDefault(x => x.PlayerData?.IsChipLead ?? false);
                        if (currentChipLead != default)
                            currentChipLead.PlayerData!.IsChipLead = false;

                        if (activeSpot.PlayerData != default)
                            activeSpot.PlayerData.IsChipLead = true;

                        SetOptionsFor(eliminatedPlayer: false, isChipLead: true);
                        return MainWindowFocusManager.FocusArea.PlayerInfo;

                    case PlayerEditOption.EditOption.RemoveAsChipLead:
                        if (activeSpot.PlayerData != default)
                            activeSpot.PlayerData.IsChipLead = false;

                        SetOptionsFor(eliminatedPlayer: false, isChipLead: false);
                        return MainWindowFocusManager.FocusArea.PlayerInfo;

                    case PlayerEditOption.EditOption.Eliminate:
                        activeSpot.IsEliminated = true;
                        SetOptionsFor(eliminatedPlayer: true, activeSpot.PlayerData?.IsChipLead ?? false);
                        NumberOfPlayersNotEliminated--;
                        SetAveragePotSize();
                        Notify(PlayerEventMessage.Type.Eliminated, activeSpot.PlayerData!.Name, playerTotalAmount: activeSpot.PlayerData!.MoneyInThePot);
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
                        NumberOfPlayers--;

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
                        SelectedSpot = activeSpot;
                        SelectedSpot.BuyInOrAddOnAmount = GameSettings.DefaultBuyInAmount;
                        return MainWindowFocusManager.FocusArea.AddOnOrBuyInBox;

                    default:
                        return MainWindowFocusManager.FocusArea.None;
                }
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
                    TotalAmountInPot += SelectedSpot.BuyInOrAddOnAmount;
                    SelectedSpot.PlayerData.MoneyInThePot += SelectedSpot.BuyInOrAddOnAmount;

                    Notify(SelectedSpot.IsEliminated ? PlayerEventMessage.Type.BuyIn : PlayerEventMessage.Type.AddOn,
                        SelectedSpot.PlayerData!.Name,
                        addOnOrBuyInAmount: SelectedSpot.BuyInOrAddOnAmount,
                        playerTotalAmount: SelectedSpot.PlayerData.MoneyInThePot,
                        potTotal: TotalAmountInPot);

                    SelectedSpot.BuyInOrAddOnAmount = 0;
                    if (SelectedSpot.IsEliminated)
                    {
                        SelectedSpot.IsEliminated = false;
                        NumberOfPlayersNotEliminated++;
                        SetOptionsFor(eliminatedPlayer: false, SelectedSpot.PlayerData?.IsChipLead ?? false);
                    }
                    SetAveragePotSize();
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
                    case InputEvent.ButtonEventType.GoBack:
                        IInputRelay.ButtonEventArgs eventArgs = new() { ButtonEvent = eventType };
                        ButtonEvent?.Invoke(this, eventArgs);
                        if (!eventArgs.Handled)
                            CurrentGameEditOption = SideMenuViewModel.GameEditOption.None;
                        return !eventArgs.Handled;

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

        private void SetOptionsFor(bool eliminatedPlayer, bool isChipLead)
        {
            _addOnOrBuyInOption.ChangeEditOption(eliminatedPlayer ? PlayerEditOption.EditOption.BuyIn : PlayerEditOption.EditOption.AddOn);
            _removeOrEliminateOption.ChangeEditOption(eliminatedPlayer ? PlayerEditOption.EditOption.Remove : PlayerEditOption.EditOption.Eliminate);
            _setOrRemoveAsChipLeadOption.ChangeEditOption(isChipLead ? PlayerEditOption.EditOption.RemoveAsChipLead : PlayerEditOption.EditOption.SetAsChipLead);

            if (eliminatedPlayer)
                _removeOrEliminateOption.IsAvailable = PlayerSpots.Where(x => x.HasPlayerData).Count() > 1;
            else
                _removeOrEliminateOption.IsAvailable = true;
        }

        private void Notify(PlayerEventMessage.Type type, string playerName, decimal addOnOrBuyInAmount = 0, decimal playerTotalAmount = 0, decimal potTotal = 0)
        {
            _eventBus.NotifyListeners(type switch
            {
                PlayerEventMessage.Type.BuyIn => GameEventBus.EventType.PlayerBuyIn,
                PlayerEventMessage.Type.AddOn => GameEventBus.EventType.PlayerAddOn,
                PlayerEventMessage.Type.Eliminated => GameEventBus.EventType.PlayerEliminated,
                _ => throw new NotImplementedException(),
            }, new PlayerEventMessage(type, playerName, addOnOrBuyInAmount, playerTotalAmount, potTotal, GameSettings.CurrencyType));
        }
        private void SetAveragePotSize()
        {
            AveragePotSize = NumberOfPlayersNotEliminated > 0 ? TotalAmountInPot / NumberOfPlayersNotEliminated : TotalAmountInPot;
        }

        private static PlayerEditOption GetSelectedOptionIn(List<PlayerEditOption> options)
            => options.First(x => x.IsSelected);
        #endregion
    }
}
