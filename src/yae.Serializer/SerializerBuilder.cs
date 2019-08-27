using System;
using System.Buffers;

namespace yae.Serializer
{
    public abstract class SerializerBuilder<TState, TStore>
    {
        public SerializerBuilder<TState, TStore> Register<T>(IFormatter<TState, T> formatter)
        {
            SerializerStorage<TStore, TState, T>.Formatter = formatter;
            return this;
        }

        public Serializer<TState, TStore> Build() => new Serializer<TState, TStore>();
    }

    public class Serializer<TState, TStore>
    {
        private static class SerializerStore<T>
        {

        }
        public IMemoryOwner<byte> Serialize<T>(T item)
        {
            //todo: add unmanaged formatter, nested classes etc...
            return default;
        }
    }
}