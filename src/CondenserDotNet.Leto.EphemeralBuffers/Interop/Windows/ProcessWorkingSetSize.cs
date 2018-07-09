using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace CondenserDotNet.Leto.EphemeralBuffers.Interop.Windows
{
    internal static partial class Interop
    {
        [DllImport(Libraries.Kernel32)]
        internal static extern bool GetProcessWorkingSetSize(IntPtr hProcess, out IntPtr lpMinimumWorkingSetSize, out IntPtr lpMaximumWorkingSetSize);

        [DllImport(Libraries.Kernel32)]
        internal static extern bool SetProcessWorkingSetSize(IntPtr hProcess, IntPtr dwMinimumWorkingSetSize, IntPtr dwMaximumWorkingSetSize);
    }
}
