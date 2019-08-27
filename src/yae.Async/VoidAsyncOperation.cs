using System.Runtime.CompilerServices;
using System.Threading.Tasks;

//todo: may I can implement an API delegate-based?
namespace yae.Async
{
    public abstract class VoidAsyncOperation<TState> //: IAsyncOperation<TState, ValueTask>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract ValueTask CanExecuteSynchronous(TState input);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract void Continuation(TState input);
        public ValueTask ExecuteAsync(TState input)
        {
            async ValueTask AwaitTask(ValueTask t)
            {
                try
                {
                    await t.ConfigureAwait(false);
                }
                finally
                {
                    Continuation(input);
                }
            }

            var continuation = true;
            try
            {
                var task = CanExecuteSynchronous(input);
                if (task.IsCompletedSuccessfully) return default;
                continuation = false;
                return AwaitTask(task);
            }
            finally
            {
                if (continuation) Continuation(input);
            }
        }

        /// <summary>
        /// <see cref="ValueTask"/> to <see cref="ValueTask"/>
        /// </summary>
        /// <typeparam name="TIn"></typeparam>
        /// <param name="input"></param>
        /// <param name="operation"></param>
        /// <param name="input2"></param>
        /// <returns></returns>
        public ValueTask MergeWith<TIn>(TState input, VoidAsyncOperation<TIn> operation, TIn input2)
        {
            async ValueTask AwaitMerge(ValueTask t)
            {
                try
                {
                    await t.ConfigureAwait(false);
                }
                finally
                {
                    Continuation(input);
                }
            }
            async ValueTask AwaitBoth(ValueTask t1, ValueTask t2)
            {
                try
                {
                    await t1.ConfigureAwait(false);
                    await t2.ConfigureAwait(false);
                }
                finally
                {
                    Continuation(input);
                }
            }

            var release = true;
            try
            {
                var operationTask = CanExecuteSynchronous(input);
                
                if (!operationTask.IsCompletedSuccessfully)
                {
                    release = false;
                    return AwaitBoth(operationTask, operation.ExecuteAsync(input2));
                }

                var task = operation.ExecuteAsync(input2);
                if (task.IsCompletedSuccessfully) return default;
                release = false;
                return AwaitMerge(task);
            }
            finally
            {
                if(release) Continuation(input);
            }
        }

        /// <summary>
        /// <see cref="ValueTask"/> to <see cref="ValueTask{TResult}"/>
        /// </summary>
        /// <typeparam name="TIn"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="input"></param>
        /// <param name="operation"></param>
        /// <param name="input2"></param>
        /// <returns></returns>
        public ValueTask<TResult> MergeWith<TIn, TResult>(TState input, AsyncOperation<TIn, TResult> operation, TIn input2)
        {
            async ValueTask<TResult> AwaitMerge(ValueTask<TResult> t)
            {
                try
                {
                    return await t.ConfigureAwait(false);
                }
                finally
                {
                    Continuation(input);
                }
            }
            async ValueTask<TResult> AwaitBoth(ValueTask t1, ValueTask<TResult> t2)
            {
                try
                {
                    await t1.ConfigureAwait(false);
                    return await t2.ConfigureAwait(false);
                }
                finally
                {
                    Continuation(input);
                }
            }

            var release = true;
            try
            {
                var operationTask = CanExecuteSynchronous(input);

                if (!operationTask.IsCompletedSuccessfully)
                {
                    release = false;
                    return AwaitBoth(operationTask, operation.ExecuteAsync(input2));
                }

                var task = operation.ExecuteAsync(input2);
                if (task.IsCompletedSuccessfully) return task;
                release = false;
                return AwaitMerge(task);
            }
            finally
            {
                if (release) Continuation(input);
            }
        }
    }
}