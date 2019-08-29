using System.Buffers;
using System.IO.Pipelines;

namespace yae.Framing.Sample.BasicFrame
{
    public class HeaderBasicFrameDecoder : HeaderFrameDecoder<BasicFrame>
    {
        public override bool TryParseHeader(ref SequenceReader<byte> sequenceReader, out BasicFrame frame, out int length)
        {
            if (sequenceReader.TryReadLittleEndian(out int messageId) &&
                sequenceReader.TryReadLittleEndian(out length))
            {
                frame = new BasicFrame {MessageId = messageId};
                return true;
            }

            frame = default;
            length = default;
            return false;
        }

        public HeaderBasicFrameDecoder(PipeReader reader) : base(reader)
        {
        }
    }
}