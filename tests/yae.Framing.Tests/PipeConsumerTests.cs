using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using yae.Framing;

namespace yae.Framing.Tests
{
    class Decoder : IFrameDecoder<Memory<byte>>
    {
        public bool TryParseFrame(SequenceReader<byte> buffer, out Memory<byte> frame, out SequencePosition consumedTo)
        {
            if(buffer.Length < 256)
            {
                frame = default;
                consumedTo = default;
                return false;
            }
            var array = new byte[256];
            buffer.Sequence.Slice(0, 256).CopyTo(array);
            frame = array;
            consumedTo = buffer.Sequence.GetPosition(256);
            return true;
        }
    }

    public class FrameDecoderExtensionsTests
    {
        [Theory]
        [InlineData(1)]
        [InlineData(8)]
        [InlineData(32)]
        [InlineData(128)]
        [InlineData(1024)]
        public void ShouldProcessFrames(int n)
        {
            var decoder = new Decoder();
            var holder = GetBuffer(n);
            var processed = 0;
            var enumerable = decoder.AsEnumerable(holder);
            foreach (var frame in enumerable)
            {
                Assert.Equal(256, frame.Length);
                processed++;
            }
            Assert.Equal(0, holder.Buffer.Length);
            Assert.Equal(n, processed);
        }

        private static BufferHolder GetBuffer(int n = 1)
        {
            var array = new byte[n * 256];
            var holder = new BufferHolder {Buffer = new ReadOnlySequence<byte>(array)};
            return holder;

        }
    }
    public class PipeConsumerTests
    {
        private readonly ITestOutputHelper _output;

        public PipeConsumerTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task ShouldThrowOnCancel()
        {
            var (consumer, _) = GetConsumer();
            using var cts = new CancellationTokenSource();

            var enumerator = consumer.ConsumeAsync(cts.Token).GetAsyncEnumerator();
            cts.Cancel();
            var canMove = await enumerator.MoveNextAsync();
            Assert.False(canMove);
        }

        [Fact]
        public async Task ShouldComplete_OnWriterComplete()
        {
            var (consumer, writer) = GetConsumer();
            var enumerable = consumer.ConsumeAsync();
            writer.Complete();
            Assert.False(await enumerable.GetAsyncEnumerator().MoveNextAsync());
        }

        [Fact]
        public void DefaultState()
        {
            var (consumer, _) = GetConsumer();
            Assert.False(consumer.IsProgressing);
        }

        [Fact]
        public async Task ShouldComplete_OnReaderComplete()
        {
            var (consumer, _) = GetConsumer();
            var enumerable = consumer.ConsumeAsync();
            consumer.Dispose();
            Assert.False(await enumerable.GetAsyncEnumerator().MoveNextAsync());
        }

        [Fact]
        public async Task ShouldConsume()
        {
            var (consumer, writer) = GetConsumer();
            await writer.WriteAsync(new byte[256]);
            await foreach (var frame in consumer.ConsumeAsync())
            {
                Assert.Equal(256, frame.Length);
                break;
            }
        }
        private static (PipeFrameConsumer<Memory<byte>>, PipeWriter) GetConsumer()
        {
            var pipe = new Pipe();
            return (pipe.Reader.AsFrameConsumer(new Decoder()) as PipeFrameConsumer<Memory<byte>>,
                pipe.Writer);
        }
    }
}
