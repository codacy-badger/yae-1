using yae.Framing.IO;

namespace yae.Framing.Sample.BasicFrame
{
    public class BasicFrameDecoder : FrameDecoder<BasicFrame>
    {
        public BasicFrameDecoder() : base(new BasicFrameParser()) { }
    }
}