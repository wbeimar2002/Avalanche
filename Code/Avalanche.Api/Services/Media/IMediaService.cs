using Avalanche.Shared.Domain.Models;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Media
{
    public interface IMediaService
    {
        Task<CommandResponse> PlayAsync(Command command);
        Task<CommandResponse> HandleMessageAsync(Command command);
        Task<CommandResponse> StopAsync(Command command);
    }
}
