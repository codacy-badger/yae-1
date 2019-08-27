using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;

namespace yae.Framing
{
    public class PipeConsumer : AbstractPipeConsumer
    {
        public PipeConsumer(PipeReader reader) : base(reader) { }
        protected override ValueTask<ReadResult> ReadAsync() => Reader.ReadAsync();
    }
}
