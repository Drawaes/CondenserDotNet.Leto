using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace CondenserDotNet.Leto.EphemeralBuffers.Interop.Unix
{
    internal static partial class Interop
    {
        [DllImport(Libraries.LibC, EntryPoint = "mlock")]
        internal unsafe static extern int MLock(IntPtr addr, long len);

        [DllImport(Libraries.LibC, EntryPoint = "munlock")]
        internal unsafe static extern int MUnlock(IntPtr addr, long len);
    }
}
