using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace yae.Buffers.Tests
{
    class Frame
    {
        public Memory<byte> Data { get; set; }
    }
    class Encoder : IFrameEncoder<Frame>
    {
        public int GetHeaderLength(Frame frame) => 4;

        public ReadOnlyMemory<byte> GetPayload(Frame frame)
        {
            return frame.Data;
        }

        public void WriteHeader(Span<byte> span, Frame frame)
        {
            BinaryPrimitives.WriteInt32LittleEndian(span, frame.Data.Length); //writes length
        }
    }
    public class PipeProducerFrameTests
    {
        [Fact]
        public async Task ValentinPd()
        {
            var pipe = new Pipe();
            var producer = new PipeProducerFrame<Frame>(pipe.Writer, new Encoder());
            var frm1 = new Frame();
            frm1.Data = new byte[1024];
            var written = await producer.ProduceAsync(frm1);
            Assert.Equal(1028, written);

            var frm2 = new Frame();
            frm2.Data = new byte[0];
            written = await producer.ProduceAsync(frm2);
            Assert.Equal(4, written);
        }
    }
}
