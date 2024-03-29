using System;
using System.Collections.Generic;
using System.Threading;
using PokerTracker3000.GamepadInput.XInput;

namespace PokerTracker3000.GamepadInput
{
    public class GamepadManager : IDisposable
    {
        #region Events
        public event EventHandler<Gamepad>? GamepadConnected;
        #endregion

        #region Private fields
        private readonly Dictionary<uint, Gamepad> _activeGamepads = [];
        private readonly Timer _checkForNewGamepadTimer;
        private const int ControllerCheckIntervalMs = 1000;
        private const uint MaxNumberOfControllers = 4;
        private bool _disposedValue;
        #endregion

        public GamepadManager()
        {
            _checkForNewGamepadTimer = new((s) => CheckForNewActiveControllers(), default, ControllerCheckIntervalMs, Timeout.Infinite);
        }

        private void CheckForNewActiveControllers()
        {
            List<uint> newControllers = [];
            XINPUT_STATE state = new();
            for (uint i = 0; i < MaxNumberOfControllers; i++)
            {
                bool controllerIsActive;
                lock (_activeGamepads)
                    controllerIsActive = _activeGamepads.ContainsKey(i);

                if (controllerIsActive)
                    continue;

                if (ImportedMethods.XInputGetState(i, ref state).ToErrorCode() == XInput.Enumerations.Win32SystemErrorCodes.ERROR_SUCCESS)
                {
                    if (Gamepad.TryCreate(i, out var newGamepad))
                    {
                        newGamepad!.GamepadDisconnected += GamepadDisconnected;
                        lock (_activeGamepads)
                            _activeGamepads.Add(i, newGamepad);
                        newControllers.Add(i);
                    }
                }
            }

            if (newControllers.Count != 0)
            {
                lock (_activeGamepads)
                {
                    foreach (var id in newControllers)
                        GamepadConnected?.Invoke(default, _activeGamepads[id]);
                }
            }

            _checkForNewGamepadTimer.Change(ControllerCheckIntervalMs, Timeout.Infinite);
        }

        private void GamepadDisconnected(object? sender, EventArgs e)
        {
            if (sender is not Gamepad gamepad)
                return;

            gamepad.GamepadDisconnected -= GamepadDisconnected;
            lock (_activeGamepads)
            {
                gamepad.Dispose();
                _activeGamepads.Remove(gamepad.Id);
            }

        }

        #region Disposal
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _checkForNewGamepadTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    _checkForNewGamepadTimer.Dispose();

                    lock (_activeGamepads)
                    {
                        foreach (var gamepad in _activeGamepads)
                            gamepad.Value.Dispose();
                        _activeGamepads.Clear();
                    }
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
