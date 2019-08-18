using System.Collections.Generic;

namespace yae.Buffers.Framing
{
    public static class FrameDecoderExtensions
    {
        public static IEnumerable<T> AsEnumerable<T>(this IFrameDecoder<T> frameDecoder, BufferHolder holder)
        {
            while (frameDecoder.TryParseFrame(holder.Buffer, out var frame, out var consumedTo))
            {
                yield return frame;
                holder.Buffer = holder.Buffer.Slice(consumedTo);

            }
        }


    }
}
