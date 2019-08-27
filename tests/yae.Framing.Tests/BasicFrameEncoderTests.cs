using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using yae.Framing.Sample.BasicFrame;

namespace yae.Framing.Tests
{

    public class BasicFrameEncoderTests
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

        private static OutputFrame<BasicFrame> GetOutputFrame(int id, ReadOnlyMemory<byte> payload)
        {
            var frame = new BasicFrame {MessageId = id};
            return new OutputFrame<BasicFrame>(frame, payload);
        }

        private static (HeaderBasicFrameEncoder encoder, PipeReader reader, PipeWriter writer) GetEncoder()
        {
            var pipe = new Pipe();
            var encoder = new HeaderBasicFrameEncoder();
            return (encoder, pipe.Reader, pipe.Writer);
        }
    }
}
