using AvidisDeviceInterface.V1.Protos;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Media
{
    public interface IAvidisService
    {
        Task ShowPreview(ShowPreviewRequest showPreviewRequest);

        Task HidePreview(HidePreviewRequest hidePreviewRequest);

    }
}
