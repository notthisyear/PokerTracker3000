﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Ookii.Dialogs.Wpf;
using PokerTracker3000.Common;
using PokerTracker3000.Common.Messages;
using PokerTracker3000.Interfaces;

using InputEvent = PokerTracker3000.Input.UserInputEvent;

namespace PokerTracker3000.GameSession
{
    public class SideMenuViewModel : ObservableObject
    {
        public enum GameEditOption
        {
            None,
            GameStages,
            ChangeDefaultAddOnAmount,
            ChangeDefaultBuyInAmount,
            ChangeDefaultStageLength,
            ChangeCurrency
        };

        public enum LastSaveLoadResult
        {
            None,
            Success,
            Failure,
        }

        #region Public properties

        #region Private fields
        private bool _isOpen = false;
        private int _currentStageNumber = 0;
        private LastSaveLoadResult _lastSaveLoadStatus = LastSaveLoadResult.None;
        private string _lastSaveLoadMessage = string.Empty;
        #endregion

        public bool IsOpen
        {
            get => _isOpen;
            private set => SetProperty(ref _isOpen, value);
        }

        public int CurrentStageNumber
        {
            get => _currentStageNumber;
            private set => SetProperty(ref _currentStageNumber, value);
        }

        public LastSaveLoadResult LastSaveLoadStatus
        {
            get => _lastSaveLoadStatus;
            private set => SetProperty(ref _lastSaveLoadStatus, value);
        }

        public string LastSaveLoadMessage
        {
            get => _lastSaveLoadMessage;
            private set => SetProperty(ref _lastSaveLoadMessage, value);
        }

        public GameSessionManager SessionManager { get; }

        public List<SideMenuOptionModel> SideMenuOptions { get; }
        #endregion

        #region Private fields
        private readonly IGameEventBus _eventBus;
        private readonly MainWindowFocusManager _focusManager;
        private readonly SpotifyClientViewModel _spotifyViewModel;
        private int _currentFocusOptionId = 0;
        private readonly Stack<int> _currentFocusParentOptionStack;
        private readonly Stack<List<SideMenuOptionModel>> _currentFocusParentOptionListStack;
        private readonly List<(SideMenuOptionModel target, Action<SideMenuOptionModel> action)> _onOpenCallbacks;
        private List<SideMenuOptionModel> _currentFocusOptionList;

        private readonly int _startPauseGameOptionId;
        private readonly int _goToOptionId;
        private readonly int _gameSettingsOptionId;
        private readonly int _loadSettingsOptionId;
        private readonly int _loginSpotifyOption;
        private readonly int _previousStageOptionId;
        private readonly int _nextStageOptionId;
        private readonly int _editStagesOptionId;
        private readonly int _resetStageOptionId;
        private readonly int _resetAllStagesOptionId;
        private readonly int _loadGameSettingsOptionId;
        private readonly int _loadPlayerConfigurationOptionId;

        private static readonly VistaSaveFileDialog s_saveSettingsDialog = new()
        {
            Title = "Save settings",
            AddExtension = true,
            DefaultExt = "json",
            Filter = "JSON file (*.json)|*.json"
        };
        private static readonly VistaOpenFileDialog s_loadSettingsDialog = new()
        {
            Title = "Load settings file",
            Multiselect = false,
            Filter = "JSON file (*.json)|*.json|All files (*.*)|*.*"
        };
        #endregion

        public SideMenuViewModel(IGameEventBus eventBus, MainWindowFocusManager focusManager, GameSessionManager sessionManager, SpotifyClientViewModel spotifyViewModel)
        {
            _eventBus = eventBus;
            _focusManager = focusManager;
            _spotifyViewModel = spotifyViewModel;

            _onOpenCallbacks = [];
            _currentFocusParentOptionStack = new();
            _currentFocusParentOptionListStack = new();

            SessionManager = sessionManager;
            SideMenuOptions = [];

            InitializeOptionsList();
            _startPauseGameOptionId = SideMenuOptions.First(x => x.OptionText.Equals("Start game", StringComparison.InvariantCulture)).Id;
            _goToOptionId = SideMenuOptions.First(x => x.OptionText.Equals("Go to...", StringComparison.InvariantCulture)).Id;
            _gameSettingsOptionId = SideMenuOptions.First(x => x.OptionText.Equals("Game settings...", StringComparison.InvariantCulture)).Id;
            _loadSettingsOptionId = SideMenuOptions.First(x => x.OptionText.Equals("Load...", StringComparison.InvariantCulture)).Id;
            _loginSpotifyOption = SideMenuOptions.First(x => x.OptionText.Equals("Link Spotify", StringComparison.InvariantCulture)).Id;
            _previousStageOptionId = SideMenuOptions[_goToOptionId].SubOptions.First(x => x.OptionText.Equals("Previous stage", StringComparison.InvariantCulture)).Id;
            _nextStageOptionId = SideMenuOptions[_goToOptionId].SubOptions.First(x => x.OptionText.Equals("Next stage", StringComparison.InvariantCulture)).Id;
            _editStagesOptionId = SideMenuOptions[_gameSettingsOptionId].SubOptions.First(x => x.OptionText.Equals("Edit stages", StringComparison.InvariantCulture)).Id;
            _resetStageOptionId = SideMenuOptions[_gameSettingsOptionId].SubOptions.First(x => x.OptionText.Equals("Reset current stage", StringComparison.InvariantCulture)).Id;
            _resetAllStagesOptionId = SideMenuOptions[_gameSettingsOptionId].SubOptions.First(x => x.OptionText.Equals("Reset all stages", StringComparison.InvariantCulture)).Id;
            _loadGameSettingsOptionId = SideMenuOptions[_loadSettingsOptionId].SubOptions.First(x => x.OptionText.Equals("Game settings", StringComparison.InvariantCulture)).Id;
            _loadPlayerConfigurationOptionId = SideMenuOptions[_loadSettingsOptionId].SubOptions.First(x => x.OptionText.Equals("Table configuration", StringComparison.InvariantCulture)).Id;

            _currentFocusOptionList = SideMenuOptions;
            SideMenuOptions.First(x => x.Id == _currentFocusOptionId).IsHighlighted = true;

            _focusManager.RegisterSideVisibilityChangedCallback(isVisible =>
            {
                IsOpen = isVisible;
                if (IsOpen)
                {
                    foreach (var (target, action) in _onOpenCallbacks)
                        action.Invoke(target);
                }
                else
                {
                    ResetToOutermostMenu();
                }
            });
            _focusManager.RegisterSideMenuNavigationCallback(direction =>
            {
                switch (direction)
                {
                    case InputEvent.NavigationDirection.Up:
                    case InputEvent.NavigationDirection.Down:
                        var newOptionId = direction switch
                        {
                            InputEvent.NavigationDirection.Up => _currentFocusOptionId == 0 ? _currentFocusOptionList.Count - 1 : _currentFocusOptionId - 1,
                            InputEvent.NavigationDirection.Down => (_currentFocusOptionId == _currentFocusOptionList.Count - 1) ? 0 : _currentFocusOptionId + 1,
                            _ => _currentFocusOptionId
                        };

                        _currentFocusOptionList.First(x => x.Id == _currentFocusOptionId).IsHighlighted = false;
                        _currentFocusOptionList.First(x => x.Id == newOptionId).IsHighlighted = true;
                        _currentFocusOptionId = newOptionId;
                        break;

                    case InputEvent.NavigationDirection.Right:
                        {
                            var currentOption = _currentFocusOptionList.First(x => x.Id == _currentFocusOptionId);
                            if (currentOption.HasSubOptions)
                                EnterSubOptionMenu(currentOption);
                        }
                        break;

                    case InputEvent.NavigationDirection.Left:
                        if (_currentFocusParentOptionStack.Count > 0)
                            ExitSubOptionMenu();
                        break;
                };
            });
            _focusManager.RegisterSideMenuButtonCallback(buttonPressed =>
            {
                switch (buttonPressed)
                {
                    case InputEvent.ButtonEventType.Select:
                        {
                            var currentOption = _currentFocusOptionList.First(x => x.Id == _currentFocusOptionId);
                            if (currentOption.HasSubOptions)
                                EnterSubOptionMenu(currentOption);
                            else if (currentOption.IsAvailable)
                                currentOption.OptionAction?.Invoke(currentOption);
                        }
                        break;

                    case InputEvent.ButtonEventType.GoBack:
                        if (_currentFocusParentOptionStack.Count > 0)
                            ExitSubOptionMenu();
                        else
                            return true;
                        break;
                }
                return false;
            });

            SessionManager.StageManager.StageAdded += (s, e) => UpdateOptionAvailability();
            SessionManager.StageManager.StageRemoved += (s, e) => UpdateOptionAvailability();
            SessionManager.StageManager.CurrentStageChanged += (s, e) =>
            {
                if (e != default)
                {
                    CurrentStageNumber = e.Number;
                    SideMenuOptions[_goToOptionId].SubOptions[_previousStageOptionId].IsAvailable = SessionManager.StageManager.TryGetStageByNumber(e.Number - 1, out _);
                    SideMenuOptions[_goToOptionId].SubOptions[_nextStageOptionId].IsAvailable = SessionManager.StageManager.TryGetStageByNumber(e.Number + 1, out _);
                }
            };
            SessionManager.StageManager.AllStagesDone += (s, e) =>
            {
                SideMenuOptions[_startPauseGameOptionId].OptionText = "Start game";
                SideMenuOptions[_startPauseGameOptionId].UnavaliableDescriptionText = "Add more or reset stages to start";
                SideMenuOptions[_startPauseGameOptionId].IsAvailable = false;
                SessionManager.Clock.Stop();
            };

            _spotifyViewModel.PropertyChanged += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.PropertyName) && e.PropertyName.Equals(nameof(_spotifyViewModel.AuthorizedUser), StringComparison.InvariantCulture))
                {
                    var userLoggedIn = !string.IsNullOrEmpty(_spotifyViewModel.AuthorizedUser);
                    SideMenuOptions[_loginSpotifyOption].OptionText = userLoggedIn ? "Unlink Spotify" : "Link Spotify";
                    SideMenuOptions[_loginSpotifyOption].DescriptionText = userLoggedIn ? $"'{_spotifyViewModel.AuthorizedUser}' is logged in" : "Log-in a Spotify user";

                }
            };

            _eventBus.RegisterListener(this, (t, m) => ApplicationClosing(m), GameEventBus.EventType.ApplicationClosing);
        }

        #region Private methods
        private void InitializeOptionsList()
        {
            SideMenuOptions.Add(new()
            {
                Id = 0,
                OptionText = "Start game",
                DescriptionText = "Start the game",
                IsAvailable = SessionManager.StageManager.Stages.Any(),
                UnavaliableDescriptionText = "Add stages to start the game",
                OptionAction = (SideMenuOptionModel opt) =>
                {
                    if (SessionManager.Clock.IsRunning)
                    {
                        opt.OptionText = "Start game";
                        opt.DescriptionText = "Start the game";
                        opt.UnavaliableDescriptionText = "Add stages to start the game";
                        SessionManager.Clock.Pause();
                    }
                    else
                    {
                        opt.OptionText = "Pause game";
                        opt.DescriptionText = "Resume the game";
                        opt.UnavaliableDescriptionText = "Add stages to resume the game";
                        SessionManager.Clock.Start();
                    }
                    SideMenuOptions[_gameSettingsOptionId].SubOptions[_editStagesOptionId].IsAvailable = !SessionManager.Clock.IsRunning;
                    SideMenuOptions[_loadSettingsOptionId].SubOptions[_loadGameSettingsOptionId].IsAvailable = !SessionManager.Clock.IsRunning;
                    SideMenuOptions[_loadSettingsOptionId].SubOptions[_loadPlayerConfigurationOptionId].IsAvailable = !SessionManager.Clock.IsRunning;
                }
            });
            SideMenuOptions.Add(new()
            {
                Id = 1,
                OptionText = "Go to...",
                DescriptionText = "Change current stage",
                HasSubOptions = true,
                SubOptions =
                [
                    new()
                    {
                        Id = 0,
                        OptionText = "Next stage",
                        IsAvailable = SessionManager.StageManager.Stages.Count > 1,
                        UnavaliableDescriptionText = "No next stage",
                        DescriptionText = "Go to next stage",
                        IsSubOption = true,
                        OptionAction = (SideMenuOptionModel opt) =>
                    {
                        SessionManager.StageManager.TryGotoNextStage();
                    }},
                    new()
                    {
                        Id = 1,
                        OptionText = "Previous stage",
                        IsAvailable = false,
                        UnavaliableDescriptionText = "No previous stage",
                        DescriptionText = "Go to previous stage",
                        IsSubOption = true,
                        OptionAction = (SideMenuOptionModel opt) =>
                    {
                        SessionManager.StageManager.TryGotoPreviousStage();
                    }}
                ],
            });

            SideMenuOptionModel addPlayerOption = new()
            {
                Id = 0,
                OptionText = "Add player",
                DescriptionText = "Add a new player to an empty spot",
                IsSubOption = true,
                OnOpenAction = (SideMenuOptionModel opt) => { opt.IsAvailable = !SessionManager.TableFull; },
                OptionAction = (SideMenuOptionModel opt) =>
                {
                    SessionManager.AddPlayerToSpot();
                    opt.IsAvailable = !SessionManager.TableFull;
                },
                UnavaliableDescriptionText = "Cannot add new player - table full"
            };
            _onOpenCallbacks.Add((addPlayerOption, addPlayerOption.OnOpenAction));
            SideMenuOptions.Add(new()
            {
                Id = 2,
                OptionText = "Table settings...",
                DescriptionText = "Change table settings",
                HasSubOptions = true,
                SubOptions =
                [
                    addPlayerOption,
                    new()
                    {
                        Id = 1,
                        OptionText = "Remove empty spots",
                        DescriptionText = "Removes all empty spots",
                        IsSubOption = true,
                        OptionAction = (_) => SessionManager.ConsolidateLayout(),
                    }
                ]
            });

            SideMenuOptions.Add(new()
            {
                Id = 3,
                OptionText = "Game settings...",
                DescriptionText = "Change settings for the current game",
                HasSubOptions = true,
                SubOptions =
                [
                    new()
                    {
                        Id = 0,
                        OptionText = "Edit stages",
                        DescriptionText = "Add, remove or change stages",
                        IsAvailable = !SessionManager.Clock.IsRunning,
                        UnavaliableDescriptionText = "Pause game to edit",
                        IsSubOption = true,
                        OptionAction = (_) =>
                        {
                            SessionManager.CurrentGameEditOption = GameEditOption.GameStages;
                            _focusManager.SideMenuEditOptionSelected();
                        }
                    },
                    new() { Id = 1, OptionText = "Edit poker chips", IsSubOption = true, IsAvailable = false, UnavaliableDescriptionText = "Not yet implemented" },
                    new() { Id = 2, OptionText = "Pay-out ratios", IsSubOption = true, IsAvailable = false, UnavaliableDescriptionText = "Not yet implemented" },
                    new()
                    {
                        Id = 3,
                        OptionText = "Add-on amount",
                        IsSubOption = true,
                        DescriptionText = "Change default add-on amount",
                        OptionAction = (_) =>
                        {
                            SessionManager.CurrentGameEditOption = GameEditOption.ChangeDefaultAddOnAmount;
                            _focusManager.SideMenuEditOptionSelected();
                        }
                    },
                    new()
                    {
                        Id = 4,
                        OptionText = "Buy-in amount",
                        IsSubOption = true,
                        DescriptionText = "Change default buy-in amount",
                        OptionAction = (_) =>
                        {
                            SessionManager.CurrentGameEditOption = GameEditOption.ChangeDefaultBuyInAmount;
                            _focusManager.SideMenuEditOptionSelected();
                        }
                    },
                    new()
                    {
                        Id = 5,
                        OptionText = "Stage length",
                        IsSubOption = true,
                        DescriptionText = "Change default stage length",
                        OptionAction = (_) =>
                        {
                            SessionManager.CurrentGameEditOption = GameEditOption.ChangeDefaultStageLength;
                            _focusManager.SideMenuEditOptionSelected();
                        }
                    },
                    new()
                    {
                        Id = 6,
                        OptionText = "Currency",
                        IsSubOption = true,
                        DescriptionText = "Change the game currency",
                        OptionAction = (_) =>
                        {
                            SessionManager.CurrentGameEditOption = GameEditOption.ChangeCurrency;
                            _focusManager.SideMenuEditOptionSelected();
                        }
                    },
                    new()
                    {
                        Id = 7,
                        OptionText = "Reset current stage",
                        DescriptionText = "Reset time for current stages",
                        IsAvailable = SessionManager.StageManager.CurrentStage != default,
                        UnavaliableDescriptionText = "No stages added",
                        IsSubOption = true, OptionAction = (_) =>
                        {
                            SessionManager.StageManager.ResetCurrentStage();
                            if (SessionManager.Clock.IsRunning)
                            {
                                SideMenuOptions[_startPauseGameOptionId].OptionAction!.Invoke(SideMenuOptions[_startPauseGameOptionId]);
                                SideMenuOptions[_startPauseGameOptionId].IsAvailable = true;
                            }
                        }
                    },
                    new()
                    {
                        Id = 8,
                        OptionText = "Reset all stages",
                        DescriptionText = "Reset time for all stages",
                        IsAvailable = SessionManager.StageManager.Stages.Any(),
                        UnavaliableDescriptionText = "No stages added",
                        IsSubOption = true, OptionAction = (_) =>
                        {
                            SessionManager.StageManager.ResetAllStages();
                            if (SessionManager.Clock.IsRunning)
                            {
                                SideMenuOptions[_startPauseGameOptionId].OptionAction!.Invoke(SideMenuOptions[_startPauseGameOptionId]);
                                SideMenuOptions[_startPauseGameOptionId].IsAvailable = true;
                            }
                        }
                    },
                    new()
                    {
                        Id = 9,
                        OptionText = "Reset all amounts",
                        DescriptionText = "Reset all player bets and pot total",
                        IsSubOption = true, OptionAction = (_) =>
                        {
                            foreach (var spot in SessionManager.PlayerSpots)
                            {
                                if (spot.HasPlayerData)
                                    spot.PlayerData!.MoneyInThePot = 0;
                            }
                            SessionManager.TotalAmountInPot = 0;
                        }
                    },
                ],
            });
            SideMenuOptions.Add(new()
            {
                Id = 4,
                OptionText = "Load...",
                DescriptionText = "Load settings or players",
                HasSubOptions = true,
                SubOptions = [
                    new()
                    {
                        Id = 0,
                        OptionText = "Game settings",
                        DescriptionText = "Load game settings",
                        IsAvailable = !SessionManager.Clock.IsRunning,
                        UnavaliableDescriptionText = "Pause game to load",
                        IsSubOption = true,
                        OptionAction = (_) =>
                        {
                            s_loadSettingsDialog.Title = "Load game settings";
                            if (s_loadSettingsDialog.ShowDialog() == true)
                            {
                                LastSaveLoadStatus = SessionManager.TryLoadGameSettingsFromFile(s_loadSettingsDialog.FileName, out var resultMessage) ?
                                LastSaveLoadResult.Success : (string.IsNullOrEmpty(resultMessage) ? LastSaveLoadResult.None : LastSaveLoadResult.Failure);
                                LastSaveLoadMessage = resultMessage;
                                Task.Run(() =>
                                {
                                    Task.Delay(3000).Wait();
                                    LastSaveLoadStatus = LastSaveLoadResult.None;
                                });
                            }
                        }
                    },
                    new()
                    {
                        Id = 1,
                        OptionText = "Table configuration",
                        DescriptionText = "Load table configuration",
                        IsAvailable = !SessionManager.Clock.IsRunning,
                        UnavaliableDescriptionText = "Pause game to load",
                        IsSubOption = true,
                        OptionAction = (_) =>
                        {
                            s_loadSettingsDialog.Title = "Load table configuration";
                            if (s_loadSettingsDialog.ShowDialog() == true)
                            {
                                LastSaveLoadStatus = SessionManager.TryLoadTableConfigurationFromFile(s_loadSettingsDialog.FileName, out var resultMessage) ?
                                LastSaveLoadResult.Success : (string.IsNullOrEmpty(resultMessage) ? LastSaveLoadResult.None : LastSaveLoadResult.Failure);
                                LastSaveLoadMessage = resultMessage;
                                Task.Run(() =>
                                {
                                    Task.Delay(3000).Wait();
                                    LastSaveLoadStatus = LastSaveLoadResult.None;
                                });
                            }
                        }
                    },
                ]
            });
            SideMenuOptions.Add(new()
            {
                Id = 5,
                OptionText = "Save...",
                DescriptionText = "Save settings or players",
                HasSubOptions = true,
                SubOptions =
                [
                    new()
                    {
                        Id = 0,
                        OptionText = "Game settings",
                        DescriptionText = "Save game settings",
                        IsSubOption = true,
                        OptionAction = (_) =>
                        {
                            s_saveSettingsDialog.Title = "Save game settings";
                            if (s_saveSettingsDialog.ShowDialog() == true)
                            {
                                LastSaveLoadStatus = SessionManager.TrySaveGameSettings(s_saveSettingsDialog.FileName, out var resultMessage) ?
                                LastSaveLoadResult.Success : (string.IsNullOrEmpty(resultMessage) ? LastSaveLoadResult.None : LastSaveLoadResult.Failure);
                                LastSaveLoadMessage = resultMessage;
                                Task.Run(() =>
                                {
                                    Task.Delay(3000).Wait();
                                    LastSaveLoadStatus = LastSaveLoadResult.None;
                                });
                            }
                        }
                    },
                    new()
                    {
                        Id = 1,
                        OptionText = "Table configuration",
                        DescriptionText = "Save table configuration",
                        IsSubOption = true,
                        OptionAction = (_) =>
                        {
                            s_saveSettingsDialog.Title = "Save table configuration";
                            if (s_saveSettingsDialog.ShowDialog() == true)
                            {
                                LastSaveLoadStatus = SessionManager.TrySaveTableConfiguration(s_saveSettingsDialog.FileName, out var resultMessage) ?
                                LastSaveLoadResult.Success : (string.IsNullOrEmpty(resultMessage) ? LastSaveLoadResult.None : LastSaveLoadResult.Failure);
                                LastSaveLoadMessage = resultMessage;
                                Task.Run(() =>
                                {
                                    Task.Delay(3000).Wait();
                                    LastSaveLoadStatus = LastSaveLoadResult.None;
                                });
                            }
                        }
                    },
                ]
            });
            SideMenuOptions.Add(new()
            {
                Id = 6,
                OptionText = "Link Spotify",
                DescriptionText = "Log-in a Spotify user",
                OptionAction = (opt) =>
                {
                    if (_spotifyViewModel.AuthenticationStatus != AuthenticationStatus.Authenticated)
                    {
                        Task.Run(async () =>
                        {
                            await _spotifyViewModel.AuthorizeApplication();
                            if (_spotifyViewModel.AuthenticationStatus == AuthenticationStatus.Authenticated)
                            {
                                await _spotifyViewModel.TrySetUserName();
                                _spotifyViewModel.StartTrackMonitoring();
                            }
                        });
                    }
                    else
                    {
                        // TODO: Unlink Spotify
                    }
                }
            });
            SideMenuOptions.Add(new()
            {
                Id = 7,
                OptionText = "Quit",
                DescriptionText = "Quit PokerTracker3000",
                OptionAction = (_) =>
                {
                    _eventBus.NotifyListeners(GameEventBus.EventType.ApplicationClosing,
                        new ApplicationClosingMessage()
                        {
                            TotalNumberOfClosingCallbacks = _eventBus.GetNumberOfListenersForEvent(GameEventBus.EventType.ApplicationClosing)
                        });
                }
            });
        }

        private void ResetToOutermostMenu()
        {
            while (_currentFocusParentOptionStack.Count > 0)
                ExitSubOptionMenu();
        }

        private void EnterSubOptionMenu(SideMenuOptionModel currentOption)
        {
            currentOption.IsSelected = true;

            _currentFocusParentOptionListStack.Push(_currentFocusOptionList);
            _currentFocusParentOptionStack.Push(currentOption.Id);

            _currentFocusOptionList = currentOption.SubOptions;
            _currentFocusOptionList.First().IsHighlighted = true;
            _currentFocusOptionId = _currentFocusOptionList.First().Id;
        }

        private void ExitSubOptionMenu()
        {
            var currentOption = _currentFocusOptionList.First(x => x.Id == _currentFocusOptionId);

            currentOption.IsHighlighted = false;
            _currentFocusOptionId = _currentFocusParentOptionStack.Pop();
            _currentFocusOptionList = _currentFocusParentOptionListStack.Pop();
            _currentFocusOptionList.First(x => x.Id == _currentFocusOptionId).IsSelected = false;
        }

        private void UpdateOptionAvailability()
        {
            var hasStages = SessionManager.StageManager.Stages.Any();
            SideMenuOptions[_startPauseGameOptionId].IsAvailable = hasStages;
            SideMenuOptions[_gameSettingsOptionId].SubOptions[_resetStageOptionId].IsAvailable = hasStages;
            SideMenuOptions[_gameSettingsOptionId].SubOptions[_resetAllStagesOptionId].IsAvailable = hasStages;

            var currentStage = SessionManager.StageManager.CurrentStage;
            SideMenuOptions[_goToOptionId].SubOptions[_previousStageOptionId].IsAvailable = currentStage != default && SessionManager.StageManager.TryGetStageByNumber(currentStage.Number - 1, out _);
            SideMenuOptions[_goToOptionId].SubOptions[_nextStageOptionId].IsAvailable = currentStage != default && SessionManager.StageManager.TryGetStageByNumber(currentStage.Number + 1, out _);
        }

        private void ApplicationClosing(IInternalMessage m)
        {
            if (m is not ApplicationClosingMessage msg)
                return;

            _spotifyViewModel.Dispose();
            msg.NumberOfClosingCallbacksCalled++;
        }

        #endregion
    }
}
