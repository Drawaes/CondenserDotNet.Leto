using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace CondenserDotNet.Leto.EphemeralBuffers.Interop.Windows
{
    internal static partial class Interop
    {
        [DllImport(Libraries.Kernel32, SetLastError = true)]
        internal extern static IntPtr VirtualAlloc(IntPtr lpAddress, UIntPtr dwSize, int flAllocationType, int flProtect);
        [DllImport(Libraries.Kernel32, SetLastError = true)]
        internal extern static bool VirtualFree(IntPtr lpAddress, UIntPtr dwSize, int dwFreeType);
        [DllImport(Libraries.Kernel32, SetLastError = true)]
        internal extern static bool VirtualLock(IntPtr lpAddress, UIntPtr dwSize);
    }
}
