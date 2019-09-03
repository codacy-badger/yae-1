using System;
using System.IO.Pipelines;
using System.Threading.Tasks;

namespace yae.Framing.Parsing
{
    //todo: TrackedPipeWrite on top of PipeWriter
    public interface IFrameWriter<in TFrame> // where TFrame : IFrame
    {
        ValueTask<FlushResult> Write(PipeWriter writer, TFrame frame, ReadOnlyMemory<byte> payload);
    }
}