using System;
using System.Buffers;

namespace yae.Framing.Parsing
{
    public interface IFrameReader<TFrame> //where TFrame : IFrame
    {
        bool TryParseFrame(ref SequenceReader<byte> reader, out TFrame frame, out int length);
    }
}