using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace yae.Framing
{
    public interface IFrameDecoder<TFrame>
    {
        bool TryParseFrame(ReadOnlySequence<byte> buffer, out TFrame frame, out SequencePosition consumedTo);
    }
}
