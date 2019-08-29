using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Logging;
using yae.Framing.Sample.BasicFrame;

namespace yae.Framing.Server
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await CreateWebHostBuilder(args).Build().RunAsync();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseKestrel(options =>
                {
                    
                    options.ListenAnyIP(5000, builder =>
                    {
                        builder.UseConnectionHandler<MyConnectionHandler>();
                    });
                })
                .UseStartup<Startup>();
        }
    }

    class Startup : IDisposable
    {
        public void Dispose()
        {

        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime lifetime)
        {
            
            /*if (env.IsDevelopment()) app.UseDeveloperExceptionPage();
            app.Run(context => context.Response.WriteAsync($"clients: {_server.ClientCount}"));*/
        }
    }

    public static class BuilderExtensions
    {
        public static void UseFramedConnectionHandler(this ListenOptions builder)
        {
            //builder.UseConnectionHandler<FramedConnectionHandler>()
        }
    }

    public class FramedConnectionHandler : ConnectionHandler
    {
        public override Task OnConnectedAsync(ConnectionContext connection)
        {
            return Task.CompletedTask;
        }
    }
    class MyConnectionHandler : ConnectionHandler
    {
        private ILogger _logger;

        public MyConnectionHandler(ILogger<MyConnectionHandler> logger) => _logger = logger;
        public override async Task OnConnectedAsync(ConnectionContext connection)
        {
            try
            {
                _logger.LogInformation("Client connected");
                var decoder = new HeaderBasicFrameDecoder(connection.Transport.Input);
                ulong total = 0;
                var sw = Stopwatch.StartNew();
                await foreach (var frame in decoder.DecodeAsync())
                {
                    var payload = frame.Payload.Memory;
                    total += (ulong) payload.Length;
                    _logger.LogInformation($"Received frame with MessageId={frame.MessageId}, Length={payload.Length}");
                    Console.Title = $"Received {total / 1000.0 / 1000 / 1000} GB";
                }
                sw.Stop();
                _logger.LogInformation($"Client sent {total} bytes of data in {sw.ElapsedMilliseconds} ms");
                _logger.LogInformation("Client disconnected");
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex.ToString());
            }
        }
    }
}
