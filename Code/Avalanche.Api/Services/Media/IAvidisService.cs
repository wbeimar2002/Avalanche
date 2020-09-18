using AvidisDeviceInterface.Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Media
{
    public interface IAvidisService
    {
        Task SetPreviewVisible(SetPreviewVisibleRequest setPreviewVisibleRequest);
        Task SetPreviewRegion(SetPreviewRegionRequest setPreviewRegionRequest);
        Task RoutePreview(RoutePreviewRequest routePreviewRequest);
    }
}
