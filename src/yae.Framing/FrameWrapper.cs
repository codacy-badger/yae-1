using System;
using System.Buffers;

namespace yae.Framing
{
    public class FrameWrapper<T> : IFrameWrapper<T>
    {
        public T Frame { get; }
        public ReadOnlySequence<byte> SequencePayload { get; }
        public ReadOnlyMemory<byte> MemoryPayload { get; }
        public FrameWrapper(T frame, ReadOnlySequence<byte> payload)
        {
            Frame = frame;
            SequencePayload = payload;
        }
        public FrameWrapper(T frame, ReadOnlyMemory<byte> payload)
        {
            Frame = frame;
            MemoryPayload = payload;
        }
    }
}