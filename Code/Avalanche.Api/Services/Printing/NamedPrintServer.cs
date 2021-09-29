using Ism.PrintServer.Client.V1;

namespace Avalanche.Api.Services.Printing
{
    public class NamedPrintServer
    {
        public string Name { get; private set; }
        public PrintingServerSecureClient Client { get; private set; }

        public NamedPrintServer(string name, PrintingServerSecureClient client)
        {
            Name = name;
            Client = client;
        }
    }
}
