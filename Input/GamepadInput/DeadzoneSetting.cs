namespace PokerTracker3000.Input.GamepadInput
{
    internal record DeadzoneSetting
    {
        #region Public properties

        #region Backing fields
        private float _positiveXPercent = 0.0f;
        private float _negativeXPercent = 0.0f;
        private float _positiveYPercent = 0.0f;
        private float _negativeYPercent = 0.0f;
        #endregion

        public float PositiveXPercent
        {
            get => _positiveXPercent;
            set
            {
                if (value >= 0 && value <= 1)
                    _positiveX = (short)(value * short.MaxValue);
                _positiveXPercent = value;
            }
        }

        public float NegativeXPercent
        {
            get => _negativeXPercent;
            set
            {
                if (value >= 0 && value <= 1)
                    _negativeX = (short)(value * short.MinValue);
                _negativeXPercent = value;
            }
        }

        public float PositiveYPercent
        {
            get => _positiveYPercent;
            set
            {
                if (value >= 0 && value <= 1)
                    _positiveY = (short)(value * short.MaxValue);
                _positiveYPercent = value;
            }
        }

        public float NegativeYPercent
        {
            get => _negativeYPercent;
            set
            {
                if (value >= 0 && value <= 1)
                    _negativeY = (short)(value * short.MinValue);
                _negativeYPercent = value;
            }
        }
        #endregion

        #region Private fields
        private short _positiveX;
        private short _negativeX;
        private short _positiveY;
        private short _negativeY;
        #endregion

        public bool IsXWithinDeadzone(short x)
            => x >= _negativeX && x <= _positiveX;

        public bool IsYWithinDeadzone(short y)
            => y >= _negativeY && y <= _positiveY;
    }
}
