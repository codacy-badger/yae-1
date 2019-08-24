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
    internal class HeaderFrame
    {
        public int MessageId { get; }

        public HeaderFrame(int messageId)
        {
            MessageId = messageId;
        }
    }

    class HeaderFrameDecoder : HeaderFrameDecoder<HeaderFrame>
    {
        protected override bool TryParseHeader(ref SequenceReader<byte> sequenceReader, out HeaderFrame frame, out int length)
        {
            if (sequenceReader.TryReadLittleEndian(out length) && 
                sequenceReader.TryReadLittleEndian(out int messageId))
            {
                frame = new HeaderFrame(messageId);
                return true;
            }

            frame = default;
            length = default;
            return false;
        }
    }

    class HeaderFrameEncoder : HeaderFrameEncoder<HeaderFrame>
    {
        protected override int GetHeaderLength(HeaderFrame frame) => 4 + 4;
        protected override void WriteHeader(Span<byte> dst, OutputFrame<HeaderFrame> outputFrame)
        {
            BinaryPrimitives.WriteInt32LittleEndian(dst, outputFrame.Frame.MessageId);
            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(4), outputFrame.Payload.Length);
        }
    }

    public class HeaderFrameEncoderTests
    {
        [Fact]
        public async Task WriteAsync_WithPayload()
        {
            var (encoder, reader, writer) = GetEncoder();
            var outputFrame = GetOutputFrame(4, new byte[1024]);

            await encoder.WriteAsync(writer, outputFrame);
            var result = await reader.ReadAsync();
            Assert.Equal(1024+8, result.Buffer.Length);
        }

        [Fact]
        public async Task WriteAsync_WithEmptyPayload()
        {
            var (encoder, _, writer) = GetEncoder();
            var outputFrame = GetOutputFrame(4, new byte[0]);

            await encoder.WriteAsync(writer, outputFrame); //shouldn't block
        }

        private static OutputFrame<HeaderFrame> GetOutputFrame(int id, ReadOnlyMemory<byte> payload)
        {
            var frame = new HeaderFrame(id);
            return new OutputFrame<HeaderFrame>(frame, payload);
        }

        private static (HeaderFrameEncoder encoder, PipeReader reader, PipeWriter writer) GetEncoder()
        {
            var pipe = new Pipe();
            var encoder = new HeaderFrameEncoder();
            return (encoder, pipe.Reader, pipe.Writer);
        }
    }

    public class HeaderFrameDecoderTests
    {
        [Fact]
        public void TryParseFrame_CompleteFrame()
        {
            var encoder = new HeaderFrameDecoder();
            var segment = GetFrame(1024, 4);
            encoder.TryParseFrame(segment, out var wrapper, out var consumedTo);
            Assert.Equal(1024, wrapper.Payload.Length);
            Assert.Equal(segment.GetPosition(1024+8), consumedTo);
        }

        [Fact]
        public void TryParseFrame_IncompleteHeader()
        {
            var encoder = new HeaderFrameDecoder();
            var segment = GetFrame(1024, 4).Slice(0, 7);

            encoder.TryParseFrame(segment, out var wrapper, out var consumedTo);
            Assert.Equal(default, wrapper);
            Assert.Equal(default, consumedTo);
        }

        [Fact]
        public void TryParseFrame_IncompletePayload()
        {
            var encoder = new HeaderFrameDecoder();
            var segment = GetFrame(1024, 4).Slice(0, 547);

            encoder.TryParseFrame(segment, out var wrapper, out var consumedTo);
            Assert.Equal(default, wrapper);
            Assert.Equal(default, consumedTo);
        }

        [Fact]
        public void TryParseFrame_NegativeLength()
        {
            var encoder = new HeaderFrameDecoder();
            var segment = GetFrame(-1, 4);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                encoder.TryParseFrame(segment, out var wrapper, out var consumedTo);
            });

        }


        private static ReadOnlySequence<byte> GetFrame(int length, int messageId)
        {
            var localLength = length;
            if (localLength < 0)
                localLength = 0;

            var dst = new byte[4 + 4 + localLength];
            BinaryPrimitives.WriteInt32LittleEndian(dst, length);
            BinaryPrimitives.WriteInt32LittleEndian(dst.AsSpan(4), messageId);
            return new ReadOnlySequence<byte>(dst);
        }

        private static (IFrameConsumer<InputFrame<HeaderFrame>>, PipeWriter writer) GetConsumer()
        {
            var pipe = new Pipe();
            return (pipe.Reader.AsFrameConsumer(new HeaderFrameDecoder()), pipe.Writer);
        }
    }
}
