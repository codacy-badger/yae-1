using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading;

namespace yae.Buffers
{
    internal static class DynamicDelegateStore<TStore, T>
    {
        public static SequenceReaderBuilder<TStore>.DynamicReadDelegate<T> Dynamic { get; set; }
    }

    internal static class FixedDelegateStore<TStore, T>
        where T : unmanaged
    {
        public static SequenceReaderBuilder<TStore>.FixedReadDelegate<T> Fixed { get; set; }
    }

    public class SequenceReaderBuilder<T>
    {
        public SequenceReaderFactory<T> Build()
        {
            return new SequenceReaderFactory<T>();
        }

        public delegate TOut FixedReadDelegate<TOut>(ReadOnlySpan<byte> src) where TOut : unmanaged;
        public delegate TOut DynamicReadDelegate<TOut>(in ReadOnlySequence<byte> src, out int read);

        public SequenceReaderBuilder<T> AddFixed<TOut>(FixedReadDelegate<TOut> dlg) where TOut : unmanaged
        {
            FixedDelegateStore<T, TOut>.Fixed = dlg;
            return this;
        }

        public SequenceReaderBuilder<T> AddDynamic<TOut>(DynamicReadDelegate<TOut> dlg)
        {
            DynamicDelegateStore<T, TOut>.Dynamic = dlg;
            return this;
        }

    }

    public class SequenceReaderFactory<TStore>
    {
        internal SequenceReaderFactory() { }
        public SequenceReader<TStore> CreateSequenceReader()
        {
            return new SequenceReader<TStore>();
        }
    }

    // todo: must optimize later (with IsSingleSegment...)
    public ref struct SequenceReader<TStore>
    {
        private ReadOnlySequence<byte> _input;
        public int Offset { get; private set; }

        public SequenceReader(in ReadOnlySequence<byte> input)
        {
            _input = input;
            Offset = 0;
        }

        public unsafe bool TryRead<T>(out T result) where T : unmanaged
        {
            //todo: assert its initialized
            var size = sizeof(T);
            var dlg = FixedDelegateStore<TStore, T>.Fixed;
            if (dlg == null)
                throw new NotImplementedException();

            if(_input.Length < size)
            {
                result = default;
                return false;
            }

            //todo: fix algorithm (not always first!)
            if(_input.First.Length >= size)
            {
                result = FixedDelegateStore<TStore, T>.Fixed(_input.First.Span);
            }
            else
            {
                Span<byte> local = stackalloc byte[size];
                _input.Slice(0, size).CopyTo(local);
                result = FixedDelegateStore<TStore, T>.Fixed(_input.First.Span);
            }

            Offset += size;
            return true;
        }

        

    }
}
