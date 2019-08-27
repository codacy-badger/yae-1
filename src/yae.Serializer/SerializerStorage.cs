namespace yae.Serializer
{
    //should we snap the TStore?
    internal static class SerializerStorage<TStore, TState, T>
    {
        public static IFormatter<TState, T> Formatter { get; set; }
    }
}
