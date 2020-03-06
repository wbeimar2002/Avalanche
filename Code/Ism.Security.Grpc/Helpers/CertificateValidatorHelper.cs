using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Ism.Security.Grpc.Helpers
{
    public static class CertificateValidatorHelper
    {
        public static bool Verify(X509Certificate2 leaf, X509Certificate2 authority)
        {
            X509Chain chain = new X509Chain();

            chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
            chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;

            chain.ChainPolicy.ExtraStore.Add(authority);

            // Do the preliminary validation.
            if (!chain.Build(leaf))
                return false;

            // This piece makes sure it actually matches your known root
            var valid = chain.ChainElements
                .Cast<X509ChainElement>()
                .Any(x => x.Certificate.Thumbprint == authority.Thumbprint);

            if (!valid)
                return false;

            return true;
        }

        public static bool Verify(string leafSubjectName)
        {
            X509Store computerCaStore = new X509Store(StoreName.CertificateAuthority, StoreLocation.LocalMachine);
            try
            {
                computerCaStore.Open(OpenFlags.ReadOnly);

                X509Certificate2Collection certificatesInStore = computerCaStore.Certificates;
                X509Certificate2Collection findResult = certificatesInStore.Find(X509FindType.FindBySubjectName, leafSubjectName, false);

                foreach (X509Certificate2 cert in findResult)
                {
                    X509Chain chain = new X509Chain();
                    X509ChainPolicy chainPolicy = new X509ChainPolicy()
                    {
                        RevocationMode = X509RevocationMode.Online,
                        RevocationFlag = X509RevocationFlag.EntireChain
                    };
                    chain.ChainPolicy = chainPolicy;
                    if (!chain.Build(cert))
                    {
                        foreach (X509ChainElement chainElement in chain.ChainElements)
                        {
                            foreach (X509ChainStatus chainStatus in chainElement.ChainElementStatus)
                            {
                                Debug.WriteLine(chainStatus.StatusInformation);
                            }
                        }
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                computerCaStore.Close();
            }
        }

        public static bool Verify(X509Certificate2 cert)
        {
            X509Chain chain = new X509Chain();
            chain.ChainPolicy.VerificationFlags = X509VerificationFlags.IgnoreNotTimeValid;

            bool validChain = chain.Build(cert);

            if (!validChain)
            {
                // Whatever you want to do about that.
                foreach (var status in chain.ChainStatus)
                {
                    // In reality you can == this, since X509Chain.ChainStatus builds
                    // an object per flag, but since it's [Flags] let's play it safe.
                    if ((status.Status & X509ChainStatusFlags.PartialChain) != 0)
                    {
                        // Incomplete chain.
                    }
                }
            }

            X509Certificate2Collection chainCerts = new X509Certificate2Collection();

            foreach (var element in chain.ChainElements)
            {
                chainCerts.Add(element.Certificate);
            }

            return true;
        }
    }
}
