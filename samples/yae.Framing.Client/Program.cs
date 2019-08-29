using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Pipelines.Sockets.Unofficial;
using yae.Framing.Sample.BasicFrame;

namespace yae.Framing.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                //await socket.ConnectAsync("127.0.0.1", 5000);
                await Task.Delay(2000);
                var conn = await SocketConnection.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 5000));
                var encoder = new BasicFrameEncoder(conn.Output);
                //var producer = conn.Output.AsPipeFrameProducer(new HeaderBasicFrameEncoder());
                await encoder.EncodeEnumerableAsync(ProduceFrames());
            }
            catch(Exception ex) 
            {
                Console.WriteLine(ex);
            }

            Console.ReadLine();
            Console.ReadLine();
        }

        public static int _messageId = 0;
        private static Random rdm = new Random();

        public static async IAsyncEnumerable<BasicFrame> ProduceFrames()
        {
            Memory<byte> array = new byte[ushort.MaxValue * 10];
            while (true)
            {
                yield return new BasicFrame {MessageId = _messageId++, Payload = array.Owned()};
                await Task.Delay(0);
                Console.WriteLine($"Sent {array.Length} bytes");
            }
        }
    }
}
