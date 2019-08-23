using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace yae.Sandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            var lr = new LoginRequest();
            var obj = (object) lr;
            int n = int.MaxValue;

            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < n; i++)
            {
                PassObj(lr);
            }

            sw.Stop();

            Console.WriteLine($"Pass raw = {sw.ElapsedMilliseconds} ms");
            sw.Restart();
            for (int i = 0; i < n; i++)
            {
                PassObj(obj);
            }

            sw.Stop();
            Console.WriteLine($"Pass as object = {sw.ElapsedMilliseconds} ms");
            Console.ReadLine();
        }

        public static T PassObj<T>(T obj)
        {
            return obj;
            //Console.WriteLine(obj.GetType());
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
