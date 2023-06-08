using System.Runtime.InteropServices;
using System.Security;
using PokerTracker3000.GamepadInput.XInput.Enumerations;

namespace PokerTracker3000.GamepadInput.XInput
{
    // This attribute is applied for performance reasons, see https://learn.microsoft.com/en-us/dotnet/api/system.security.suppressunmanagedcodesecurityattribute?view=net-8.0
    [SuppressUnmanagedCodeSecurity]
    internal partial class ImportedMethods
    {
        private const string XInputDllName = "Xinput1_4.dll";

        /// <summary>
        /// Retrieves the current state of the specified controller.
        /// </summary>
        /// <param name="dwUserIndex">Index of the user's controller. 
        /// Can be a value from 0 to 3.</param>
        /// <param name="pState">Reference to an <see cref="XINPUT_STATE"/> 
        /// structure that receives the current state of the controller.</param>
        /// <returns>
        /// If the function succeeds, the return value is 
        /// <see cref="Win32SystemErrorCodes.ERROR_SUCCESS"/>.
        /// If the controller is not connected, the return value is 
        /// <see cref="Win32SystemErrorCodes.ERROR_DEVICE_NOT_CONNECTED"/>.
        /// If the function fails, the return value is an error code defined in Winerror.h. 
        /// The function does not use SetLastError to set the calling thread's last-error code.
        /// </returns>
        /// <remarks>
        /// When <see cref="XInputGetState(uint, ref XINPUT_STATE)"/> is used to retrieve 
        /// controller data, the left and right triggers are each reported separately. 
        /// For legacy reasons, when DirectInput retrieves controller data, the two triggers 
        /// share the same axis. The legacy behavior is noticeable in the current 
        /// Game Device Control Panel, which uses DirectInput for controller state.
        /// </remarks>
        [LibraryImport(XInputDllName, EntryPoint = "XInputGetState", SetLastError = false)]
        internal static partial uint XInputGetState(uint dwUserIndex, ref XINPUT_STATE pState);

        /// <summary>
        /// Retrieves the capabilities and features of a connected controller.
        /// </summary>
        /// <param name="dwUserIndex">Index of the user's controller. Can be a value in the range 0–3.</param>
        /// <param name="dwFlags">Input flags that identify the controller type.
        /// If this value is 0, then the capabilities of all controllers connected to the system are returned.
        /// See <seealso cref="XInputControllerTypeFlag"/> for supported values.</param>
        /// <param name="pCapabilities">Reference to an <see cref="XINPUT_CAPABILITIES"/> 
        /// structure that receives controller capabilities.</param>
        /// <returns>
        /// If the function succeeds, the return value is 
        /// <see cref="Win32SystemErrorCodes.ERROR_SUCCESS"/>.
        /// If the controller is not connected, the return value is 
        /// <see cref="Win32SystemErrorCodes.ERROR_DEVICE_NOT_CONNECTED"/>.
        /// If the function fails, the return value is an error code defined in Winerror.h. 
        /// The function does not use SetLastError to set the calling thread's last-error code.
        /// </returns>
        [LibraryImport(XInputDllName, EntryPoint = "XInputGetCapabilities", SetLastError = false)]
        internal static partial uint XInputGetCapabilities(uint dwUserIndex, uint dwFlags, ref XINPUT_CAPABILITIES pCapabilities);

        /// <summary>
        /// Retrieves the battery type and charge status of a wireless controller.
        /// </summary>
        /// <param name="dwUserIndex">Index of the signed-in gamer associated with the device.
        /// Can be a value in the range 0–XUSER_MAX_COUNT − 1.</param>
        /// <param name="devType">Specifies which device associated with this user index should be queried.
        /// Must be BATTERY_DEVTYPE_GAMEPAD or BATTERY_DEVTYPE_HEADSET.</param>
        /// <returns>
        /// If the function succeeds, the return value is 
        /// <see cref="Win32SystemErrorCodes.ERROR_SUCCESS"/>.
        /// </returns>
        [LibraryImport(XInputDllName, EntryPoint = "XInputGetBatteryInformation", SetLastError = false)]
        internal static partial uint XInputGetBatteryInformation(uint dwUserIndex, byte devType, ref XINPUT_BATTERY_INFORMATION pBatteryInformation);

        /// <summary>
        /// Sends data to a connected controller. This function is used 
        /// to activate the vibration function of a controller.
        /// </summary>
        /// <param name="dwUserIndex">Index of the user's controller. 
        /// Can be a value from 0 to 3.</param>
        /// <param name="pVibration">Reference to an <see cref="XINPUT_VIBRATION"/> 
        /// structure containing the vibration information to send to the controller.</param>
        /// <returns>
        /// If the function succeeds, the return value is 
        /// <see cref="Win32ErrorCodes.ERROR_SUCCESS"/>.
        /// If the controller is not connected, the return value is 
        /// <see cref="Win32ErrorCodes.ERROR_DEVICE_NOT_CONNECTED"/>.
        /// If the function fails, the return value is an error code defined in Winerror.h. 
        /// The function does not use SetLastError to set the calling thread's last-error code.
        /// </returns>
        [LibraryImport(XInputDllName, EntryPoint = "XInputSetState", SetLastError = false)]
        internal static partial uint XInputSetState(uint dwUserIndex, ref XINPUT_VIBRATION pVibration);
    }
}
