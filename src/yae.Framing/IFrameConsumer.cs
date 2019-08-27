using System;
using System.Collections.Generic;
using System.Threading;

namespace yae.Framing
{
    public interface IFrameConsumer<out T> : IDisposable
    {
        IAsyncEnumerable<T> ConsumeAsync(CancellationToken token = default);
        void Close(Exception ex = null);
    }
}
