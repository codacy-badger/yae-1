using System;
using System.Threading.Tasks;
using Xunit;

namespace yae.Async.Tests
{
    public class VoidAsyncOperationTests
    {

        [Fact]
        public async Task ExecuteAsync_ShouldBeSynchronous()
        {
            var operation = new MockVoidAsyncOperation(true);
            var task = operation.ExecuteAsync(null);
            Assert.True(task.IsCompletedSuccessfully);
            Assert.Equal(1, operation.Continuations);
            await task;
        }

        [Fact]
        public async Task ExecuteAsync_ShouldBeAsynchronous()
        {
            var operation = new MockVoidAsyncOperation(false);
            var task = operation.ExecuteAsync(null);
            Assert.False(task.IsCompletedSuccessfully);
            Assert.Equal(0, operation.Continuations);
            await task;
            Assert.Equal(1, operation.Continuations);
        }

        [Fact]
        public async Task MergeWith_ToVoid_BothSynchronous()
        {
            var (operation1, operation2) = MergeWith_ToResult_Void_Void(true, true);
            var task = operation1.MergeWith(null, operation2, null);
            Assert.True(task.IsCompletedSuccessfully);
            Assert.Equal(1, operation1.Continuations);
            Assert.Equal(1, operation2.Continuations);
            await task;
        }
        
        [Fact]
        public async Task MergeWith_ToVoid_FirstSyncSecondAsync()
        {
            var (operation1, operation2) = MergeWith_ToResult_Void_Void(true, false);
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
            var (operation1, operation2) = MergeWith_ToResult_Void_Void(false, false);
            var task = operation1.MergeWith(null, operation2, null);
            Assert.False(task.IsCompletedSuccessfully);
            await task;
            Assert.Equal(1, operation1.Continuations);
            Assert.Equal(1, operation2.Continuations);
        }

        [Fact]
        public async Task MergeWith_ToResult_BothSynchronous()
        {
            var (operation1, operation2) = MergeWith_ToResult_Void_Async(true, true);
            var task = operation1.MergeWith(null, operation2, null);
            Assert.True(task.IsCompletedSuccessfully);
            Assert.Equal(1, operation1.Continuations);
            Assert.Equal(1, operation2.Continuations);
            Assert.Equal(int.MinValue, await task);
        }

        [Fact]
        public async Task MergeWith_ToResult_FirstSyncSecondAsync()
        {
            var (operation1, operation2) = MergeWith_ToResult_Void_Async(true, false);
            var task = operation1.MergeWith(null, operation2, null);
            Assert.False(task.IsCompletedSuccessfully);
            Assert.Equal(0, operation1.Continuations);
            await task;
            Assert.Equal(1, operation1.Continuations);
            Assert.Equal(1, operation2.Continuations);
            Assert.Equal(int.MaxValue, await task);
        }

        [Fact]
        public async Task MergeWith_ToResult_BothAsync()
        {
            var (operation1, operation2) = MergeWith_ToResult_Void_Async(false, false);
            var task = operation1.MergeWith(null, operation2, null);
            Assert.False(task.IsCompletedSuccessfully);
            await task;
            Assert.Equal(1, operation1.Continuations);
            Assert.Equal(1, operation2.Continuations);
            Assert.Equal(int.MaxValue, await task);
        }

        private static (MockVoidAsyncOperation operation1, MockAsyncOperation operation2) MergeWith_ToResult_Void_Async(bool op1, bool op2) 
            => (new MockVoidAsyncOperation(op1), new MockAsyncOperation(op2));

        private static (MockVoidAsyncOperation operation1, MockVoidAsyncOperation operation2) MergeWith_ToResult_Void_Void( bool op1, bool op2) 
            => (new MockVoidAsyncOperation(op1), new MockVoidAsyncOperation(op2));
    }
}
