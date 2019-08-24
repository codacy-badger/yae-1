using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace yae.Framing.Tests
{
    /// <summary>
    /// [4: Length, 4: MessageId, n: Payload]
    /// </summary>
    class HeaderFrame
    {
        public int MessageId { get; set; }
    }

    class HeaderFrameDecoder : HeaderFrameDecoder<HeaderFrame>
    {
        protected override bool TryParseHeader(ref SequenceReader<byte> sequenceReader, out HeaderFrame frame, out int length)
        {
            if (sequenceReader.TryReadLittleEndian(out length) && 
                sequenceReader.TryReadLittleEndian(out int messageId))
            {
                frame = new HeaderFrame {MessageId = messageId};
                return true;
            }

            frame = default;
            length = default;
            return false;
        }
    }

    //test decoder, not consumer...XD
    public class HeaderFrameEncoderTests
    {
        [Fact]
        public async Task Test()
        {
            var (consumer, writer) = GetConsumer();
            await WriteFrame(writer, 32, 4);
            await foreach (var frame in consumer.ConsumeAsync())
            {
                Assert.Equal(32, frame.SequencePayload.Length);
                Assert.Equal(4, frame.Frame.MessageId);
                break;
            }
        }

        //todo: rework ? xD
        private static ValueTask WriteFrame(PipeWriter writer, int length, int messageId)
        {
            async ValueTask AwaitWrite(ReadOnlyMemory<byte> data)
            {
                await writer.WriteAsync(data);
            }

            Memory<byte> dst = new byte[4 + 4 + length];
            BinaryPrimitives.WriteInt32LittleEndian(dst.Span, length);
            BinaryPrimitives.WriteInt32LittleEndian(dst.Span.Slice(4), messageId);
            return AwaitWrite(dst);
        }

        //we should remove this wrapper lmao
        private static (IFrameConsumer<IFrameWrapper<HeaderFrame>>, PipeWriter writer) GetConsumer()
        {
            var pipe = new Pipe();
            return (pipe.Reader.AsFrameConsumer(new HeaderFrameDecoder()), pipe.Writer);
        }
    }
}
