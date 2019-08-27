using System.Buffers.Binary;
using System.IO.Pipelines;
using System.Threading.Tasks;

namespace yae.Framing.Sample.BasicFrame
{
    public class BasicFrameEncoder : PipeFrameEncoder<BasicFrame>
    {
        protected override ValueTask<FlushResult> Write(PipeWriter writer, BasicFrame frame)
        {
            var header = writer.GetSpan(8);
            BinaryPrimitives.WriteInt32LittleEndian(header, frame.MessageId);
            BinaryPrimitives.WriteInt32LittleEndian(header.Slice(4), frame.Data.Length);
            writer.Advance(8);
            return frame.Data.IsEmpty ? default : writer.WriteAsync(frame.Data);
        }
    }
}