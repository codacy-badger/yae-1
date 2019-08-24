using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace yae.Framing.Tests
{
    //todo: use headerFrame everywhere!
    class Frame
    {
        public Memory<byte> Data { get; set; }
    }

    class FrameEncoder : IFrameEncoder<Frame>
    {
        public ValueTask<FlushResult> WriteAsync(PipeWriter writer, Frame outputFrame)
        {
            return writer.WriteAsync(outputFrame.Data);
        }
    }

    public class PipeProducerFrameTests
    {
        private readonly ITestOutputHelper _output;

        public PipeProducerFrameTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void ValueTaskDefault()
        {
            ValueTask<FlushResult> result = default;
            Assert.True(result.IsCompletedSuccessfully);
        }

        [Fact]
        public async Task ProduceAsync_Frame()
        {
            var (producer, reader) = GetProducer();
            var frame = GetFrame();
            await producer.ProduceAsync(frame);

            var result = await reader.ReadAsync(); //can get it in a read since its smaller than 32kb
            Assert.Equal(256, result.Buffer.Length);
        }

        [Fact]
        public async Task ProduceAsync_Enumerable()
        {
            await ProduceAsync_EnumerableBase(producer => producer.ProduceAsync(GetFrames(N)));
        }

        [Fact]
        public async Task ProduceAsync_AsyncEnumerable()
        {
            await ProduceAsync_EnumerableBase(producer => producer.ProduceAsync(GetFramesAsync(N)));
        }

        [Fact]
        public void Dispose()
        {
            var (producer, _) = GetProducer();
            producer.Dispose();
            Assert.Throws<ObjectDisposedException>(producer.Dispose);
        }

        private async Task ProduceAsync_EnumerableBase(Func<PipeFrameProducer<Frame>, ValueTask> method)
        {
            var (producer, reader) = GetProducer();
            const int totalSize = N * FrameSize;

            var produceAsyncTask = method(producer);
            var sum = 0;
            while (true)
            {
                try
                {
                    var result = await reader.ReadAsync();
                    var buffer = result.Buffer;
                    sum += (int)buffer.Length;

                    reader.AdvanceTo(buffer.End);
                    if (sum >= totalSize)
                        break;
                }
                catch
                {
                    break;
                }
            }

            await produceAsyncTask;
            Assert.Equal(totalSize, sum);
        }

        private static IEnumerable<Frame> GetFrames(int n)
        {
            for (var i = 0; i < n; i++)
                yield return GetFrame();
        }

        private static async IAsyncEnumerable<Frame> GetFramesAsync(int n)
        {
            foreach (var frame in GetFrames(n))
            {
                yield return frame;
                await Task.Delay(0);
            }
        }

        public const int FrameSize = 256;
        public const int N = 1024;

        private static Frame GetFrame()
        {
            return new Frame {Data = new byte[FrameSize]};
        }

        private static (PipeFrameProducer<Frame> producer, PipeReader reader) GetProducer()
        {
            var pipe = new Pipe();
            return (new PipeFrameProducer<Frame>(pipe.Writer, new FrameEncoder()), pipe.Reader);

        }
    }
}
