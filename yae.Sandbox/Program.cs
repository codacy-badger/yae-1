using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace yae.Sandbox
{
    class Program
    {
        static async Task Main(string[] args)
        {
            foreach (var obj in GetEnumerable())
            {
                PrintObj(obj);
            }

            await foreach (var obj in GetAsyncEnumerable())
            {
                PrintObj(obj);
            }
            Console.ReadLine();
        }

        public static void PrintObj<T>(T obj)
        {
            Console.WriteLine(typeof(T));
        }

        public static IEnumerable<object> GetEnumerable()
        {
            yield return new LoginRequest();
            yield return new object();
            yield return AsObject();
        }
        public static async IAsyncEnumerable<object> GetAsyncEnumerable()
        {
            foreach (var obj in GetEnumerable())
            {
                yield return obj;
                await Task.Delay(0);
            }

        }

        public static object AsObject()
        {
            return (object)new LoginRequest(); //force cast, should it helps ? no...
        }
    }

    class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class Frame
    {
        public int MessageId { get; set; }
        public Memory<byte> Payload { get; set; }
    }

    public class Client
    {
        public ValueTask OnReceiveAsync(Frame frame)
        {
            var dispatcher = new Dispatcher(); //cache it
            
            return dispatcher.DispatchAsync(frame);
        }
    }

    public static class SerializerStore<T>
    {
        public static Func<ReadOnlyMemory<byte>, T> Deserialize { get; set; }
    }

    public static class HandlerStore<T>
    {
        public static Func<T, ValueTask> Handler { get; set; } //we may register not as a static class!
    }


    public class Dispatcher
    {
        public Dictionary<int, Func<Frame, ValueTask>> Mapper { get; }

        public void AddMapping<T>(int id)
        {
            ValueTask Fnc(Frame frm)
            {
                var message = SerializerStore<T>.Deserialize(frm.Payload);
                return HandlerStore<T>.Handler(message); //may return IAsycnEnumerable if you want to
            }

            Mapper.Add(id, Fnc);
        }

        public ValueTask DispatchAsync(Frame frame)
        {
            Mapper.TryGetValue(frame.MessageId, out var handler);
            return handler(frame) ;
        }
    }

}
