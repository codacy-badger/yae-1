using System.Buffers.Binary;
using System.IO.Pipelines;
using System.Threading.Tasks;
using yae.Framing.IO;
//
namespace yae.Framing.Sample.BasicFrame
{
    public class BasicFrameEncoder : FrameEncoder<BasicFrame>
    {
        public BasicFrameEncoder() : base(new BasicFrameParser())
        {
        }
    }
}