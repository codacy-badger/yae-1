using System.Threading.Tasks;

namespace yae.Async
{
    public abstract class AsyncOperation<T> : IAsyncOperation<T>
    {
        protected abstract ValueTask<T> CanCompleteSync();

        public ValueTask<T> ExecuteAsync()
        {
            async ValueTask<T> AwaitTask(ValueTask<T> t)
            {
                try { return await t.ConfigureAwait(false); }
                finally { OnFinally(); }
            }

            var release = true;
            try
            {
                var task = CanCompleteSync();
                if (task.IsCompletedSuccessfully) return task;
                release = false;
                return AwaitTask(task);
            }
            finally
            {
                if(release) OnFinally();
            }
        }

        public virtual void OnFinally()
        {
        }
    }

    public abstract class AsyncOperation : IAsyncOperation
    {
        protected abstract ValueTask CanCompleteSync();

        public ValueTask ExecuteAsync()
        {
            async ValueTask AwaitTaskFinally(ValueTask t)
            {
                try
                {
                    await t;
                }
                finally { OnFinally(); }
            }

            var release = true;
            try
            {
                var task = CanCompleteSync();
                if (task.IsCompletedSuccessfully) return default;
                release = false;
                return AwaitTaskFinally(task);
            }
            finally
            {
                if(release) 
                    OnFinally();
            }
        }

        public ValueTask<T> MergeWith<T>(IAsyncOperation<T> operation)
        {
            //worst case...we can't execute it synchronously, so we await both.
            async ValueTask<T> AwaitBoth(ValueTask task, ValueTask<T> continuation)
            {
                try
                {
                    await task.ConfigureAwait(false);
                    return await continuation.ConfigureAwait(false);
                }
                finally { OnFinally(); }
            }

            //a good case, but not the best - first operation was sync, but not the second one.
            async ValueTask<T> AwaitContinuation(ValueTask<T> continuation)
            {
                try
                {
                    return await continuation.ConfigureAwait(false);
                }
                finally
                {
                    OnFinally();
                }
            }

            var release = false;
            try
            {
                var task = CanCompleteSync();
                //release = false;

                if (!task.IsCompletedSuccessfully) return AwaitBoth(task, operation.ExecuteAsync());

                var continuation = operation.ExecuteAsync();
                if (!continuation.IsCompletedSuccessfully)
                    return AwaitContinuation(continuation);

                //best case: both were sync!
                release = true;
                return continuation;
            }
            finally
            {
                if (release) OnFinally();
            }
        }

        public virtual void OnFinally()
        {
        }
    }
}