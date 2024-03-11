﻿using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using PokerTracker3000.GameComponents;

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

    public class GameSessionManager : ObservableObject
    {
        #region Public properties

        #region Backing fields
        private PlayerSpot? _selectedSpot;
        private CurrencyType _currencyType = CurrencyType.SwedishKrona;
        private bool _tableFull = false;
        private decimal _totalAmountInPot = 0;
        private decimal _defaultBuyInAmount = 500;
        private decimal _defaultAddOnAmount = 500;
        #endregion

        public List<PlayerSpot> PlayerSpots { get; } = [];

        public List<PlayerEditOption> AddOnOrBuyInOptions { get; } =
        [
            new(PlayerEditOption.EditOption.Add1000, type: PlayerEditOption.OptionType.Success, isSelected: true),
            new(PlayerEditOption.EditOption.Add100, type: PlayerEditOption.OptionType.Success),
            new(PlayerEditOption.EditOption.Add10, type: PlayerEditOption.OptionType.Success),
            new(PlayerEditOption.EditOption.Add1, type: PlayerEditOption.OptionType.Success),
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
        ];

        public PlayerSpot? SelectedSpot
        {
            get => _selectedSpot;
            private set => SetProperty(ref _selectedSpot, value);
        }

        public CurrencyType CurrencyType
        {
            get => _currencyType;
            private set => SetProperty(ref _currencyType, value);
        }

        public decimal TotalAmountInPot
        {
            get => _totalAmountInPot;
            private set => SetProperty(ref _totalAmountInPot, value);
        }

        public decimal DefaultBuyInAmount
        {
            get => _defaultBuyInAmount;
            private set => SetProperty(ref _defaultBuyInAmount, value);
        }

        public decimal DefaultAddOnAmount
        {
            get => _defaultAddOnAmount;
            private set => SetProperty(ref _defaultAddOnAmount, value);
        }

        public bool TableFull
        {
            get => _tableFull;
            private set => SetProperty(ref _tableFull, value);
        }

        public GameClock Clock { get; } = new();

        public MainWindowFocusManager FocusManager { get; }
        #endregion

        #region Events
        public event EventHandler<int>? LayoutMightHaveChangedEvent;
        #endregion

        #region Private fields
        private const int NumberOfPlayerSpots = 12;
        private readonly string _pathToDefaultPlayerImage;
        private readonly NavigationManager _navigationManager;
        private int _nextPlayerId = 0;
        private bool _moveInProgress = false;
        private TableLayout _currentTableLayout;
        private readonly PlayerEditOption _addOnOrBuyInOption;
        private readonly PlayerEditOption _removeOrEliminateOption;
        #endregion

        public GameSessionManager(string pathToDefaultPlayerImage, MainWindowFocusManager focusManager)
        {
            FocusManager = focusManager;

            _pathToDefaultPlayerImage = pathToDefaultPlayerImage;
            for (var i = 0; i < NumberOfPlayerSpots; i++)
                PlayerSpots.Add(new() { SpotIndex = i });

            _navigationManager = new(PlayerSpots.AsReadOnly());


            _addOnOrBuyInOption = new(PlayerEditOption.EditOption.AddOn, PlayerEditOption.OptionType.Success);
            _removeOrEliminateOption = new(PlayerEditOption.EditOption.Eliminate, PlayerEditOption.OptionType.Cancel);
            SpotOptions.Add(_addOnOrBuyInOption);
            SpotOptions.Add(_removeOrEliminateOption);

            RegisterFocusManagerCallbacks();
            InitializeSpots(8);
        }

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
                var newSpotIndex = _navigationManager.Navigate(_currentTableLayout, currentSpotIdx, direction, _moveInProgress);
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
            FocusManager.RegisterPlayerOptionsCallback((PlayerSpot activeSpot, InputEvent.NavigationDirection direction) =>
            {
                var currentOption = GetSelectedOption();
                var currentOptionIndex = SpotOptions.IndexOf(currentOption);

                var onTopRow = currentOptionIndex < 2;
                var numberOfOptionsEven = (SpotOptions.Count % 2) == 0;
                var onBottomRow = currentOptionIndex >= (SpotOptions.Count - (numberOfOptionsEven ? 2 : 1));
                var isOnLastSingleOption = onBottomRow && !numberOfOptionsEven;

                var newOptionIndex = direction switch
                {
                    InputEvent.NavigationDirection.Left or
                    InputEvent.NavigationDirection.Right =>
                    (isOnLastSingleOption ? currentOptionIndex - 1 :
                    (currentOptionIndex % 2 == 0) ? currentOptionIndex + 1 : currentOptionIndex - 1),
                    InputEvent.NavigationDirection.Down => onBottomRow ?
                    (currentOptionIndex % 2) : Math.Min(currentOptionIndex + 2, SpotOptions.Count - 1),
                    InputEvent.NavigationDirection.Up => onTopRow ?
                    (SpotOptions.Count - (numberOfOptionsEven ? (2 - currentOptionIndex) : (1 + currentOptionIndex))) : (currentOptionIndex - 2),
                    _ => currentOptionIndex
                };
                currentOption.IsSelected = false;
                SpotOptions[newOptionIndex].IsSelected = true;
            });
            FocusManager.RegisterPlayerInfoBoxSelectCallback((PlayerSpot activeSpot) =>
            {
                switch (GetSelectedOption().Option)
                {
                    case PlayerEditOption.EditOption.ChangeName:
                        SelectedSpot = activeSpot;
                        return MainWindowFocusManager.FocusArea.EditNameBox;

                    case PlayerEditOption.EditOption.ChangeImage:
                        activeSpot.ChangeImage();
                        // TODO: When there's a nice image picker dialog, this line
                        //       will have to change
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
                        return MainWindowFocusManager.FocusArea.AddOnOrBuyInBox;

                    case PlayerEditOption.EditOption.BuyIn:
                        SetOptionsFor(eliminatedPlayer: false);
                        SelectedSpot = activeSpot;
                        return MainWindowFocusManager.FocusArea.AddOnOrBuyInBox;

                    default:
                        return MainWindowFocusManager.FocusArea.None;
                };
            });
            FocusManager.RegisterEditMenuLostFocusCallback(() =>
            {
                SelectedSpot = default;
            });
            FocusManager.RegisterMovementDoneCallback((PlayerSpot spot) =>
            {
                _moveInProgress = false;
                spot.IsBeingMoved = false;
            });
        }

        private PlayerEditOption GetSelectedOption()
            => SpotOptions.First(x => x.IsSelected);

        private void SetOptionsFor(bool eliminatedPlayer)
        {
            _addOnOrBuyInOption.ChangeEditOption(eliminatedPlayer ? PlayerEditOption.EditOption.BuyIn : PlayerEditOption.EditOption.AddOn);
            _removeOrEliminateOption.ChangeEditOption(eliminatedPlayer ? PlayerEditOption.EditOption.Remove : PlayerEditOption.EditOption.Eliminate);
            if (eliminatedPlayer)
                _removeOrEliminateOption.IsAvailable = PlayerSpots.Where(x => x.HasPlayerData).Count() > 1;
            else
                _removeOrEliminateOption.IsAvailable = true;
        }
        #endregion
    }
}
