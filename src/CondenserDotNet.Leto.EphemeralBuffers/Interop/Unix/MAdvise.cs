using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace CondenserDotNet.Leto.EphemeralBuffers.Interop.Unix
{
    internal static partial class Interop
    {
        [DllImport(Libraries.LibC, EntryPoint = "madvise")]
        internal static extern int MAdvise(IntPtr ptr, long length, Advice advice);

        public enum Advice : int
        {
            MADV_DONTDUMP = 16,
            MADV_DODUMP = 17,
        }
    }
}
