using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ism.Security.Grpc.Models
{
    public class CertificateInfoBase
    {
        public string SubjectName { get; internal set; }
        public string Thumbprint { get; internal set; }
    }
}
