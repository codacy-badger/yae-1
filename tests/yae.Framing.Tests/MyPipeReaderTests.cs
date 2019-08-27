using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace yae.Framing.Tests
{
    class TestBasePipeFrameConsumer : AbstractPipeConsumer
    {
        public ReadResult Result { get; set; }
        public bool ShouldThrow { get; set; } = false;

        public TestBasePipeFrameConsumer(PipeReader reader) : base(reader)
        {
        }

        protected override ValueTask<ReadResult> ReadAsync()
        {
            if(ShouldThrow) throw new Exception("Exception from tests");
            return new ValueTask<ReadResult>(Result);
        }
    }
    public class MyPipeReaderTests
    {
        [Fact]
        public async Task ReadResult_BreakLoopOnCancel()
        {
            var (pipe, _) = GetPipe();
            pipe.Result = new ReadResult(default, true, false);

            await foreach (var _ in pipe.ConsumeAsync())
            {
                Assert.True(false); //we shouldn't reach it
            }
        }

        [Fact]
        public async Task ConsumeAsync_BreakLoopOnTokenCancel()
        {
            var (pipe, _) = GetPipe();
            var token = new CancellationToken(true);
            await foreach (var _ in pipe.ConsumeAsync(token))
            {
                Assert.True(false);
            }
        }

        [Fact]
        public async Task ConsumeAsync_ShouldBreakOnComplete()
        {
            var (pipe, _) = GetPipe();
            var buffer = new ReadOnlySequence<byte>(new byte[256]);
            pipe.Result = new ReadResult(buffer, false, true);
            await foreach (var result in pipe.ConsumeAsync())
            {
                Assert.Equal(buffer, result);
            }
        }

        [Fact]
        public async Task ConsumeAsync_ShouldBreakOnEmptyBuffer()
        {
            var (pipe, _) = GetPipe();
            var buffer = new ReadOnlySequence<byte>(new byte[0]);
            pipe.Result = new ReadResult(buffer, false, true);
            await foreach (var result in pipe.ConsumeAsync())
            {
                Assert.True(false);
            }
        }

        [Fact]
        public async Task ConsumeAsync_ShouldBreakOnException()
        {
            var (pipe, _) = GetPipe();
            pipe.ShouldThrow = true;
            await foreach (var result in pipe.ConsumeAsync())
            {
                Assert.True(false);
            }
        }

        private static (TestBasePipeFrameConsumer reader, PipeWriter writer) GetPipe()
        {
            var pipe = new Pipe();
            return (new TestBasePipeFrameConsumer(pipe.Reader), pipe.Writer);
        }
    }
}
