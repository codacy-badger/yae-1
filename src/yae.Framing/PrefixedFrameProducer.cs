using System;
using System.IO.Pipelines;
using System.Threading.Tasks;

namespace yae.Framing
{
    public class PrefixedFrameProducer<T> : PipeFrameProducer<T>
    {
        private readonly IPrefixedFrameEncoder<T> _encoder;

        public PrefixedFrameProducer(PipeWriter writer, IPrefixedFrameEncoder<T> encoder) : base(writer) 
            => _encoder = encoder;

        protected override ValueTask<FlushResult> WriteAsync(PipeWriter writer, T frame)
        {
            var headerLen = _encoder.GetHeaderLength(frame);
            var headerSpan = writer.GetSpan(headerLen);
            _encoder.WriteHeader(headerSpan, frame);
            writer.Advance(headerLen);

            var payload = _encoder.GetPayload(frame);
            return payload.IsEmpty 
                ? default //returns IsCompletedSuccessfully!
                : writer.WriteAsync(payload) /*AwaitAndWrite(writer, headerLen, payload)*/;
        }

        private static async ValueTask<int> AwaitAndWrite(PipeWriter writer, int headerLen, ReadOnlyMemory<byte> payload)
        {
            await writer.WriteAsync(payload);
            return headerLen + payload.Length;
        }
    }
}