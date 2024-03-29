using System;
using System.Collections.Generic;
using PokerTracker3000.GamepadInput.XInput.Enumerations;

namespace PokerTracker3000.GamepadInput.XInput
{
    internal static class ExtensionMethods
    {
        public static Win32SystemErrorCodes ToErrorCode(this uint number)
            => !Enum.IsDefined(typeof(Win32SystemErrorCodes), number) ? Win32SystemErrorCodes.Undefined : (Win32SystemErrorCodes)number;

        public static XInputControllerType ToControllerType(this byte number)
             => !Enum.IsDefined(typeof(XInputControllerType), number) ? XInputControllerType.Undefined : (XInputControllerType)number;

        public static XInputControllerSubtype ToControllerSubtype(this byte number)
            => !Enum.IsDefined(typeof(XInputControllerSubtype), number) ? XInputControllerSubtype.Undefined : (XInputControllerSubtype)number;

        public static BatteryType ToBatteryType(this byte number)
           => !Enum.IsDefined(typeof(BatteryType), number) ? BatteryType.BATTERY_TYPE_UNKNOWN : (BatteryType)number;

        public static BatteryLevel ToBatteryLevel(this byte number)
           => !Enum.IsDefined(typeof(BatteryLevel), number) ? BatteryLevel.Undefined : (BatteryLevel)number;

        public static List<XInputControllerFeatureFlag> GetControllerFeatureFlags(this ushort number)
        {
            var flags = new List<XInputControllerFeatureFlag>();
            var numberAsEnum = (XInputControllerFeatureFlag)number;
            foreach (var f in Enum.GetValues<XInputControllerFeatureFlag>())
            {
                if (numberAsEnum.HasFlag(f))
                    flags.Add(f);
            }
            return flags;
        }

        public static uint ToUnsignedInteger(this XInputControllerTypeFlag flag)
            => (uint)flag;

        public static byte ToUnsignedByte(this XInputBatteryDeviceType type)
            => (byte)type;
    }
}
