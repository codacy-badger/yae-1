using System;
using System.Buffers;
using System.Collections.Generic;

namespace yae.Framing
{
    public interface IFrameDecoder<TFrame>
    {
        bool TryParseFrame(ReadOnlySequence<byte> buffer, out TFrame frame, out SequencePosition consumedTo);
    }
}
