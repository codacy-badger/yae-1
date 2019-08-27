using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Xunit;

namespace yae.Async.Tests
{
    public class AsyncOperationTests
    {

        [Fact]
        public async Task ExecuteAsync_ShouldBeSynchronous()
        {
            var operation = new MockAsyncOperation(true);
            var task = operation.ExecuteAsync(null);
            Assert.True(task.IsCompletedSuccessfully);
            Assert.Equal(1, operation.Continuations);
            await task;
        }

        [Fact]
        public async Task ExecuteAsync_ShouldBeAsynchronous()
        {
            var operation = new MockAsyncOperation(false);
            var task = operation.ExecuteAsync(null);
            Assert.False(task.IsCompletedSuccessfully);
            Assert.Equal(0, operation.Continuations);
            await task;
            Assert.Equal(1, operation.Continuations);
        }

        [Fact]
        public async Task MergeWith__ToVoid_BothSynchronous()
        {
            var operation1 = new MockAsyncOperation(true);
            var operation2 = new MockVoidAsyncOperation(true);
            var task = operation1.MergeWith(null, operation2, null);
            Assert.True(task.IsCompletedSuccessfully);
            Assert.Equal(1, operation1.Continuations);
            Assert.Equal(1, operation2.Continuations);
            await task;
        }

        [Fact]
        public async Task MergeWith_ToVoid_FirstSyncSecondAsync()
        {
            var operation1 = new MockAsyncOperation(true);
            var operation2 = new MockVoidAsyncOperation(false);
            var task = operation1.MergeWith(null, operation2, null);
            Assert.False(task.IsCompletedSuccessfully);
            Assert.Equal(0, operation1.Continuations);
            await task;
            Assert.Equal(1, operation1.Continuations);
            Assert.Equal(1, operation2.Continuations);
        }

        [Fact]
        public async Task MergeWith_ToVoid_BothAsync()
        {
            var operation1 = new MockAsyncOperation(false);
            var operation2 = new MockVoidAsyncOperation(false);
            var task = operation1.MergeWith(null, operation2, null);
            Assert.False(task.IsCompletedSuccessfully);
            await task;
            Assert.Equal(1, operation1.Continuations);
            Assert.Equal(1, operation2.Continuations);
        }

        [Fact]
        public async Task MergeWith_ToResult_BothSynchronous()
        {
            var operation1 = new MockAsyncOperation(true);
            var operation2 = new MockAsyncOperation(true);
            var task = operation1.MergeWith(null, operation2, null);
            Assert.True(task.IsCompletedSuccessfully);
            Assert.Equal(0, task.Result);
            Assert.Equal(1, operation1.Continuations);
            Assert.Equal(1, operation2.Continuations);
            await task;
        }

        [Fact]
        public async Task MergeWith_ToResult_FirstSyncSecondAsync()
        {
            var operation1 = new MockAsyncOperation(true);
            var operation2 = new MockAsyncOperation(false);
            var task = operation1.MergeWith(null, operation2, null);
            Assert.False(task.IsCompletedSuccessfully);
            Assert.Equal(0, operation1.Continuations);
            await task;
            Assert.Equal(1, operation1.Continuations);
            Assert.Equal(1, operation2.Continuations);
        }

        [Fact]
        public async Task MergeWith_ToResult_BothAsync()
        {
            var operation1 = new MockAsyncOperation(false);
            var operation2 = new MockAsyncOperation(false);
            var task = operation1.MergeWith(null, operation2, null);
            Assert.False(task.IsCompletedSuccessfully);
            await task;
            Assert.Equal(1, operation1.Continuations);
            Assert.Equal(1, operation2.Continuations);
        }
    }
}