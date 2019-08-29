using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace yae.Framing
{
    public interface IFrameEncoder<in TFrame> : IDisposable where TFrame : IFrame
    {
        ValueTask EncodeAsync(TFrame frame);
        ValueTask EncodeEnumerableAsync(IEnumerable<TFrame> frame, CancellationToken token = default);
        ValueTask EncodeEnumerableAsync(IAsyncEnumerable<TFrame> frames, CancellationToken token = default);
        void Close(Exception ex = null);
    }
}