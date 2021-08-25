using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Books
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            Console.Write ("Starting ... ");

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            using (var scope = host.Services.CreateScope())
            {
                var bookRepository = scope.ServiceProvider.GetRequiredService<IBookRepository>();
                await bookRepository.Load();
            }

            stopWatch.Stop();

            Console.WriteLine($"Precaching of all books took about {stopWatch.ElapsedMilliseconds}ms");
            
            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
