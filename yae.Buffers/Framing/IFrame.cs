using System.Buffers;

namespace yae.Buffers.Framing
{
    public interface IFrame
    {
        ReadOnlySequence<byte> Payload { get; }
    }
}
