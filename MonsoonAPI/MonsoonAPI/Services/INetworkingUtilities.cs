using System.Threading.Tasks;
using MonsoonAPI.Models;

namespace MonsoonAPI.Services {
    public interface INetworkingUtilities {
        Task<bool> IsIpValid(IpAddressValidationRequest validationReq);
    }
}