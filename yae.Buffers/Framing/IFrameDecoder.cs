using System;
using System.Buffers;

namespace yae.Buffers.Framing
{
    public interface IFrameDecoder<TFrame>
    {
        //passes the "buffer" as a ref struct to avoid copies!
        bool TryParseFrame(in ReadOnlySequence<byte> buffer, out TFrame frame, out SequencePosition consumedTo);
    }
}
