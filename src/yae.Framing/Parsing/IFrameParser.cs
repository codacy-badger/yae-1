using yae.Framing.Parsing;

namespace yae.Framing.Parsing
{
    public interface IFrameParser<TFrame> : IFrameReader<TFrame>, IFrameWriter<TFrame>
    {

    }
}