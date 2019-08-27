using System;
using System.Buffers.Binary;

namespace yae.Framing.Sample.BasicFrame
{
    public class HeaderBasicFrameEncoder : HeaderFrameEncoder<BasicFrame>
    {
        protected override int GetHeaderLength(BasicFrame frame) => 4 + 4;
        protected override void WriteHeader(Span<byte> dst, OutputFrame<BasicFrame> outputFrame)
        {
            BinaryPrimitives.WriteInt32LittleEndian(dst, outputFrame.Frame.MessageId);
            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(4), outputFrame.Payload.Length);
        }
    }
}