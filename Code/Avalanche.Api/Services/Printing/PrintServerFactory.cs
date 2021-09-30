using System;
using System.Collections.Generic;
using System.Linq;
using Ism.PrintServer.Client.V1;

namespace Avalanche.Api.Services.Printing
{
    public class PrintServerFactory
    {
        private readonly IDictionary<string, PrintingServerSecureClient> _clients;

        public PrintServerFactory(IEnumerable<NamedPrintServer> clients)
        {
            _clients = clients.ToDictionary(n => n.Name, n => n.Client);
        }

        public PrintingServerSecureClient GetClient(string name)
        {
            if (_clients.TryGetValue(name, out var client))
                return client;

            // handle error
            throw new ArgumentException(nameof(name));
        }
    }
}
