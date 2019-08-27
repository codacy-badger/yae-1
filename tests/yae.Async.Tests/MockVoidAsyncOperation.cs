using System.Threading.Tasks;

namespace yae.Async.Tests
{
    public class MockVoidAsyncOperation : VoidAsyncOperation<object>
    {
        private readonly bool _isSync;

        public int Continuations { get; private set; }

        public MockVoidAsyncOperation(bool isSynchronous)
        {
            _isSync = isSynchronous;
            Continuations = 0;
        }

        protected override ValueTask CanExecuteSynchronous(object obj) =>
            _isSync ? default : new ValueTask(Task.Delay(1));

        protected override void Continuation(object obj) => Continuations++;
    }
}