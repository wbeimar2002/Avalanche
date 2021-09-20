using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Medpresence
{
    public interface IMedpresenceManager
    {
        Task StartServiceSession();
        Task StopServiceSession();
    }
}
