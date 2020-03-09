using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Ism.Security.Grpc.Models
{
    public class CertificateInfo : CertificateInfoBase
    {
        public bool IsValid { get; internal set; }
        public X509Certificate2Collection Chain { get; internal set; }
    }
}
