using Avalanche.Shared.Infrastructure.Helpers;
using Ism.Common.Core.Configuration.Models;
using Ism.Common.Core.Extensions;
using Ism.Security.Grpc.Configuration;
using Ism.Storage.Configuration.Client.V1;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Avalanche.Api
{
    [ExcludeFromCodeCoverage]
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

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.AppSettings()
                .Enrich.With(new ApplicationNameEnricher("Avalanche.Api"))
                .Enrich.FromLogContext()
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
                var configClient = ConfigurationServiceSecureClient.FromEnvironment();
                CreateWebHostBuilder(args, configClient).Build().Run();
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

        public static IWebHostBuilder CreateWebHostBuilder(string[] args, ConfigurationServiceSecureClient configClient) =>
            WebHost.CreateDefaultBuilder(args)
                .UseSerilog()
                .UseStartup<Startup>()
                .ConfigureAppConfiguration((hostingContext, configBuilder) =>
                {
                    configBuilder.UseIsmConfigurationPattern(
                        hostingContext.HostingEnvironment,
                        GetConfigurationServiceRequests(),
                        configClient,
                        args);
                })
                .UseKestrel(options =>
                {
                    options.Limits.MaxRequestBodySize = 209715200;
                });

        private static IEnumerable<ConfigurationServiceRequest> GetConfigurationServiceRequests()
        {
            // config types may be decorated with guid and version and other helper attributes
            var context = new ConfigurationContext();
            var requests = new List<ConfigurationServiceRequest>();

            // needed for grpc clients
            requests.Add(new ConfigurationServiceRequest(nameof(GrpcServiceRegistry), 1, context));

            return requests;
        }
    }
}
