using System;
using System.Buffers.Binary;
using System.IO.Pipelines;
using yae.Framing.Parsing;

namespace yae.Framing.Sample.BasicFrame
{
    public class HeaderBasicFrameWriter : HeaderFrameWriter<BasicFrame>
    {
        public override int GetHeaderLength(BasicFrame frame) => 4 + 4;
        public override void WriteHeader(Span<byte> dst, BasicFrame frame, int payloadLength)
        {
            BinaryPrimitives.WriteInt32LittleEndian(dst, frame.MessageId);
            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(4), payloadLength);
        }
    }
}