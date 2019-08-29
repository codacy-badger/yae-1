using System;
using System.Buffers;
using System.Buffers.Binary;
using System.IO.Pipelines;
using Xunit;
using yae.Framing.Sample.BasicFrame;

namespace yae.Framing.Tests
{
    public class HeaderBasicFrameDecoderTests
    {
        /*[Fact]
        public void TryParseFrame_CompleteFrame()
        {
            var encoder = new HeaderBasicFrameDecoder();
            var reader = new SequenceReader<byte>(GetFrame(1024, 4));
            var canParse = encoder.TryParseFrame(reader, out var wrapper, out var consumedTo);
            Assert.True(canParse);
            //Assert.Equal(1024, wrapper.Payload.Length);
            Assert.Equal(1024+8, consumedTo.GetInteger());
        }

        [Fact]
        public void TryParseFrame_IncompleteHeader()
        {
            var encoder = new HeaderBasicFrameDecoder();
            var reader = new SequenceReader<byte>(GetFrame(1024, 4).Slice(0, 7));

            encoder.TryParseFrame(reader, out var wrapper, out var consumedTo);
            Assert.Equal(default, wrapper);
            Assert.Equal(default, consumedTo);
        }

        [Fact]
        public void TryParseFrame_IncompletePayload()
        {
            var encoder = new HeaderBasicFrameDecoder();
            var reader = new SequenceReader<byte>(GetFrame(1024, 4).Slice(0, 547));

            encoder.TryParseFrame(reader, out var wrapper, out var consumedTo);
            Assert.Equal(default, wrapper);
            Assert.Equal(default, consumedTo);
        }

        [Fact]
        public void TryParseFrame_NegativeLength()
        {
            var encoder = new HeaderBasicFrameDecoder();

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var reader = new SequenceReader<byte>(GetFrame(-1, 4));
                encoder.TryParseFrame(reader, out var wrapper, out var consumedTo);
            });

        }


        private static ReadOnlySequence<byte> GetFrame(int length, int messageId)
        {
            var localLength = length;
            if (localLength < 0)
                localLength = 0;

            var dst = new byte[4 + 4 + localLength];
            BinaryPrimitives.WriteInt32LittleEndian(dst, messageId);
            BinaryPrimitives.WriteInt32LittleEndian(dst.AsSpan(4), length);
            return new ReadOnlySequence<byte>(dst);
        }*/
    }
}