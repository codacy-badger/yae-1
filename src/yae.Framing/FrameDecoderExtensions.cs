using System.Collections.Generic;

namespace yae.Framing
{
    internal static class FrameDecoderExtensions
    {
        public static IEnumerable<T> AsEnumerable<T>(this IFrameDecoder<T> frameDecoder, BufferHolder holder)
        {
            while (frameDecoder.TryParseFrame(new System.Buffers.SequenceReader<byte>(holder.Buffer), out var frame, out var consumedTo))
            {
                yield return frame;
                holder.Buffer = holder.Buffer.Slice(consumedTo);
            }
        }


    }
}
