using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;

namespace MonsoonAPI {
    public class Program {
        public static void Main(string[] args) {
            var config = new ConfigurationBuilder()
                                .SetBasePath(Directory.GetCurrentDirectory())
                                .AddJsonFile("appsettings.json")
                                .Build();

            var ssl = config.GetValue("useSsl", false);
            var certSubject = config.GetValue("sslSubject", "Image Stream Medical WCF");
            var standardIp = GetIp(config.GetValue("hostOn", ""));

            var host = new WebHostBuilder()
                .UseStartup<Startup>()
                .UseKestrel(o => {
                    o.Limits.MaxRequestBodySize = long.MaxValue;
                    o.Limits.KeepAliveTimeout = new TimeSpan(0,0,10,0);
                    o.Listen(standardIp, 5000);
                    if (ssl) {
                        o.Listen(standardIp, 5001, lo => {
                            lo.UseHttps(GetCert(certSubject));
                        });
                    }
                })
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseApplicationInsights()
                .Build();

            host.Run();

            try { ResMgrInstance.GetResMgrInstance()?.LogEvent(EventLogEntryType.Information, 0, "MonsoonAPI is now shutting down.", 1); }
            catch { /* DO NOTHING */ }
        }

        static IPAddress GetIp(string whichOne) {
            if (string.IsNullOrWhiteSpace(whichOne)) { return IPAddress.Loopback; }
            if (whichOne.Equals("any", StringComparison.InvariantCultureIgnoreCase)) { return IPAddress.Any; }

            return whichOne.Equals("loopback", StringComparison.InvariantCultureIgnoreCase) ? IPAddress.Loopback : IPAddress.Parse(whichOne);
        }

        static X509Certificate2 GetCert(string subject) {
            using (var store = new X509Store(StoreLocation.LocalMachine)) {
                store.Open(OpenFlags.ReadOnly);

                var results = store.Certificates.Find(X509FindType.FindBySubjectName, subject, false);

                store.Close();
                return results.Count > 0 ? results[0] : null;
            }
        }
    }
}
