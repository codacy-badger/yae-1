using System.Threading.Tasks;

namespace yae.Async.Tests
{
    public class MockAsyncOperation : AsyncOperation<object, int>
    {
        private readonly bool _isSync;

        public int Continuations { get; private set; }

        public MockAsyncOperation(bool isSynchronous)
        {
            _isSync = isSynchronous;
            Continuations = 0;
        }

        protected override ValueTask<int> CanExecuteSynchronous(object obj) =>
            _isSync 
                ? new ValueTask<int>(int.MinValue) 
                : new ValueTask<int>(Task.Delay(1).ContinueWith(t => int.MaxValue));

        protected override void Continuation(object obj) => Continuations++;
    }
}