using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PokerTracker3000.GamepadInput.XInput;

namespace PokerTracker3000.GamepadInput
{
    internal record GamepadState
    {
        public uint Id { get; }

        public bool AButtonPressed { get; }

        public bool BButtonPressed { get; }

        public bool XButtonPressed { get; }

        public bool YButtonPressed { get; }

        public bool LeftShoulderButtonPressed { get; }


        //public static GamepadState Create(uint id, XINPUT_GAMEPAD gamepad)
        //{
        //    // TODO: Map the XInputGamepadDigialButtonFlag to internal enum and come up with something nice
        //}
    }
}
