using System;
using System.IO.Pipelines;
using System.Threading.Tasks;

namespace yae.Framing
{
    public abstract class HeaderFrameEncoder<TFrame> : PipeFrameEncoder<OutputFrame<TFrame>>
    {
        /// <summary>
        /// Write to the PipeWrite.
        /// You shouldn't await in this method
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="frame"></param>
        /// <returns></returns>
        protected override ValueTask<FlushResult> Write(PipeWriter writer, OutputFrame<TFrame> frame)
        {
            var headerLen = GetHeaderLength(frame.Frame);
            var headerSpan = writer.GetSpan(headerLen);
            WriteHeader(headerSpan, frame);
            writer.Advance(headerLen);

            var payload = frame.Payload;
            return payload.IsEmpty ? default : writer.WriteAsync(payload);
        }

        /// <summary>
        /// Gets the header length, in bytes.
        /// </summary>
        /// <param name="frame"></param>
        /// <returns></returns>
        /// todo: check negative length!
        protected abstract int GetHeaderLength(TFrame frame);

        protected abstract void WriteHeader(Span<byte> dst, OutputFrame<TFrame> frame);
    }
}