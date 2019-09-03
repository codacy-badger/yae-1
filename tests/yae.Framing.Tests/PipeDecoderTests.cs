using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Extensions;
using Moq;
using Xunit;
using yae.Framing.IO;
using yae.Framing.Sample.BasicFrame;

namespace yae.Framing.Tests
{
    public class PipeDecoderTests
    {

        [Fact]
        public void Close_ShouldCompletePipe()
        {
            var (decoder, reader, _) = GetDecoder();
            decoder.Reset(reader);

            decoder.Close();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(256)]
        [InlineData(1024)]
        //[InlineData(8192)]
        public async Task DecodeAsync_ReadFrames(int n)
        {
            var (decoder, reader, writer) = GetDecoder();
            decoder.Reset(reader);
            var enumerator = decoder.DecodeAsync().GetAsyncEnumerator();

            for (var i = 0; i < n; i++)
            {
                await writer.WriteAsync(FrameProvider.GetFrameMemory(4, FrameProvider.FrameSize));
                var moveNext = await enumerator.MoveNextAsync();
                moveNext.Should().BeTrue();
                var (frame, payload) = enumerator.Current;
                frame.MessageId.Should().Be(4);
                payload.Length.Should().Be(FrameProvider.FrameSize);
            }
            decoder.FramesRead.Should().Be(n);
        }

        [Fact]
        public async Task DecodeAsync_BreakOnWriterComplete()
        {
            var (decoder, reader, writer) = GetDecoder();
            decoder.Reset(reader);
            var enumerator = decoder.DecodeAsync().GetAsyncEnumerator();
            var canMoveNext = enumerator.MoveNextAsync();
            writer.Complete();
            reader.CancelPendingRead();
            Assert.False(await canMoveNext);
        }

        [Fact]
        public async Task DecodeAsync_BreakOnReaderComplete()
        {
            var (decoder, reader, writer) = GetDecoder();
            decoder.Reset(reader);
            var enumerator = decoder.DecodeAsync().GetAsyncEnumerator();
            var canMoveNext = enumerator.MoveNextAsync();
            reader.Complete();
            reader.CancelPendingRead();
            Assert.False(await canMoveNext);
        }

        [Fact]
        public async Task DecodeAsync_ThrowOnCancellation()
        {
            var (decoder, reader, writer) = GetDecoder();
            decoder.Reset(reader);
            var token = new CancellationToken(true);
            await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                await foreach (var _ in decoder.DecodeAsync(token))
                {

                }
            });
        }


        private static (FrameDecoder<BasicFrame> decoder, PipeReader reader, PipeWriter writer) GetDecoder()
        {
            var pipe = new Pipe();
            var decoder = new BasicFrameDecoder();
            decoder.Reset(pipe.Reader);
            return (decoder, pipe.Reader, pipe.Writer);
        }
    }
}