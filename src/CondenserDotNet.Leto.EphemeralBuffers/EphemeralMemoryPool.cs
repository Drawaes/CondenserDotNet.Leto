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
    public sealed partial class EphemeralMemoryPool : MemoryPool<byte>
    {
        private readonly int _bufferSize;
        private readonly int _numberOfBuffers;
        private ConcurrentQueue<EphemeralOwnedMemory> _buffers = new ConcurrentQueue<EphemeralOwnedMemory>();
        private IntPtr _memoryPointer;
        private long _totalAllocated;
        private static readonly int _pageSize = GetPageSize();
        private bool _allowWorkingSetIncrease;

        public override int MaxBufferSize => _bufferSize;
                
        public unsafe EphemeralMemoryPool(int bufferSize, int numberOfBuffers, bool allowWorkingsetIncrease = false)
        {
            _allowWorkingSetIncrease = allowWorkingsetIncrease;
            _bufferSize = bufferSize;
            _numberOfBuffers = numberOfBuffers;

            // Calculate total to allocate, which is space needed for buffers rounded up to the nearest page
            _totalAllocated = _numberOfBuffers * bufferSize;
            _totalAllocated = _totalAllocated / _pageSize + (_totalAllocated % _pageSize == 0 ? 0 : 1);
            _totalAllocated *= _pageSize;

            _memoryPointer = GetMemoryPtr();

            var ptr = (byte*) _memoryPointer;
            for(var i = 0; i < numberOfBuffers;i++)
            {
                _buffers.Enqueue(new EphemeralOwnedMemory(this, ptr, bufferSize));
                ptr += bufferSize;
            }
        }

        public override IMemoryOwner<byte> Rent(int minBufferSize = -1)
        {
            if (minBufferSize > _bufferSize) ExceptionHelper.RequestedBufferTooLarge();
            if(!_buffers.TryDequeue(out var owner))
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
