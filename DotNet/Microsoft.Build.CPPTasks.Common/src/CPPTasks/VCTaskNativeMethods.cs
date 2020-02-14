namespace Microsoft.Build.CPPTasks
{
    using Microsoft.Build.Shared;
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

            return CreateEventW(bManualReset, bInitialState, lpName);
        }

        internal static EventWaitHandle CreateEventW(bool bManualReset, bool bInitialState, string lpName)
        {
            return new EventWaitHandle(bInitialState, (!bManualReset) ? EventResetMode.AutoReset : EventResetMode.ManualReset, lpName);
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

