using System.IO.Pipelines;

namespace yae.Framing
{
    public static class ProducerConsumerExtensions
    {
        public static IFrameConsumer<T> AsFrameConsumer<T>(this PipeReader reader, IFrameDecoder<T> decoder)
            => new PipeFrameConsumer<T>(reader, decoder);

        public static IFrameProducer<T> AsFrameProducer<T>(this PipeWriter writer, IFrameEncoder<T> encoder)
            => default;
    }
}