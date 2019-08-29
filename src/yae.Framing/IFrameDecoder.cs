using System;
using System.Collections.Generic;
using System.Threading;

namespace yae.Framing
{
    public interface IFrameDecoder<out TFrame> : IDisposable where TFrame : IFrame
    {
        IAsyncEnumerable<TFrame> DecodeAsync(CancellationToken token = default);
        void Close(Exception ex = null);
    }
}