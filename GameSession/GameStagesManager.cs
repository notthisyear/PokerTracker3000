﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using PokerTracker3000.Common.Messages;
using PokerTracker3000.Common;
using PokerTracker3000.Interfaces;

namespace PokerTracker3000.GameSession
{
    public class GameStagesManager : ObservableObject
    {
        #region Public properties

        #region Backing fields
        private GameStage? _currentStage = default;
        private bool _hasPauseStage = false;
        private int _secondsUntilPause = 0;
        private int _secondsUntilEnd = 0;
        private bool _onLastStage = false;
        #endregion

        public GameStage? CurrentStage
        {
            get => _currentStage;
            private set => SetProperty(ref _currentStage, value);
        }

        public bool HasPauseStage
        {
            get => _hasPauseStage;
            private set => SetProperty(ref _hasPauseStage, value);
        }

        public int SecondsUntilPause
        {
            get => _secondsUntilPause;
            private set => SetProperty(ref _secondsUntilPause, value);
        }

        public int SecondsUntilEnd
        {
            get => _secondsUntilEnd;
            private set => SetProperty(ref _secondsUntilEnd, value);
        }

        public bool OnLastStage
        {
            get => _onLastStage;
            private set => SetProperty(ref _onLastStage, value);
        }

        public ObservableCollection<string> Stages { get; }
        #endregion

        #region Events
        public event EventHandler? StageAdded;
        public event EventHandler? StageRemoved;
        public event EventHandler<GameStage>? CurrentStageChanged;
        public event EventHandler? AllStagesDone;
        #endregion

        #region Private fields
        private int _secondsInOtherStagesUntilPause;
        private int _secondsInOtherStagesUntilEnd;
        private readonly IGameEventBus _eventBus;
        private readonly GameClock _clock;
        private readonly GameSettings _settings;
        private readonly List<GameStage> _stages;
        private readonly object _stagesAccessLock = new();
        #endregion

        public GameStagesManager(IGameEventBus eventBus, GameClock clock, GameSettings settings)
        {
            Stages = [];
            _stages = [];
            BindingOperations.EnableCollectionSynchronization(Stages, _stagesAccessLock);

            _eventBus = eventBus;
            _clock = clock;
            _settings = settings;

            clock.RegisterCallbackOnTick(() =>
            {
                if (CurrentStage != default)
                    CurrentStage.LengthSecondsRemaining--;
            });

            _clock.RegisterCallbackOnSecondsLeft(0, (clock) =>
            {
                if (CurrentStage == default)
                    return;

                if (!TryGetNextStage(CurrentStage, out var nextStage))
                {
                    OnLastStage = false;
                    _eventBus.NotifyListeners(GameEventBus.EventType.GameDone, new GameEventMessage());
                    AllStagesDone?.Invoke(this, EventArgs.Empty);
                    return;
                }
                ChangeStage(nextStage!);
            });
        }

        #region Public methods
        public void AddStage(int number = -1, bool isPause = false, decimal smallBlind = -1, decimal bigBlind = -1, int stageLengthSeconds = -1)
        {
            number = number == -1 ? _stages.Last().Number + 1 : number;
            smallBlind = smallBlind == -1 ? _stages.Last().SmallBlind * 2 : smallBlind;
            bigBlind = bigBlind == -1 ? smallBlind * 2 : bigBlind;
            stageLengthSeconds = stageLengthSeconds == -1 ? _settings.DefaultStageLengthSeconds : stageLengthSeconds;

            _stages.Add(new()
            {
                Number = number,
                IsPause = isPause,
                SmallBlind = smallBlind,
                BigBlind = bigBlind,
                LengthSeconds = stageLengthSeconds,
                LengthSecondsRemaining = stageLengthSeconds
            });

            _stages.Last().PropertyChanged += StagePropertyChanged;
            lock (Stages)
                Stages.Add(_stages.Last().Name);

            if (_stages.Count == 1)
                ChangeStage(_stages[0]);
            else if (CurrentStage != default)
                CalculateTimeUntilPauseAndEnd(CurrentStage);

            OnLastStage = CurrentStage != default && CurrentStage.Number == Stages.Count;
            StageAdded?.Invoke(this, EventArgs.Empty);
        }

        public void RemoveStage(int number)
        {
            var stageToRemove = _stages.FirstOrDefault(x => x.Number == number);
            if (stageToRemove == default)
                return;

            var indexToRemove = _stages.IndexOf(stageToRemove);
            _stages.Remove(stageToRemove);

            lock (Stages)
            {
                Stages.RemoveAt(indexToRemove);
                for (var i = 0; i < _stages.Count; i++)
                {
                    _stages[i].Number = i + 1;
                    Stages[i] = _stages[i].Name;
                }
            }

            if (_stages.Count == 0)
            {
                CurrentStage = default;
                SecondsUntilPause = 0;
                SecondsUntilEnd = 0;
            }
            else if (CurrentStage != default && stageToRemove.Number == CurrentStage.Number)
            {
                // If the deleted stage is the selected one, we have to reselect the
                // same index again so that the CurrentStage property updates
                CurrentStage = _stages[Math.Min(indexToRemove, _stages.Count - 1)];
            }

            if (CurrentStage != default)
                CalculateTimeUntilPauseAndEnd(CurrentStage);

            OnLastStage = CurrentStage != default && CurrentStage.Number == Stages.Count;
            StageRemoved?.Invoke(this, EventArgs.Empty);
        }

        public bool TryGetStageByIndex(int index, out GameStage? stage)
        {
            var stageName = string.Empty;
            lock (Stages)
                stageName = index >= 0 && index < Stages.Count ? Stages[index] : string.Empty;

            stage = default;
            if (string.IsNullOrEmpty(stageName))
                return false;

            stage = _stages.FirstOrDefault(x => x.Name.Equals(stageName, StringComparison.InvariantCulture));
            return stage != default;
        }

        public bool TryGetStageByNumber(int number, out GameStage? stage)
            => TryGetStageByIndex(GetIndexForNumber(number), out stage);

        public bool TryGetNextStage(GameStage current, out GameStage? nextStage)
            => TryGetStageByNumber(current.Number + 1, out nextStage);

        public void TryGotoNextStage()
        {
            if (CurrentStage != default && TryGetNextStage(CurrentStage, out var nextStage))
                ChangeStage(nextStage!);
        }

        public void TryGotoPreviousStage()
        {
            if (CurrentStage != default && TryGetStageByNumber(CurrentStage.Number - 1, out var previousStage))
                ChangeStage(previousStage!);
        }

        public void ResetCurrentStage()
        {
            if (CurrentStage != default)
            {
                CurrentStage.ResetStage();
                _clock.UpdateNumberOfSeconds(CurrentStage.LengthSecondsRemaining);
            }
        }

        public void ResetAllStages()
        {
            foreach (var stage in _stages)
                stage.ResetStage();

            if (CurrentStage != default)
                _clock.UpdateNumberOfSeconds(CurrentStage.LengthSecondsRemaining);
        }

        public void SetStages(List<GameStage> stages)
        {
            while (_stages.Count > 0)
                RemoveStage(1);

            foreach (var stage in stages)
                AddStage(stage.Number, stage.IsPause, stage.SmallBlind, stage.BigBlind, stage.LengthSeconds);
        }

        public static int GetIndexForNumber(int number)
            => number - 1;
        #endregion

        #region Private methods
        private void ChangeStage(GameStage newStage)
        {
            CurrentStage = newStage!;
            CalculateTimeUntilPauseAndEnd(CurrentStage);
            OnLastStage = CurrentStage.Number == Stages.Count;
            _clock.UpdateNumberOfSeconds(CurrentStage.LengthSecondsRemaining);

            if (_clock.IsRunning)
                _eventBus.NotifyListeners(GameEventBus.EventType.StageChanged, StageChangedMessage.Create(CurrentStage));

            CurrentStageChanged?.Invoke(this, CurrentStage);
        }

        private void StagePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (CurrentStage == default)
                return;

            if (e.PropertyName?.Equals(nameof(CurrentStage.IsPause), StringComparison.InvariantCulture) ?? false)
            {
                CalculateTimeUntilPauseAndEnd(CurrentStage);
            }
            else if (e.PropertyName?.Equals(nameof(CurrentStage.LengthSeconds), StringComparison.InvariantCulture) ?? false)
            {
                if (sender is GameStage stage)
                {
                    stage.ResetStage();
                    _clock.UpdateNumberOfSeconds(stage.LengthSecondsRemaining);
                }
                CalculateTimeUntilPauseAndEnd(CurrentStage);
            }
            else if (e.PropertyName?.Equals(nameof(CurrentStage.LengthSecondsRemaining), StringComparison.InvariantCulture) ?? false)
            {
                UpdateTimeRemaining(CurrentStage);
            }
        }

        private void CalculateTimeUntilPauseAndEnd(GameStage currentStage)
        {
            _secondsInOtherStagesUntilEnd = _stages.Where(x => x.Number > currentStage.Number).Sum(x => x.LengthSecondsRemaining);

            var nextPauseStage = _stages.FirstOrDefault(x => x.Number > currentStage.Number && x.IsPause && x.LengthSecondsRemaining > 0);
            HasPauseStage = nextPauseStage != default;

            if (HasPauseStage)
                _secondsInOtherStagesUntilPause = _stages.Where(x => x.Number > currentStage.Number && x.Number < nextPauseStage!.Number).Sum(x => x.LengthSecondsRemaining);
            UpdateTimeRemaining(currentStage);
        }

        private void UpdateTimeRemaining(GameStage currentStage)
        {
            SecondsUntilPause = HasPauseStage ? _secondsInOtherStagesUntilPause + currentStage.LengthSecondsRemaining : 0;
            SecondsUntilEnd = _secondsInOtherStagesUntilEnd + currentStage.LengthSecondsRemaining;
        }
        #endregion
    }
}
