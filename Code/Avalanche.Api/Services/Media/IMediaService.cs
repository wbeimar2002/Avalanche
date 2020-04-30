using Avalanche.Shared.Domain.Models;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Media
{
    public interface IMediaService
    {
        Task<CommandResponse> Play(Command command);
        Task<CommandResponse> HandleMesssage(Command command);
        Task<CommandResponse> Stop(Command command);
    }
}
