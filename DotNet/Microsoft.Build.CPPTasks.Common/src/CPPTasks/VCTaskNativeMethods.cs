namespace Microsoft.Build.CPPTasks
{
    using Microsoft.Build.Shared;
    using Microsoft.Build.Shared.Extension;
    using System;
    using System.Threading;

    internal static class VCTaskNativeMethods
    {
        internal static bool CloseHandle(EventWaitHandle hEvent)
        {
            hEvent.Close();
            return true;
        }

        internal static EventWaitHandle CreateEventW(IntPtr lpEventAttributes, bool bManualReset, bool bInitialState, string lpName)
        {
            ErrorUtilities.VerifyThrow(
                                     lpEventAttributes == IntPtr.Zero,
                                     "This method should only be passed IntPtr.Zero, but was passed {0} instead!",
                                     lpEventAttributes
                                     );
            //FiX me:
            //UNIX not support named event
            return new EventWaitHandle(bInitialState, (!bManualReset) ? EventResetMode.AutoReset : EventResetMode.ManualReset, null);
        }

#if false
        internal static extern uint SearchPath(string lpPath, string lpFileName, string lpExtension, int nBufferLength, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpBuffer, out IntPtr lpFilePart);
#endif

        internal static bool SetEvent(EventWaitHandle hEvent)
        {
            hEvent.Set();
            return true;
        }
    }
}

