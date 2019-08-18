using System.Threading;

namespace yae.Threading
{
    //todo: check lockTaken when Entering monitors!
    public class Mutex<T>
    {
        private readonly T _object;
        private readonly object _lock;

        public Mutex(T value)
        {
            _object = value;
            _lock = new object();
        }

        public Locker<T> Lock(out T value)
        {
            Monitor.Enter(_lock);
            value = _object;
            return new Locker<T>(_lock);
        }
    }
}
