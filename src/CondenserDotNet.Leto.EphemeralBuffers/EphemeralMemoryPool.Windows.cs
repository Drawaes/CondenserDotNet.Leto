using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using static CondenserDotNet.Leto.EphemeralBuffers.Interop.Windows.Interop;

namespace CondenserDotNet.Leto.EphemeralBuffers
{
    public sealed partial class EphemeralMemoryPool : MemoryPool<byte>
    {
        private static readonly object _lock = new object();
        
        private unsafe IntPtr GetMemoryPtr()
        {
            var result = VirtualAlloc(IntPtr.Zero, (UIntPtr)_totalAllocated, MemOptions.MEM_COMMIT | MemOptions.MEM_RESERVE, PageOptions.PAGE_READWRITE);
            try
            {
                if (!VirtualLock(result, (UIntPtr)_totalAllocated))
                {
                    //We couldn't lock the memory
                    var error = (ExceptionHelper.WinErrors)Marshal.GetLastWin32Error();
                    if (_allowWorkingSetIncrease && error == ExceptionHelper.WinErrors.ERROR_WORKING_SET_QUOTA)
                    {
                        //We are going to try to increase the working set to allow us to lock the memory
                        lock (_lock)
                        {
                            var currentProcess = GetCurrentProcess();
                            if (!GetProcessWorkingSetSize(currentProcess, out IntPtr minimumWorkingSetSize, out IntPtr maximumWorkingSetSize))
                            {
                                error = (ExceptionHelper.WinErrors)Marshal.GetLastWin32Error();
                                ExceptionHelper.UnableToAllocateMemory(error);
                            }
                            var minSize = minimumWorkingSetSize.ToInt64() + _totalAllocated;
                            var maxSize = Math.Max(minSize, maximumWorkingSetSize.ToInt64());
                            if (!SetProcessWorkingSetSize(currentProcess, (IntPtr)minSize, (IntPtr)maxSize))
                            {
                                error = (ExceptionHelper.WinErrors)Marshal.GetLastWin32Error();
                                ExceptionHelper.UnableToAllocateMemory(error);
                            }
                            //We should have increased the working set so we can attempt to lock again
                            if (VirtualLock(result, (UIntPtr)_totalAllocated))
                            {
                                  return result;
                            }
                            error = (ExceptionHelper.WinErrors)Marshal.GetLastWin32Error();
                        }
                    }
                    ExceptionHelper.UnableToAllocateMemory(error);
                }
                return result;
            }
            catch
            {
                //Attempt to free the memory we couldn't lock
                VirtualFree(result, (UIntPtr)0, 0x8000);
                throw;
            }
        }

        protected override void Dispose(bool disposing)
        {
            var swap = Interlocked.Exchange(ref _memoryPointer, IntPtr.Zero);
            if (swap != IntPtr.Zero)
            {
                if (!VirtualFree(swap, (UIntPtr)0, 0x8000))
                {
                    var error = (ExceptionHelper.WinErrors)Marshal.GetLastWin32Error();
                    ExceptionHelper.UnableToFreeMemory(error);
                }
            }
        }

        private static int GetPageSize()
        {
            GetSystemInfo(out SYSTEM_INFO sysInfo);
            return sysInfo.dwPageSize;
        }
    }
}
