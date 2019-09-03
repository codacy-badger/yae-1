using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Extensions;
using Moq;
using Xunit;
using yae.Framing.Sample.BasicFrame;

namespace yae.Framing.Tests
{
    public class PipeDecoderTests
    {
        [Fact]
        public void ResetPipe_ShouldSetReader_OnUninitialized()
        {
            var (decoder, reader, _) = GetDecoder();
            Assert.Null(decoder.Reader);
            decoder.Reset(reader);
            Assert.NotNull(decoder.Reader);
        }

        [Fact]
        public void ResetPipe_ShouldNotSetReader_OnInitialized()
        {
            var (decoder, reader, _) = GetDecoder();

            decoder.Reader.Should().BeNull();
            decoder.Reset(reader);
            decoder.Reader.Should().Be(reader);

            var newReader = new Pipe().Reader;

            decoder.Reset(newReader);
            decoder.Reader.Should().NotBe(newReader);
        }

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
                var current = enumerator.Current;
                current.MessageId.Should().Be(4);
                current.Payload.Memory.Length.Should().Be(FrameProvider.FrameSize);
            }

            var totalSize = n * (FrameProvider.FrameSize + FrameProvider.HeaderSize);
            decoder.BytesRead.Should().Be(totalSize);
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

            return (new BasicFrameDecoder(pipe.Reader), pipe.Reader, pipe.Writer);
        }
    }
}