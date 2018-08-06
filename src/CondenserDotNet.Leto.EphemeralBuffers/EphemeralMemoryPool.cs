using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace CondenserDotNet.Leto.EphemeralBuffers
{
    /// <summary>
    /// Memory pool that uses "locked" memory that tells the OS not to page the memory to disk
    /// This could still be saved to disk say if for instance you are in a VM and the state is saved
    /// </summary>
    public sealed partial class EphemeralMemoryPool : MemoryPool<byte>
    {
        private readonly int _bufferSize;
        private readonly int _numberOfBuffers;
        private ConcurrentQueue<EphemeralOwnedMemory> _buffers = new ConcurrentQueue<EphemeralOwnedMemory>();
        private IntPtr _memoryPointer;
        private long _totalAllocated;
        private static readonly int _pageSize = GetPageSize();
        private bool _allowWorkingSetIncrease;

        /// <summary>
        /// This max size is actually == the only size for buffers as they are all equally sized
        /// </summary>
        public override int MaxBufferSize => _bufferSize;

        /// <summary>
        /// Constructs a new ephemeral memory pool, will throw if the OS does not allow the requested size to be allocated
        /// </summary>
        /// <param name="bufferSize"></param>
        /// <param name="numberOfBuffers"></param>
        /// <param name="allowWorkingsetIncrease"></param>
        public unsafe EphemeralMemoryPool(int bufferSize, int numberOfBuffers, bool allowWorkingsetIncrease = false)
        {
            _allowWorkingSetIncrease = allowWorkingsetIncrease;
            _bufferSize = bufferSize;
            _numberOfBuffers = numberOfBuffers;

            // Calculate total to allocate, which is space needed for buffers rounded up to the nearest page
            _totalAllocated = _numberOfBuffers * bufferSize;
            _totalAllocated = (_totalAllocated + _pageSize) & (~_pageSize);

            _memoryPointer = GetMemoryPtr();

            var ptr = (byte*)_memoryPointer;
            for (var i = 0; i < numberOfBuffers; i++)
            {
                _buffers.Enqueue(new EphemeralOwnedMemory(this, ptr, bufferSize));
                ptr += bufferSize;
            }
        }

        /// <summary>
        /// Rents a memory buffer will throw OOM exception if there are no available buffers in the pool
        /// </summary>
        /// <param name="minBufferSize"></param>
        /// <returns></returns>
        public override IMemoryOwner<byte> Rent(int minBufferSize = -1)
        {
            if (minBufferSize > _bufferSize) ExceptionHelper.RequestedBufferTooLarge();
            if (!_buffers.TryDequeue(out var owner))
            {
                ExceptionHelper.OutOfAvailableBuffers();
                return owner;
            }
            return owner;
        }

        private void Return(EphemeralOwnedMemory ownedMemory) => _buffers.Enqueue(ownedMemory);

        private unsafe class EphemeralOwnedMemory : MemoryManager<byte>
        {
            private EphemeralMemoryPool _memoryPool;
            private readonly byte* _ptr;
            private readonly int _length;

            public EphemeralOwnedMemory(EphemeralMemoryPool memoryPool, byte* ptr, int length)
            {
                _ptr = ptr;
                _length = length;
                _memoryPool = memoryPool;
            }

            public override Span<byte> GetSpan() => new Span<byte>(_ptr, _length);
            public override void Unpin() { }
            public override MemoryHandle Pin(int elementIndex = 0) => new MemoryHandle(_ptr + _length);
            public override Memory<byte> Memory => CreateMemory(0, _length);

            protected override void Dispose(bool disposing)
            {
                Debug.Assert(disposing, "Memory was finalised potential leak!");
                _memoryPool.Return(this);
            }
        }
    }
}
