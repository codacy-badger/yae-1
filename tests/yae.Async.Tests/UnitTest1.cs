using System;
using System.Threading.Tasks;
using Xunit;

namespace yae.Async.Tests
{
    public class MockAsyncOperation : AsyncOperation
    {
        private readonly bool _isSync;

        public int FinallyTimes { get; private set; }

        public MockAsyncOperation(bool isSynchronous)
        {
            _isSync = isSynchronous;
            FinallyTimes = 0;
        }

        protected override ValueTask CanCompleteSync() => _isSync ? default : new ValueTask(Task.Delay(10));

        public override void OnFinally() => FinallyTimes++;
    }

    public class MockAsyncOperationInt : AsyncOperation<int>
    {
        private readonly bool _isSync;

        public int FinallyTimes { get; private set; }

        public MockAsyncOperationInt(bool isSynchronous)
        {
            _isSync = isSynchronous;
            FinallyTimes = 0;
        }
        
        protected override ValueTask<int> CanCompleteSync()
        {
            return _isSync 
                ? new ValueTask<int>(147) 
                : new ValueTask<int>(Task.Delay(10).ContinueWith(t => 148));
        }

        public override void OnFinally() => FinallyTimes++;
    }

    public class UnitTest1
    {
        [Fact]
        public async Task ExecuteAsync_ShouldBeSynchronous()
        {
            var operation = new MockAsyncOperation(true);
            var task = operation.ExecuteAsync();
            Assert.True(task.IsCompletedSuccessfully);
            Assert.Equal(1, operation.FinallyTimes);
            await task;
        }
        [Fact]
        public async Task ExecuteAsync_ShouldBeAsynchronous()
        {
            var operation = new MockAsyncOperation(false);
            var task = operation.ExecuteAsync();
            Assert.False(task.IsCompletedSuccessfully);
            Assert.Equal(0, operation.FinallyTimes);
            await task;
            Assert.True(task.IsCompletedSuccessfully);
            Assert.Equal(1, operation.FinallyTimes);
        }

        [Fact]
        public async Task MergeWith_BothSynchronous()
        {
            var operation1 = new MockAsyncOperation(true);
            var operation2 = new MockAsyncOperationInt(true);
            var task = operation1.MergeWith(operation2);
            Assert.Equal(147, task.Result);
            Assert.True(task.IsCompletedSuccessfully);
            Assert.Equal(1, operation1.FinallyTimes);
            Assert.Equal(1, operation2.FinallyTimes);
            var result = await task;
            
            //Assert.Equal(147, result);
        }

        [Fact]
        public async Task MergeWith_FirstSyncSecondAsync()
        {
            var operation1 = new MockAsyncOperation(true);
            var operation2 = new MockAsyncOperationInt(false);
            var task = operation1.MergeWith(operation2);
            Assert.False(task.IsCompletedSuccessfully);
            Assert.Equal(0, operation1.FinallyTimes);
            var result = await task;
            Assert.True(task.IsCompletedSuccessfully);
            Assert.Equal(1, operation1.FinallyTimes);
            Assert.Equal(1, operation2.FinallyTimes);
            Assert.Equal(148, result);
        }

        [Fact]
        public async Task MergeWith_BothAsync()
        {
            var operation1 = new MockAsyncOperation(false);
            var operation2 = new MockAsyncOperationInt(false);
            var task = operation1.MergeWith(operation2);
            Assert.False(task.IsCompletedSuccessfully);
            await task;
            Assert.True(task.IsCompletedSuccessfully);
            Assert.Equal(1, operation1.FinallyTimes);
            Assert.Equal(1, operation2.FinallyTimes);
            Assert.Equal(148, await task);
        }
    }
}
