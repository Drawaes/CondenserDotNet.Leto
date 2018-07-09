using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace CondenserDotNet.Leto.EphemeralBuffers.Interop.Unix
{
    internal static partial class Interop
    {
        [Flags]
        internal enum MemoryMappedProtections
        {
            PROT_NONE = 0x0,
            PROT_READ = 0x1,
            PROT_WRITE = 0x2,
            PROT_EXEC = 0x4
        }

        [Flags]
        internal enum MemoryMappedFlags
        {
            MAP_SHARED = 0x01,
            MAP_PRIVATE = 0x02,
            MAP_ANONYMOUS = 0x20,
        }

        [DllImport(Libraries.LibC, SetLastError = true, EntryPoint = "mmap")]
        internal unsafe static extern IntPtr MMap(IntPtr addr, long length, MemoryMappedProtections prot, MemoryMappedFlags flags, int fd, UIntPtr offset);

        [DllImport(Libraries.LibC, SetLastError = true, EntryPoint = "munmap")]
        internal static extern int MUnmap(IntPtr addr, long len);
    }
}
