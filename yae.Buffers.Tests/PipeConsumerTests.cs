using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using yae.Buffers.Framing;

namespace yae.Buffers.Tests
{
    class Decoder : IFrameDecoder<Memory<byte>>
    {
        public bool TryParseFrame(in ReadOnlySequence<byte> buffer, out Memory<byte> frame, out SequencePosition consumedTo)
        {
            if(buffer.Length < 256)
            {
                frame = default;
                consumedTo = default;
                return false;
            }
            var array = new byte[256];
            buffer.CopyTo(array);
            frame = array;
            consumedTo = buffer.GetPosition(256);
            return true;
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
            
            var exception = await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            {
                var enumerable = consumer.ConsumeAsync(cts.Token);
                cts.Cancel();
                await foreach(var frame in enumerable)
                {

                }
            });
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
        public async Task ShouldComplete_OnReaderComplete()
        {
            var (consumer, _) = GetConsumer();
            var enumerable = consumer.ConsumeAsync();
            consumer.Dispose();
            Assert.False(await enumerable.GetAsyncEnumerator().MoveNextAsync());
        }

        private static (IPipeConsumer<Memory<byte>>, PipeWriter) GetConsumer()
        {
            var pipe = new Pipe();
            return (PipeConsumerFactory.CreatePipeConsumerFrame(pipe.Reader, new Decoder()), pipe.Writer);
        }
    }
}
