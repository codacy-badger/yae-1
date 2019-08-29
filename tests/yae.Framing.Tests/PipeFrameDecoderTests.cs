using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using yae.Framing;
using yae.Framing.Sample.BasicFrame;

namespace yae.Framing.Tests
{
    public class PipeDecoderTests
    {
        [Fact]
        public void Dispose_ShouldDispose()
        {
            var (decoder, _) = GetDecoder();
            decoder.Dispose();
        }

        [Fact]
        public async Task ShouldDecodeOneFrame()
        {
            var (decoder, writer) = GetDecoder();
            await FrameProvider.WriteFrame(writer, FrameProvider.GetFrame(4));
            await foreach (var frame in decoder.DecodeAsync())
            {
                Assert.Equal(4, frame.MessageId);
                Assert.Equal(FrameProvider.FrameSize, frame.Payload.Memory.Length);
                break;
            }
        }
        [Fact]
        public async Task ShouldDecode1024Frame()
        {
            var (decoder, writer) = GetDecoder();
            var enumerator = decoder.DecodeAsync().GetAsyncEnumerator();

            for (var i = 0; i < 1024; i++)
            {
                await FrameProvider.WriteFrame(writer, FrameProvider.GetFrame(i));
                await enumerator.MoveNextAsync();
                var frame = enumerator.Current;
                Assert.Equal(i, frame.MessageId);
                Assert.Equal(FrameProvider.FrameSize, frame.Payload.Memory.Length);
            }
        }

        private static (PipeFrameDecoder<BasicFrame> decoder, PipeWriter writer) GetDecoder()
        {
            var pipe = new Pipe();
            return (new BasicFrameDecoder(pipe.Reader), pipe.Writer);
        }
    }
}
