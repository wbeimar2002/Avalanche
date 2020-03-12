using Grpc.Core;
using Ism.Security.Grpc.Models;
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
        public static CertificateInfo Verify(X509Certificate2 leaf, X509Certificate2 authority, List<CertificateInfo> certificatesInChain)
        {
            X509Chain chain = new X509Chain();

            chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
            chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;

            chain.ChainPolicy.ExtraStore.Add(authority);
            var result = chain.Build(leaf); //For docker don't verify the result of this because is validated on the certificate store.

            if (certificatesInChain.Count == chain.ChainElements.Count)
            {
                for (int i = 0; i < certificatesInChain.Count; i++)
                {
                    if (!certificatesInChain[0].SubjectName.Equals(chain.ChainElements[0].Certificate.SubjectName)
                        || !certificatesInChain[0].Thumbprint.Equals(chain.ChainElements[0].Certificate.Thumbprint))
                    {
                        return new CertificateInfo()
                        {
                            SubjectName = leaf.SubjectName.Name,
                            Thumbprint = leaf.Thumbprint,
                            IsValid = false
                        };
                    }
                }

                return new CertificateInfo()
                {
                    SubjectName = leaf.SubjectName.Name,
                    Thumbprint = leaf.Thumbprint,
                    IsValid = true
                };
            }
            else
            {
                return new CertificateInfo()
                {
                    SubjectName = leaf.SubjectName.Name,
                    Thumbprint = leaf.Thumbprint,
                    IsValid = false
                };
            }
        }

        public static CertificateInfo Verify(X509Certificate2 leaf, X509Certificate2 authority)
        {
            X509Chain chain = new X509Chain();

            chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
            chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;

            chain.ChainPolicy.ExtraStore.Add(authority);

            // Do the preliminary validation.
            if (chain.Build(leaf))
            {
                // This piece makes sure it actually matches your known root
                var valid = chain.ChainElements
                    .Cast<X509ChainElement>()
                    .Any(x => x.Certificate.Thumbprint == authority.Thumbprint);

                return new CertificateInfo()
                {
                    SubjectName = leaf.SubjectName.Name,
                    Thumbprint = leaf.Thumbprint,
                    IsValid = valid,
                };
            }
            else
                return new CertificateInfo()
                {
                    SubjectName = leaf.SubjectName.Name,
                    IsValid = false,
                };
        }

        public static CertificateInfo Verify(X509Certificate2 cert)
        {
            X509Chain chain = new X509Chain();
            chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
            chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;

            bool validChain = chain.Build(cert);

            if (validChain)
            {
                X509Certificate2Collection chainCerts = new X509Certificate2Collection();

                foreach (var element in chain.ChainElements)
                {
                    chainCerts.Add(element.Certificate);
                }

                return new CertificateInfo()
                {
                    Thumbprint = cert.Thumbprint,
                    SubjectName = cert.SubjectName.Name,
                    IsValid = true,
                    Chain = chainCerts,
                };
            }
            else 
            { 
                return new CertificateInfo()
                {
                    Thumbprint = cert.Thumbprint,
                    SubjectName = cert.SubjectName.Name,
                    IsValid = false,
                };
            }
        }

        public static CertificateInfo Verify(string leafSubjectName)
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

                    if (chain.Build(cert))
                    {
                        return new CertificateInfo()
                        {
                            SubjectName = leafSubjectName,
                            IsValid = true,
                            Chain = findResult
                        };
                    }
                    else
                    {
                        return new CertificateInfo()
                        {
                            Chain = findResult,
                            SubjectName = leafSubjectName,
                            IsValid = false,
                        };
                    }
                }

                //Empty chain
                return new CertificateInfo()
                {
                    SubjectName = leafSubjectName,
                    IsValid = false,
                };
            }
            catch
            {
                return new CertificateInfo()
                {
                    SubjectName = leafSubjectName,
                    IsValid = false,
                };
            }
            finally
            {
                computerCaStore.Close();
            }
        }
    }
}
