using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

using InputEvent = PokerTracker3000.Input.InputManager.UserInputEvent;

namespace PokerTracker3000.GameSession
{
    public class SideMenuViewModel : ObservableObject
    {
        public enum GameEditOption
        {
            None,
            GameStages,
            ChangeDefaultAddOnAmount,
            ChangeDefaultBuyInAmount
        };

        #region Public properties

        #region Private fields
        private bool _isOpen = false;
        private int _currentStageNumber = 0;
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

        public GameSessionManager SessionManager { get; }

        public List<SideMenuOptionModel> SideMenuOptions { get; }
        #endregion

        #region Private fields
        private readonly MainWindowFocusManager _focusManager;
        private int _currentFocusOptionId = 0;
        private readonly Stack<int> _currentFocusParentOptionStack;
        private readonly Stack<List<SideMenuOptionModel>> _currentFocusParentOptionListStack;
        private readonly List<(SideMenuOptionModel target, Action<SideMenuOptionModel> action)> _onOpenCallbacks;
        private List<SideMenuOptionModel> _currentFocusOptionList;

        private readonly int _startPauseGameOptionId;
        private readonly int _goToOptionId;
        private readonly int _previousStageOptionId;
        private readonly int _nextStageOptionId;
        private readonly int _gameSettingsOptionId;
        private readonly int _editStagesOptionId;
        private readonly int _resetStageOptionId;
        private readonly int _resetAllStagesOptionId;
        #endregion

        public SideMenuViewModel(MainWindowFocusManager focusManager, GameSessionManager sessionManager)
        {
            _focusManager = focusManager;
            _onOpenCallbacks = [];
            _currentFocusParentOptionStack = new();
            _currentFocusParentOptionListStack = new();

            SessionManager = sessionManager;
            SideMenuOptions = [];
            InitializeOptionsList();

            _startPauseGameOptionId = SideMenuOptions.First(x => x.OptionText.Equals("Start game", StringComparison.InvariantCulture)).Id;
            _goToOptionId = SideMenuOptions.First(x => x.OptionText.Equals("Go to...", StringComparison.InvariantCulture)).Id;
            _gameSettingsOptionId = SideMenuOptions.First(x => x.OptionText.Equals("Game settings...", StringComparison.InvariantCulture)).Id;
            _previousStageOptionId = SideMenuOptions[_goToOptionId].SubOptions.First(x => x.OptionText.Equals("Previous stage", StringComparison.InvariantCulture)).Id;
            _nextStageOptionId = SideMenuOptions[_goToOptionId].SubOptions.First(x => x.OptionText.Equals("Next stage", StringComparison.InvariantCulture)).Id;
            _editStagesOptionId = SideMenuOptions[_gameSettingsOptionId].SubOptions.First(x => x.OptionText.Equals("Edit stages", StringComparison.InvariantCulture)).Id;
            _resetStageOptionId = SideMenuOptions[_gameSettingsOptionId].SubOptions.First(x => x.OptionText.Equals("Reset current stage", StringComparison.InvariantCulture)).Id;
            _resetAllStagesOptionId = SideMenuOptions[_gameSettingsOptionId].SubOptions.First(x => x.OptionText.Equals("Reset all stages", StringComparison.InvariantCulture)).Id;

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
                        IsSubOption = true,
                        OptionAction = (SideMenuOptionModel opt) =>
                    {
                        SessionManager.StageManager.TryGotoPreviousStage();
                    }},
                    new() { Id = 2, OptionText = "Stage...", IsSubOption = true, IsAvailable = false, UnavaliableDescriptionText = "Not yet implemented" }
                ],
            });

            SideMenuOptionModel addPlayerOption = new()
            {
                Id = 2,
                OptionText = "Add player",
                DescriptionText = "Add a new player to an empty spot",
                OnOpenAction = (SideMenuOptionModel opt) => { opt.IsAvailable = !SessionManager.TableFull; },
                OptionAction = (SideMenuOptionModel opt) =>
                {
                    SessionManager.AddPlayerToSpot();
                    opt.IsAvailable = !SessionManager.TableFull;
                },
                UnavaliableDescriptionText = "Cannot add new player - table full"
            };
            _onOpenCallbacks.Add((addPlayerOption, addPlayerOption.OnOpenAction));
            SideMenuOptions.Add(addPlayerOption);

            SideMenuOptions.Add(new()
            {
                Id = 3,
                OptionText = "Remove empty spots",
                DescriptionText = "Removes all empty slots",
                OptionAction = (_) => SessionManager.ConsolidateLayout(),
            });
            SideMenuOptions.Add(new()
            {
                Id = 4,
                OptionText = "Game settings...",
                DescriptionText = "Change settings for the current game",
                HasSubOptions = true,
                SubOptions =
                [
                    new() { Id = 0, OptionText = "Edit poker chips", IsSubOption = true, IsAvailable = false, UnavaliableDescriptionText = "Not yet implemented" },
                    new()
                    {
                        Id = 1,
                        OptionText = "Edit stages",
                        IsAvailable = !SessionManager.Clock.IsRunning,
                        UnavaliableDescriptionText = "Pause game to edit",
                        IsSubOption = true,
                        OptionAction = (_) =>
                        {
                            SessionManager.CurrentGameEditOption = GameEditOption.GameStages;
                            _focusManager.SideMenuEditOptionSelected();
                        }
                    },
                    new()
                    {
                        Id = 2,
                        OptionText = "Reset current stage",
                        IsAvailable = SessionManager.StageManager.CurrentStage != default,
                        UnavaliableDescriptionText = "No stages",
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
                        Id = 3,
                        OptionText = "Reset all stages",
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
                    new() { Id = 4, OptionText = "Pay-out ratios", IsSubOption = true, IsAvailable = false, UnavaliableDescriptionText = "Not yet implemented" },
                    new()
                    {
                        Id = 5,
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
                        Id = 6,
                        OptionText = "Buy-in amount",
                        IsSubOption = true,
                        DescriptionText = "Change default buy-in amount",
                        OptionAction = (_) =>
                        {
                            SessionManager.CurrentGameEditOption = GameEditOption.ChangeDefaultBuyInAmount;
                            _focusManager.SideMenuEditOptionSelected();
                        }
                    },
                    new() { Id = 7, OptionText = "Stage length", IsSubOption = true, IsAvailable = false, UnavaliableDescriptionText = "Not yet implemented" },
                    new() { Id = 8, OptionText = "Reset all amounts", IsSubOption = true, IsAvailable = false, UnavaliableDescriptionText = "Not yet implemented" }
                ],
            });
            SideMenuOptions.Add(new()
            {
                Id = 5,
                OptionText = "Program settings...",
                DescriptionText = "Change program settings",
                HasSubOptions = true,
                SubOptions =
                [
                    new() { Id = 0, OptionText = "Player image", IsSubOption = true }
                ],
            });
            SideMenuOptions.Add(new()
            {
                Id = 6,
                OptionText = "Quit",
                DescriptionText = "Quit PokerTracker3000",
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
        #endregion
    }
}
