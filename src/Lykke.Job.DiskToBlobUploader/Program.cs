﻿using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.PlatformAbstractions;

namespace Lykke.Job.DiskToBlobUploader
{
    internal sealed class Program
    {
        internal const int Port = 5000;

        public static string EnvInfo => Environment.GetEnvironmentVariable("ENV_INFO");

        public static async Task Main(string[] args)
        {
            Console.WriteLine($"{PlatformServices.Default.Application.ApplicationName} version {PlatformServices.Default.Application.ApplicationVersion}");
#if DEBUG
            Console.WriteLine("Is DEBUG");
#else
            Console.WriteLine("Is RELEASE");
#endif
            Console.WriteLine($"ENV_INFO: {EnvInfo}");

            try
            {
                var webHost = new WebHostBuilder()
                    .UseKestrel()
                    .UseUrls($"http://*:{Port}")
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .UseStartup<Startup>()
                    .UseApplicationInsights()
                    .Build();

                await webHost.RunAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fatal error:");
                Console.WriteLine(ex);

                // Lets devops to see startup error in console between restarts in the Kubernetes
                var delay = TimeSpan.FromMinutes(1);

                Console.WriteLine();
                Console.WriteLine($"Process will be terminated in {delay}. Press any key to terminate immediately.");

                await Task.WhenAny(
                            Task.Delay(delay),
                            Task.Run(() =>
                            {
                                Console.ReadKey(true);
                            }));
            }

            Console.WriteLine("Terminated");
        }
    }
}
