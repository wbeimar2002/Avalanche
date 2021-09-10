using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Medpresence
{
    public interface IMedpresenceService
    {
        Task StartServiceSession();
        Task StopServiceSession();
    }
}
