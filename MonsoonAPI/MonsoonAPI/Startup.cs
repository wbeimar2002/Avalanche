using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MonsoonAPI.Services;
using MonsoonAPI.Controllers.Compatibility;
using MonsoonAPI.Middleware;
using Newtonsoft.Json.Serialization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Localization;
using IsmUtility;
using Microsoft.AspNetCore.Http;

namespace MonsoonAPI
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEncryptedJsonFile("appsettings.encrypted.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //this takes our supported languages and creates a list of just the culture infos
            var languages = LocalizationUtilities.SupportedLanguages.SelectMany(x => x.Value.AllCultures).ToList();
            services.Configure<RequestLocalizationOptions>(options =>
            {
                //a request must specify the culture exactly (fr-FR for example). If it does not, the default (en-US) is used
                options.DefaultRequestCulture = new RequestCulture(LocalizationUtilities.GetLanguageInfo(Language.English).PrimaryCulture);
                options.SupportedCultures = languages;
                options.SupportedUICultures = languages;
                //these would make fr-CA fallback to fr
                options.FallBackToParentCultures = true;
                options.FallBackToParentUICultures = true;
                //ideally where would be some way fr fall forward to fr-FR, but that doesn't make a lot of sense
            });

            services.AddMvc()
            .AddJsonOptions(opt =>
            {
                var resolver = opt.SerializerSettings.ContractResolver;
                if (resolver is DefaultContractResolver res) res.NamingStrategy = null; // this removes the camelcasing

                opt.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter // serialize enums as strings!
                {
                    CamelCaseText = false
                });

            });

            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.CookieHttpOnly = true;
                options.IdleTimeout = TimeSpan.FromSeconds(10);
            });

            // note - awful hack because log client sometimes fails with an obscure security policy error checking the config section when done shortly after machine boot and I can't figure out why.
            //      - retrying a little later, it is fine
            //      - other services init before monsoon api and seem to be fine.....
            //      - see FB20257
            IMonsoonResMgr rm = null;
            for (int i = 0; (null == rm) && (i < 5); i++)
            {
                try
                {
                    rm = ResMgrInstance.GetResMgrInstance();
                }
                catch // resmgr init will log errors...and we don't have a good way to here, anyways
                {
                    System.Threading.Thread.Sleep(5000);
                }
            }

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton(typeof(IMonsoonResMgr), ResMgrInstance.GetResMgrInstance());
            services.AddSingleton<ISettingsMgr, SettingsMgr>();
            services.AddSingleton<IMwMgr, MwMgr>();
            services.AddSingleton<ILibMgr, LibMgr>();
            services.AddSingleton<IActiveMgr, ActiveMgr>();
            services.AddSingleton<IPrintMgr, PrintMgr>();
            services.AddSingleton<INodeMgr, NodeMgr>();
            services.AddSingleton<IConfiguration>(Configuration);
            services.AddSingleton<INetworkingUtilities, NetworkingUtilities>();
            services.AddSingleton<CompatibilityGlobal>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseSession();
            app.UseResponseBuffering();
            
            //CultureInfo.CurrentCulture will be set based on the request headers
            app.UseRequestLocalization();

            app.UseMvc();
            app.UseIpGateway();
        }
    }
}
