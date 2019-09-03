using System;
using System.Buffers;
using System.Buffers.Binary;
using System.IO.Pipelines;
using System.Threading.Tasks;
using yae.Framing.Parsing;
using yae.Memory;

namespace yae.Framing.Sample.BasicFrame
{
    public class BasicFrameParser : IFrameParser<BasicFrame>
    {
        public bool TryParseFrame(ref SequenceReader<byte> reader,  out BasicFrame frame, out int length)
        {
            //todo: may use Remaining there to avoid BasicFrame alloc in the case you don't have enough data to complete the frame
            if (reader.TryReadLittleEndian(out int messageId) &&
                reader.TryReadLittleEndian(out length))
            {
                frame = new BasicFrame {MessageId = messageId};
                return true;
            }

            frame = default;
            length = default;
            return false;
        }

        public ValueTask<FlushResult> Write(PipeWriter writer, BasicFrame frame, ReadOnlyMemory<byte> payload)
        {
            var header = writer.GetSpan(8);
            BinaryPrimitives.WriteInt32LittleEndian(header, frame.MessageId);
            BinaryPrimitives.WriteInt32LittleEndian(header.Slice(4), payload.Length);
            writer.Advance(8);
            return payload.IsEmpty ? writer.FlushAsync() : writer.WriteAsync(payload);
        }
    }
}