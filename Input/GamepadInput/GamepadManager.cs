using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using PokerTracker3000.GamepadInput.XInput;

namespace PokerTracker3000.GamepadInput
{
    internal class GamepadManager
    {
        #region Events
        public event EventHandler<Gamepad>? GamepadConnected;
        #endregion

        #region Private fields
        private readonly Dictionary<uint, Gamepad> _activeGamepads = new();
        private readonly Thread _gamepadWatcherThread;

        private const int ControllerCheckIntervalMs = 1000;
        private const uint MaxNumberOfControllers = 4;
        #endregion

        public GamepadManager()
        {
            _gamepadWatcherThread = new(CheckForNewActiveControllers)
            {
                IsBackground = true
            };
            _gamepadWatcherThread.Start();
        }

        private void CheckForNewActiveControllers()
        {
            List<uint> newControllers = new();
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
                    var newGamepad = new Gamepad(i);
                    newGamepad.GamepadDisconnected += GamepadDisconnected;
                    lock (_activeGamepads) 
                        _activeGamepads.Add(i, newGamepad);
                    newControllers.Add(i);
                }
            }

            if (newControllers.Any())
            {
                lock (_activeGamepads)
                {
                    foreach (var id in newControllers)
                        GamepadConnected?.Invoke(default, _activeGamepads[id]);
                }
            }

            Thread.Sleep(ControllerCheckIntervalMs);
            CheckForNewActiveControllers();
        }

        private void GamepadDisconnected(object? sender, EventArgs e)
        {
            if (sender is not Gamepad gamepad)
                return;

            gamepad.GamepadDisconnected -= GamepadDisconnected;
            lock (_activeGamepads)
                _activeGamepads.Remove(gamepad.Id);
        }
    }
}
