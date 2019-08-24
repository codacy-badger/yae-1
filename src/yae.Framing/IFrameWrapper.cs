using System;
using System.Buffers;

namespace yae.Framing
{
    public interface IFrameWrapper<out T>
    {
        T Frame { get; }
        /// <summary>
        /// Use only when you receive the frame
        /// </summary>
        ReadOnlySequence<byte> SequencePayload { get; }

        /// <summary>
        /// Use only when you send the frame
        /// </summary>
        ReadOnlyMemory<byte> MemoryPayload { get; }
    }
}