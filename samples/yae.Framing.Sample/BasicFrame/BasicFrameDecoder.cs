using System;
using System.Buffers;
using System.IO.Pipelines;

namespace yae.Framing.Sample.BasicFrame
{
    public class BasicFrameDecoder : PipeFrameDecoder<BasicFrame>
    {
        public override bool TryParseFrame(SequenceReader<byte> reader, out BasicFrame frame, out SequencePosition consumedTo)
        {
            if (reader.TryReadLittleEndian(out int messageId) && 
                reader.TryReadLittleEndian(out int length) &&
                reader.Remaining >= length)
            {
                frame = new BasicFrame
                {
                    MessageId = messageId
                };
                var payload = reader.Sequence.Slice(reader.Position, length);
                frame.Payload = payload.Lease();
                consumedTo = payload.End;
                return true;
            }
            frame = default;
            consumedTo = default;
            return false;
        }

        public BasicFrameDecoder(PipeReader reader) : base(reader)
        {
        }
    }
}