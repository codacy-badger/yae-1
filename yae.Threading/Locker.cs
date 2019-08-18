using System;
using System.Threading;

namespace yae.Threading
{

    public readonly ref struct Locker<T>
    {
        private readonly object _locker;

        public Locker(object locker)
        {
            _locker = locker;
        }

        public void Dispose()
        {
            Monitor.Exit(_locker);
        }
    }
}
