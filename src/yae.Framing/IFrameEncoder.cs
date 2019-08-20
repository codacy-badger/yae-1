using System;

namespace yae.Framing
{
    public interface IFrameEncoder<T>
    {
        /*int GetHeaderLength(T frame);
        void WriteHeader(Span<byte> span, T frame);*/
        ReadOnlyMemory<byte> GetPayload(T frame); //as ReadOnlySequence? nah!
    }

    public interface IPrefixedFrameEncoder<T> : IFrameEncoder<T>
    {
        int GetHeaderLength(T frame);
        void WriteHeader(Span<byte> span, T frame);
    }
}