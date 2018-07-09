using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace CondenserDotNet.Leto.EphemeralBuffers.Interop.Unix
{
    internal static partial class Interop
    {
        internal enum SysConfName
        {
            _SC_CLK_TCK = 1,
            _SC_PAGESIZE = 2,
            _SC_NPROCESSORS_ONLN = 3,
        }

        [DllImport(Libraries.LibC, EntryPoint = "sysconf")]
        internal static extern IntPtr SysConf(SysConfName name);
    }
}
