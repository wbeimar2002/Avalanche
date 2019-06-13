using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MonsoonAPI.Models;

namespace MonsoonAPI.Services {
    internal class NetworkingUtilities : INetworkingUtilities {
        readonly IMonsoonResMgr _rm;
        readonly int _timeout = 10000;

        public NetworkingUtilities(IConfiguration config, IMonsoonResMgr rm) {
            _rm = rm;
            var timeout = config.GetValue<int>("pingTimeout");
            if (timeout > 0) {
                _timeout = timeout;
            }
        }

        public async Task<bool> IsIpValid(IpAddressValidationRequest validationReq) {
            _rm?.LogEvent(EventLogEntryType.Information, 0, $"Attempting to ping IP {validationReq.IpAddress} requested by {validationReq.Login}.", 3);

            var pinger = new Ping();
            var opt = new PingOptions() {
                DontFragment = true,
            };

            var buff = Encoding.ASCII.GetBytes("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx");

            var reply = pinger.Send(IPAddress.Parse(validationReq.IpAddress), _timeout, buff, opt);
            return reply != null && await Task.FromResult(reply.Status == IPStatus.Success);
        }
    }
}