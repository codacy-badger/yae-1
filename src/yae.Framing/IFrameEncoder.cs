using System;

namespace yae.Framing
{
    public interface IFrameEncoder<T>
    {
        ReadOnlyMemory<byte> GetPayload(T frame);
    }

    public interface IPrefixedFrameEncoder<T> : IFrameEncoder<T>
    {
        int GetHeaderLength(T frame);
        void WriteHeader(Span<byte> span, T frame);
    }
}