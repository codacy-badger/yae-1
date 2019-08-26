using System.Threading.Tasks;

namespace yae.Async
{
    public interface IAsyncOperation
    {
        ValueTask ExecuteAsync();
        ValueTask<T> MergeWith<T>(IAsyncOperation<T> operation);
        void OnFinally();
    }
    public interface IAsyncOperation<TResult>
    {
        ValueTask<TResult> ExecuteAsync();
        void OnFinally();
    }

    public interface IAsyncOperationV2<in TInput>
    {
        ValueTask ExecuteAsync(TInput input);
        //ValueTask<T> MergeWith<T, TIn>(TInput input, IAsyncOperation<T> operation, TIn operationInput);
        void OnFinally();
    }

    public abstract class AsyncOperationV2<TInput> : IAsyncOperationV2<TInput>
    {
        protected abstract ValueTask CanCompleteSync(TInput input);

        public ValueTask ExecuteAsync(TInput input)
        {
            async ValueTask AwaitTask(ValueTask t)
            {
                try { await t.ConfigureAwait(false); }
                finally { OnFinally(); }
            }

            var release = true;
            try
            {
                var task = CanCompleteSync(input);
                if (task.IsCompletedSuccessfully) return task;
                release = false;
                return AwaitTask(task);
            }
            finally
            {
                if (release) OnFinally();
            }
        }

        public void OnFinally()
        {
        }
    }


}