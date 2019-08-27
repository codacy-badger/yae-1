using System;
using System.Buffers;

namespace yae.Framing.Sample.BasicFrame
{
    public class BasicFrameDecoder : IFrameDecoder<BasicFrame>
    {
        public bool TryParseFrame(SequenceReader<byte> reader, out BasicFrame frame, out SequencePosition consumedTo)
        {
            if (reader.TryReadLittleEndian(out int messageId) && 
                reader.TryReadLittleEndian(out int length) &&
                reader.Remaining >= length)
            {
                frame = new BasicFrame
                {
                    MessageId = messageId,
                    Data = new byte[length]
                };
                var payload = reader.Sequence.Slice(reader.Position, length);
                payload.CopyTo(frame.Data.Span);
                consumedTo = payload.End;
                return true;
            }
            frame = default;
            consumedTo = default;
            return false;
        }
    }
}