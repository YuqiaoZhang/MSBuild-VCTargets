namespace Microsoft.Build.CPPTasks
{
    using Microsoft.Build.Shared;
    using System;
    using System.Threading;

    internal static class VCTaskNativeMethods
    {
        internal static bool CloseHandle(Semaphore hObject)
        {
            hObject.Close();
            return true;
        }

        internal static Semaphore CreateEventW(IntPtr lpEventAttributes, bool bManualReset, bool bInitialState, string lpName)
        {
            ErrorUtilities.VerifyThrow(
                                     lpEventAttributes == IntPtr.Zero && bManualReset == false,
                                     "This method should only be passed IntPtr.Zero and false, but was passed {0} and {1} instead!",
                                     lpEventAttributes,
                                     bManualReset
                                     );

            return CreateEventW(bInitialState, lpName);
        }

        internal static Semaphore CreateEventW(bool bInitialState, string lpName)
        {
            return new Semaphore(bInitialState ? 1 : 0, 1, lpName);
        }

#if false
        internal static extern uint SearchPath(string lpPath, string lpFileName, string lpExtension, int nBufferLength, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpBuffer, out IntPtr lpFilePart);
#endif

        internal static bool SetEvent(Semaphore hEvent)
        {
            hEvent.Release();
            return true;
        }
    }
}

