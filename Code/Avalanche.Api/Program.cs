using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;
using Serilog.Events;
using Avalanche.Shared.Infrastructure.Models;
using Newtonsoft.Json;
using Avalanche.Shared.Infrastructure.Helpers;

namespace Avalanche.Api
{
    public class Program
    {
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
                CreateHostBuilder(args)
                    .Build()
                    .Run(); 
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
