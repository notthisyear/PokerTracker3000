using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;

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
        private int _lengthSeconds;
        private int _lengthSecondsRemaining;
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

        public int LengthSeconds
        {
            get => _lengthSeconds;
            set => SetProperty(ref _lengthSeconds, value);
        }

        [JsonIgnore]
        public int LengthSecondsRemaining
        {
            get => _lengthSecondsRemaining;
            set => SetProperty(ref _lengthSecondsRemaining, value);
        }

        public void ResetStage()
        {
            LengthSecondsRemaining = LengthSeconds;
        }

        [JsonIgnore]
        public string Name => $"Stage {Number}";
        #endregion
    }
}
