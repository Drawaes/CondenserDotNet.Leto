using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using static CondenserDotNet.Leto.EphemeralBuffers.Interop.Unix.Interop;

namespace CondenserDotNet.Leto.EphemeralBuffers
{
    public sealed partial class EphemeralMemoryPool : MemoryPool<byte>
    {
        protected override void Dispose(bool disposing)
        {
            var swap = Interlocked.Exchange(ref _memoryPointer, IntPtr.Zero);
            if(swap != IntPtr.Zero)
            {
                MUnmap(swap, _totalAllocated);
            }
        }

        private unsafe IntPtr GetMemoryPtr()
        {
            var result = MMap(IntPtr.Zero, _totalAllocated, MemoryMappedProtections.PROT_READ | MemoryMappedProtections.PROT_WRITE, MemoryMappedFlags.MAP_PRIVATE | MemoryMappedFlags.MAP_ANONYMOUS, -1, (UIntPtr)0);
            if (result.ToInt64() == -1)
            {
                var errorCode = Marshal.GetLastWin32Error();
                ExceptionHelper.UnableToAllocateMemory();
            }
            if (MLock(result, _totalAllocated) < 0)
            {
                ExceptionHelper.UnableToAllocateMemory();
            }
            return result;
        }

        private static int GetPageSize()
        {
            var pageSize = SysConf(SysConfName._SC_PAGESIZE).ToInt32();
            if (pageSize < 0)
            {
                ExceptionHelper.MemoryBadPageSize();
            }
            return pageSize;
        }
    }
}
