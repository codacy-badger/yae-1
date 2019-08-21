using System.IO.Pipelines;

namespace yae.Framing
{
    public static class ProducerConsumerExtensions
    {
        public static IFrameConsumer<T> AsFrameConsumer<T>(this PipeReader reader, IFrameDecoder<T> decoder)
            => new PipeFrameConsumer<T>(reader, decoder);

        public static IFrameProducer<T> AsPrefixedFrameProducer<T>(this PipeWriter writer, IPrefixedFrameEncoder<T> encoder)
            => new PrefixedFrameProducer<T>(writer, encoder);
    }
}