using System;
using System.Buffers;

namespace yae.Framing
{
    public interface IFrameDecoder<TFrame>
    {
        bool TryParseFrame(SequenceReader<byte> buffer, out TFrame frame, out SequencePosition consumedTo);
    }
}
