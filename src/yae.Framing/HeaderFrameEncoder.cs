using System;
using System.IO.Pipelines;
using System.Threading.Tasks;

namespace yae.Framing
{
    public abstract class HeaderFrameEncoder<TFrame> : IFrameEncoder<IFrameWrapper<TFrame>>
    {
        public  ValueTask<FlushResult> WriteAsync(PipeWriter writer, IFrameWrapper<TFrame> frameWrapper)
        {
            var headerLen = GetHeaderLength(frameWrapper.Frame);
            var headerSpan = writer.GetSpan(headerLen);
            WriteHeader(headerSpan, frameWrapper.Frame);
            writer.Advance(headerLen);

            var payload = frameWrapper.MemoryPayload;
            return payload.IsEmpty
                ? default
                : writer.WriteAsync(payload);
        }

        protected abstract int GetHeaderLength(TFrame frame);
        protected abstract void WriteHeader(Span<byte> dst, TFrame frame);
    }
}