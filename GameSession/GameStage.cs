using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PokerTracker3000.GameSession
{
    public class GameStage : ObservableObject
    {
        #region Public properties

        #region Private fields
        private int _number;
        private bool _pause;
        private decimal _smallBlind;
        private decimal _bigBlind;
        private TimeSpan _length;
        #endregion

        public int Number
        {
            get => _number;
            set => SetProperty(ref _number, value);
        }

        public bool IsPause
        {
            get => _pause;
            set => SetProperty(ref _pause, value);
        }

        public decimal SmallBlind
        {
            get => _smallBlind;
            set => SetProperty(ref _smallBlind, value);
        }

        public decimal BigBlind
        {
            get => _bigBlind;
            set => SetProperty(ref _bigBlind, value);
        }

        public TimeSpan Length
        {
            get => _length;
            set => SetProperty(ref _length, value);
        }
        public string Name => $"Stage {Number}";
        #endregion
    }
}
