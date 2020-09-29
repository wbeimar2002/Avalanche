using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Media
{
    public interface IAvidisService
    {
        Task HidePreview(AvidisDeviceInterface.V1.Protos.HidePreviewRequest hidePreviewRequest);
        Task ShowPreview(AvidisDeviceInterface.V1.Protos.ShowPreviewRequest showPreviewRequest);
        Task RoutePreview(AvidisDeviceInterface.V1.Protos.RoutePreviewRequest routePreviewRequest);
    }
}
