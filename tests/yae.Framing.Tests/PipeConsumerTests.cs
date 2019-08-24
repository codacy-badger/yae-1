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

namespace yae.Framing.Tests
{
    class Decoder : IFrameDecoder<Memory<byte>>
    {
        public bool TryParseFrame(ReadOnlySequence<byte> buffer, out Memory<byte> frame, out SequencePosition consumedTo)
        {
            if(buffer.Length < 256)
            {
                frame = default;
                consumedTo = default;
                return false;
            }

            var array = new byte[256];
            buffer.Slice(0, 256).CopyTo(array);
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
        public void Dispose_ShouldDispose()
        {
            var (consumer, _) = GetConsumer();
            consumer.Close();

        }

        [Fact]
        public void Dispose_ShouldThrow()
        {
            var (consumer, _) = GetConsumer();
            consumer.Dispose();
            Assert.Throws<ObjectDisposedException>(consumer.Dispose);

        }
        [Fact]
        public async Task ShouldConsumeOneFrame()
        {
            var (consumer, writer) = GetConsumer();
            await writer.WriteAsync(new byte[256]);
            await foreach (var frame in consumer.ConsumeAsync())
            {
                Assert.Equal(256, frame.Length);
                break;
            }
        }
        [Fact]
        public async Task ShouldConsume1024Frame()
        {
            var (consumer, writer) = GetConsumer();
            var enumerator = consumer.ConsumeAsync().GetAsyncEnumerator();

            for (var i = 0; i < 1024; i++)
            {
                await writer.WriteAsync(new byte[256]); //produces it
                await enumerator.MoveNextAsync(); //consumes it
                Assert.Equal(256, enumerator.Current.Length);
            }
            
        }

        private static (PipeFrameConsumer<Memory<byte>>, PipeWriter) GetConsumer()
        {
            var pipe = new Pipe();
            return (pipe.Reader.AsFrameConsumer(new Decoder()) as PipeFrameConsumer<Memory<byte>>,
                pipe.Writer);
        }

        private static (PipeFrameConsumer<Memory<byte>>, PipeReader) GetConsumerAsReader()
        {
            var pipe = new Pipe();
            return (pipe.Reader.AsFrameConsumer(new Decoder()) as PipeFrameConsumer<Memory<byte>>, pipe.Reader);
        }
    }
}
