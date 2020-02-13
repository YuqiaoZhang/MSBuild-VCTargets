namespace Microsoft.Build.CPPTasks
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    internal static class VCTaskNativeMethods
    {
        [DllImport("KERNEL32.DLL", SetLastError = true)]
        internal static extern bool CloseHandle(IntPtr hObject);
        [DllImport("KERNEL32.DLL", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern IntPtr CreateEventW(IntPtr lpEventAttributes, bool bManualReset, bool bInitialState, string lpName);
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern uint SearchPath(string lpPath, string lpFileName, string lpExtension, int nBufferLength, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpBuffer, out IntPtr lpFilePart);
        [DllImport("KERNEL32.DLL", SetLastError = true)]
        internal static extern bool SetEvent(IntPtr hEvent);
    }
}

