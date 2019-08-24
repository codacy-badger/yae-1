using System;
using System.IO.Pipelines;
using System.Threading.Tasks;

namespace yae.Framing
{
    public abstract class HeaderFrameEncoder<TFrame> : IFrameEncoder<OutputFrame<TFrame>>
    {
        public  ValueTask<FlushResult> WriteAsync(PipeWriter writer, OutputFrame<TFrame> outputFrame)
        {
            var headerLen = GetHeaderLength(outputFrame.Frame);
            var headerSpan = writer.GetSpan(headerLen);
            WriteHeader(headerSpan, outputFrame);
            writer.Advance(headerLen);

            var payload = outputFrame.Payload;
            return payload.IsEmpty ? default : writer.WriteAsync(payload);
        }

        protected abstract int GetHeaderLength(TFrame frame);
        protected abstract void WriteHeader(Span<byte> dst, OutputFrame<TFrame> frame);

        //public OutputFrame<TFrame> GetOutputFrame(TFrame frame, ReadOnlyMemory<>) //todo: may we can add a "pooling fashion"
    }
}