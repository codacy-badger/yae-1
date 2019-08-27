using System.IO.Pipelines;
using System.Runtime.InteropServices.ComTypes;

namespace yae.Framing
{
    public static class ProducerConsumerExtensions
    {
        public static IFrameConsumer<T> AsPipeFrameConsumer<T>(this PipeReader reader, IFrameDecoder<T> decoder)
            => new PipeFrameConsumer<T>(reader, decoder);

        public static IFrameProducer<T> AsPipeFrameProducer<T>(this PipeWriter writer, PipeFrameEncoder<T> encoder)
            => new PipeFrameProducer<T>(writer, encoder);

        public static (IFrameConsumer<T>, IFrameProducer<T>) AsProducerConsumer<T>(this IDuplexPipe pipe, 
            IFrameDecoder<T> decoder, PipeFrameEncoder<T> encoder)
            => (AsPipeFrameConsumer(pipe.Input, decoder), AsPipeFrameProducer(pipe.Output, encoder));
    }
}