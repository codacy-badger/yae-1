using System.Buffers;

namespace yae.Framing
{
    public interface IFrame
    {
        IMemoryOwner<byte> Payload { get; set; }
    }
}