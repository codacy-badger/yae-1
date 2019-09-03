using System;
using System.IO.Pipelines;
using System.Threading.Tasks;
using yae.Framing.IO;
using yae.Framing.Parsing;

namespace yae.Framing.Parsing
{
    public abstract class HeaderFrameWriter<TFrame> : IFrameWriter<TFrame>
    {
        /// <summary>
        /// Gets the header length, in bytes.
        /// </summary>
        /// <param name="frame"></param>
        /// <returns></returns>
        public abstract int GetHeaderLength(TFrame frame);

        public abstract void WriteHeader(Span<byte> dst, TFrame frame, int payloadLength);

        public ValueTask<FlushResult> Write(PipeWriter writer, TFrame frame, ReadOnlyMemory<byte> payload)
        {
            var headerLen = GetHeaderLength(frame);
            if(headerLen < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(GetHeaderLength));
            }

            var headerSpan = writer.GetSpan(headerLen);
            WriteHeader(headerSpan, frame, payload.Length);
            writer.Advance(headerLen);

            return payload.IsEmpty ? writer.FlushAsync() : writer.WriteAsync(payload);
        }
    }
}