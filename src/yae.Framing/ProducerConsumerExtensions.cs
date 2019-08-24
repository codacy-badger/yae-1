using System.IO.Pipelines;
using System.Runtime.InteropServices.ComTypes;

namespace yae.Framing
{
    public static class ProducerConsumerExtensions
    {
        public static IFrameConsumer<T> AsFrameConsumer<T>(this PipeReader reader, IFrameDecoder<T> decoder)
            => new PipeFrameConsumer<T>(reader, decoder);

        public static IFrameProducer<T> AsFrameProducer<T>(this PipeWriter writer, IFrameEncoder<T> encoder)
            => new PipeFrameProducer<T>(writer, encoder);

        public static (IFrameConsumer<T>, IFrameProducer<T>) AsProducerConsumer<T>(this IDuplexPipe pipe, 
            IFrameDecoder<T> decoder, IFrameEncoder<T> encoder)
        {
            return (AsFrameConsumer(pipe.Input, decoder), AsFrameProducer(pipe.Output, encoder));
        }
    }
}