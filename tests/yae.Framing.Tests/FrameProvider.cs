using System;
using System.Buffers.Binary;
using System.IO.Pipelines;
using System.Threading.Tasks;
using yae.Framing.Sample.BasicFrame;

namespace yae.Framing.Tests
{
    internal static class FrameProvider
    {
        public const int FrameSize = 1024;
        public const int HeaderSize = 8;
        public static BasicFrame GetFrame(int id)
        {
            Memory<byte> payload = new byte[1024];
            return new BasicFrame { MessageId = id, Payload = payload.Owned()};
        }

        public static ValueTask<FlushResult> WriteFrame(PipeWriter writer, BasicFrame frame)
        {
            var memory = writer.GetMemory(8);
            BinaryPrimitives.WriteInt32LittleEndian(memory.Span, frame.MessageId);
            BinaryPrimitives.WriteInt32LittleEndian(memory.Span.Slice(4), frame.Payload.Memory.Length);
            writer.Advance(8);
            return writer.WriteAsync(frame.Payload.Memory);
        }

        /*private static ReadOnlySequence<byte> GetFrame(int length, int messageId)
        {
            var localLength = length;
            if (localLength < 0)
                localLength = 0;

            var dst = new byte[4 + 4 + localLength];
            BinaryPrimitives.WriteInt32LittleEndian(dst, messageId);
            BinaryPrimitives.WriteInt32LittleEndian(dst.AsSpan(4), length);
            return new ReadOnlySequence<byte>(dst);
        }*/
    }
}