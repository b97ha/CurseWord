using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;

namespace MovieProfanityDetector
{
    public class Program
    {

        public static async Task Main(string[] args)
        {

            var host = CreateHostBuilder(args).Build();
          
            using (IServiceScope scope = host.Services.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;
                var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
                try
                {
                    //var db = serviceProvider.GetRequiredService<ApplicationDbContext>();
                    //var seed = new ContextSeed(db);
                    //await seed.SeedAsync();


                    var ffmpegPath = Path.Combine(Directory.GetCurrentDirectory(), "FFmpeg");
                    //Set directory where app should look for FFmpeg 
                    FFmpeg.SetExecutablesPath(ffmpegPath);
                    
                    //Get latest version of FFmpeg. It's great idea if you don't know if you had installed FFmpeg.
                    await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official, ffmpegPath);

                }
                catch (Exception ex)
                {
                    loggerFactory.CreateLogger<Program>().LogError(ex, "An error occurred seeding the DB.");
                }
            }

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
