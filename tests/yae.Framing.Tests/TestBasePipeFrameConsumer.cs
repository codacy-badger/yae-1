using System;
using System.IO.Pipelines;
using System.Threading.Tasks;

namespace yae.Framing.Tests
{
    class TestBasePipeFrameConsumer : AbstractPipeConsumer
    {
        public ReadResult Result { get; set; }
        public bool ShouldThrow { get; set; } = false;

        public TestBasePipeFrameConsumer(PipeReader reader) : base(reader)
        {
        }

        protected override ValueTask<ReadResult> ReadAsync()
        {
            if(ShouldThrow) throw new Exception("Exception from tests");
            return new ValueTask<ReadResult>(Result);
        }
    }
}