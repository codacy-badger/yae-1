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
            var frame = new BasicFrame {MessageId = 4, Data = new byte[256]};

            await new BasicFrameEncoder().WriteAsync(writer, frame);

            await foreach (var frm in consumer.ConsumeAsync())
            {
                Assert.Equal(4, frm.MessageId);
                Assert.Equal(256, frm.Data.Length);
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
                var frame = new BasicFrame { MessageId = 4, Data = new byte[256] };

                await new BasicFrameEncoder().WriteAsync(writer, frame);
                await enumerator.MoveNextAsync(); //consumes it
                var frm = enumerator.Current;
                Assert.Equal(4, frm.MessageId);
                Assert.Equal(256, frm.Data.Length);
            }
            
        }
        private static (PipeFrameConsumer<BasicFrame>, PipeWriter) GetConsumer()
        {
            var pipe = new Pipe();
            return (pipe.Reader.AsPipeFrameConsumer(new BasicFrameDecoder()) as PipeFrameConsumer<BasicFrame>,
                pipe.Writer);
        }

        private static (PipeFrameConsumer<BasicFrame>, PipeReader) GetConsumerAsReader()
        {
            var pipe = new Pipe();
            return (pipe.Reader.AsPipeFrameConsumer(new BasicFrameDecoder()) as PipeFrameConsumer<BasicFrame>, pipe.Reader);
        }
    }
}
