using Microsoft.Extensions.DependencyInjection;
using System;
using CertificateManager;
using CertificateManager.Models;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using System.Collections.Generic;

namespace CreateChainedCertsConsoleDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            //LowLevelApiExamples.Run();

            var serviceProvider = new ServiceCollection()
                .AddCertificateManager()
                .BuildServiceProvider();
            Console.WriteLine("Type the DNS Name");
            string dnsName = Console.ReadLine();

            Console.WriteLine("Type the Prefix");
            string prefix = Console.ReadLine().Trim();

            var createClientServerAuthCerts = serviceProvider.GetService<CreateCertificatesClientServerAuth>();

            var rootCaL1 = createClientServerAuthCerts.NewRootCertificate(
                new DistinguishedName { CommonName = $"{prefix.ToLower()} root dev", Country = "US" },
                new ValidityPeriod { ValidFrom = DateTime.UtcNow, ValidTo = DateTime.UtcNow.AddYears(10) },
                3, dnsName);
            rootCaL1.FriendlyName = $"{prefix} Development Root L1 certificate";

            // Platform Intermediate L2 chained from root L1
            var intermediateCaL2 = createClientServerAuthCerts.NewIntermediateChainedCertificate(
                new DistinguishedName { CommonName = $"{prefix.ToLower()} platform intermediate dev", Country = "US" },
                new ValidityPeriod { ValidFrom = DateTime.UtcNow, ValidTo = DateTime.UtcNow.AddYears(10) },
                2, dnsName, rootCaL1);
            intermediateCaL2.FriendlyName = $"{prefix} Development Platform Intermediate L2 certificate";

            // Site Intermediate L3 chained from platform intermediate L2
            var intermediateCaL3 = createClientServerAuthCerts.NewIntermediateChainedCertificate(
                new DistinguishedName { CommonName = $"{prefix.ToLower()} site intermediate dev", Country = "US" },
                new ValidityPeriod { ValidFrom = DateTime.UtcNow, ValidTo = DateTime.UtcNow.AddYears(10) },
                2, dnsName, intermediateCaL2);
            intermediateCaL3.FriendlyName = $"{prefix} Development Site Intermediate L3 certificate";

            // Child-Site Intermediate L4 chained from Site intermediate L3
            var intermediateCaL4 = createClientServerAuthCerts.NewIntermediateChainedCertificate(
                new DistinguishedName { CommonName = $"{prefix.ToLower()} child-site intermediate dev", Country = "US" },
                new ValidityPeriod { ValidFrom = DateTime.UtcNow, ValidTo = DateTime.UtcNow.AddYears(10) },
                2, dnsName, intermediateCaL3);
            intermediateCaL4.FriendlyName = $"{prefix} Development Child-Site Intermediate L4 certificate";

            // Leaf-Child-Site L5 chained from Child-Site L4
            var serverL5 = createClientServerAuthCerts.NewServerChainedCertificate(
                new DistinguishedName { CommonName = $"{prefix.ToLower()} leaf child site server dev", Country = "US" },
                new ValidityPeriod { ValidFrom = DateTime.UtcNow, ValidTo = DateTime.UtcNow.AddYears(10) },
                dnsName, intermediateCaL4);

            serverL5.FriendlyName = $"{prefix} Development Leaf Child Site Server L5 certificate";

            var clientL5 = createClientServerAuthCerts.NewClientChainedCertificate(
                new DistinguishedName { CommonName = $"{prefix.ToLower()} client", Country = "US" },
                new ValidityPeriod { ValidFrom = DateTime.UtcNow, ValidTo = DateTime.UtcNow.AddYears(10) },
                dnsName, intermediateCaL4);
                        
            clientL5.FriendlyName = $"{prefix} Development Client L5 certificate";

            Console.WriteLine($"Created Client, Server L5 Certificates {clientL5.FriendlyName}");

            string password = "0123456789";
            var importExportCertificate = serviceProvider.GetService<ImportExportCertificate>();

            var rootCertInPfxBtyes = importExportCertificate.ExportRootPfx(password, rootCaL1);
            File.WriteAllBytes($"{prefix.ToLower()}_localhost_root_l1.pfx", rootCertInPfxBtyes);

            var rootPublicKey = importExportCertificate.ExportCertificatePublicKey(rootCaL1);
            var rootPublicKeyBytes = rootPublicKey.Export(X509ContentType.Cert);
            File.WriteAllBytes($"{prefix.ToLower()}_localhost_root_l1.cer", rootPublicKeyBytes);

            var intermediatePlatformCertInPfxBtyes = importExportCertificate.ExportChainedCertificatePfx(password, intermediateCaL2, rootCaL1);
            File.WriteAllBytes($"{prefix.ToLower()}_localhost_platform_intermediate_l2.pfx", intermediatePlatformCertInPfxBtyes);

            var intermediateSiteCertInPfxBtyes = importExportCertificate.ExportChainedCertificatePfx(password, intermediateCaL3, intermediateCaL2);
            File.WriteAllBytes($"{prefix.ToLower()}_localhost_site_intermediate_l3.pfx", intermediateSiteCertInPfxBtyes);

            var intermediateChildSiteCertInPfxBtyes = importExportCertificate.ExportChainedCertificatePfx(password, intermediateCaL4, intermediateCaL3);
            File.WriteAllBytes($"{prefix.ToLower()}_localhost_child_site_intermediate_l4.pfx", intermediateChildSiteCertInPfxBtyes);

            var leafChildSiteServerCertL5InPfxBtyes = importExportCertificate.ExportChainedCertificatePfx(password, serverL5, intermediateCaL4);
            File.WriteAllBytes($"{prefix.ToLower()}_serverl5.pfx", leafChildSiteServerCertL5InPfxBtyes);

            var clientCertL5InPfxBtyes = importExportCertificate.ExportChainedCertificatePfx(password, clientL5, intermediateCaL4);
            File.WriteAllBytes($"{prefix.ToLower()}_clientl5.pfx", clientCertL5InPfxBtyes);

            Console.WriteLine("Certificates exported to pfx and cer files");
        }

        static void OriginalSample(string[] args)
        {
            //LowLevelApiExamples.Run();

            var serviceProvider = new ServiceCollection()
                .AddCertificateManager()
                .BuildServiceProvider();

            var createClientServerAuthCerts = serviceProvider.GetService<CreateCertificatesClientServerAuth>();

            var rootCaL1 = createClientServerAuthCerts.NewRootCertificate(
                new DistinguishedName { CommonName = "root dev", Country = "IT" },
                new ValidityPeriod { ValidFrom = DateTime.UtcNow, ValidTo = DateTime.UtcNow.AddYears(10) },
                3, "localhost");
            rootCaL1.FriendlyName = "developement root L1 certificate";

            // Intermediate L2 chained from root L1
            var intermediateCaL2 = createClientServerAuthCerts.NewIntermediateChainedCertificate(
                new DistinguishedName { CommonName = "intermediate dev", Country = "FR" },
                new ValidityPeriod { ValidFrom = DateTime.UtcNow, ValidTo = DateTime.UtcNow.AddYears(10) },
                2,  "localhost", rootCaL1);
            intermediateCaL2.FriendlyName = "developement Intermediate L2 certificate";

            // Server, Client L3 chained from Intermediate L2
            var serverL3 = createClientServerAuthCerts.NewServerChainedCertificate(
                new DistinguishedName { CommonName = "server", Country = "DE" },
                new ValidityPeriod { ValidFrom = DateTime.UtcNow, ValidTo = DateTime.UtcNow.AddYears(10) },
                "localhost", intermediateCaL2);

            var clientL3 = createClientServerAuthCerts.NewClientChainedCertificate(
                new DistinguishedName { CommonName = "client", Country = "IE" },
                new ValidityPeriod { ValidFrom = DateTime.UtcNow, ValidTo = DateTime.UtcNow.AddYears(10) },
                "localhost", intermediateCaL2);
            serverL3.FriendlyName = "developement server L3 certificate";
            clientL3.FriendlyName = "developement client L3 certificate";
            
            Console.WriteLine($"Created Client, Server L3 Certificates {clientL3.FriendlyName}");

            string password = "1234";
            var importExportCertificate = serviceProvider.GetService<ImportExportCertificate>();

            var rootCertInPfxBtyes = importExportCertificate.ExportRootPfx(password, rootCaL1);
            File.WriteAllBytes("localhost_root_l1.pfx", rootCertInPfxBtyes);

            var rootPublicKey = importExportCertificate.ExportCertificatePublicKey(rootCaL1);
            var rootPublicKeyBytes = rootPublicKey.Export(X509ContentType.Cert);
            File.WriteAllBytes($"localhost_root_l1.cer", rootPublicKeyBytes);

            var intermediateCertInPfxBtyes = importExportCertificate.ExportChainedCertificatePfx(password, intermediateCaL2, rootCaL1);
            File.WriteAllBytes("localhost_intermediate_l2.pfx", intermediateCertInPfxBtyes);

            var serverCertL3InPfxBtyes = importExportCertificate.ExportChainedCertificatePfx(password, serverL3, intermediateCaL2);
            File.WriteAllBytes("serverl3.pfx", serverCertL3InPfxBtyes);

            var clientCertL3InPfxBtyes = importExportCertificate.ExportChainedCertificatePfx(password, clientL3, intermediateCaL2);
            File.WriteAllBytes("clientl3.pfx", clientCertL3InPfxBtyes);

            Console.WriteLine("Certificates exported to pfx and cer files");
        }
    }
}
