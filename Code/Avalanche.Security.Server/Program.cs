using Avalanche.Security.Server.Core.Security.Hashing;
using Avalanche.Security.Server.Persistence;
using Avalanche.Shared.Infrastructure.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using System;
using System.IO;

namespace Avalanche.Security.Server
{
	public class Program
	{
        /*public static void Main(string[] args)
		{
			var host = CreateHostBuilder(args).Build();

			using (var scope = host.Services.CreateScope())
			{
				var services = scope.ServiceProvider;
				var context = services.GetService<AppDbContext>();
				var passwordHasher = services.GetService<IPasswordHasher>();
				DatabaseSeed.Seed(context, passwordHasher);
			}

			host.Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args)
		{
			return Host.CreateDefaultBuilder(args)
										.ConfigureWebHostDefaults(webBuilder =>
										{
											webBuilder.UseStartup<Startup>();
										});
		}*/

        public static void Main(string[] args)
        {
            var logFile = Environment.GetEnvironmentVariable("loggerFileName") ?? "avalancheapilogs.txt";
            var logFolder = Environment.GetEnvironmentVariable("loggerFolder") ?? "/logs";

            var logFilePath = Path.Combine(logFolder, logFile);

            Int32 logFileSizeLimit = Convert.ToInt32(Environment.GetEnvironmentVariable("loggerFileSizeLimit") ?? "209715200");
            Int32 retainedFileCountLimit = Convert.ToInt32(Environment.GetEnvironmentVariable("loggerRetainedFileCountLimit") ?? "5");

            //https://github.com/serilog/serilog/wiki/Configuration-Basics
            string seqUrl = Environment.GetEnvironmentVariable("seqUrl") ?? "http://seq:5341";//"http://localhost:5341"; 

            LogEventLevel level = LogEventLevel.Information;
#if DEBUG
            level = LogEventLevel.Debug;
#endif

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.AppSettings()
                .Enrich.With(new ApplicationNameEnricher("Avalanche.Api"))
                .Enrich.FromLogContext()
#if DEBUG
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Debug)
                .MinimumLevel.Debug()
#endif
                .WriteTo.File(
                    path: logFilePath,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: retainedFileCountLimit,
                    fileSizeLimitBytes: logFileSizeLimit,
                    restrictedToMinimumLevel: level)
                .WriteTo.Seq(seqUrl)
                .WriteTo.Console(restrictedToMinimumLevel: level)
                .CreateLogger();

            try
            {
                var host = CreateHostBuilder(args).Build();

                using (var scope = host.Services.CreateScope())
                {
                    var services = scope.ServiceProvider;
                    var context = services.GetService<AppDbContext>();
                    var passwordHasher = services.GetService<IPasswordHasher>();
                    DatabaseSeed.Seed(context, passwordHasher);
                }

                host.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                throw;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}