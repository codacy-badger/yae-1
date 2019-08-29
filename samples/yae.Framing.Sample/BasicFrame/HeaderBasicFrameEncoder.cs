using System;
using System.Buffers.Binary;
using System.IO.Pipelines;

namespace yae.Framing.Sample.BasicFrame
{
    public class HeaderBasicFrameEncoder : HeaderFrameEncoder<BasicFrame>
    {
        public override int GetHeaderLength(BasicFrame frame) => 4 + 4;
        public override void WriteHeader(Span<byte> dst, BasicFrame frame)
        {
            BinaryPrimitives.WriteInt32LittleEndian(dst, frame.MessageId);
            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(4), frame.Payload.Memory.Length);
        }

        public HeaderBasicFrameEncoder(PipeWriter writer) : base(writer)
        {
        }
    }
}