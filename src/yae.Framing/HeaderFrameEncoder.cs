using System;
using System.IO.Pipelines;
using System.Threading.Tasks;

namespace yae.Framing
{
    public abstract class HeaderFrameEncoder<TFrame> : PipeFrameEncoder<TFrame> where TFrame : IFrame
    {
        protected HeaderFrameEncoder(PipeWriter writer) : base(writer)
        {
        }
        /// <summary>
        /// Write to the PipeWriter
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="frame"></param>
        /// <returns></returns>
        protected override ValueTask<FlushResult> Write(PipeWriter writer, TFrame frame)
        {
            var headerLen = GetHeaderLength(frame);
            var headerSpan = writer.GetSpan(headerLen);
            WriteHeader(headerSpan, frame);
            writer.Advance(headerLen);

            var payload = frame.Payload;
            return payload.Memory.IsEmpty ? writer.FlushAsync() : writer.WriteAsync(payload.Memory);
        }

        /// <summary>
        /// Gets the header length, in bytes.
        /// </summary>
        /// <param name="frame"></param>
        /// <returns></returns>
        /// todo: check negative length!
        public abstract int GetHeaderLength(TFrame frame);

        public abstract void WriteHeader(Span<byte> dst, TFrame frame);
    }
}