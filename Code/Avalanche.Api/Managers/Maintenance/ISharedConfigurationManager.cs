using Avalanche.Shared.Infrastructure.Configuration;

namespace Avalanche.Api.Managers.Maintenance
{
    public interface ISharedConfigurationManager
    {
        GeneralApiConfiguration GetGeneralApiConfigurationSettings();
        PrintingConfiguration GetPrintingConfigurationSettings();
        MedPresenceConfiguration GetMedPresenceConfigurationSettings();

        void UseVSSPrintingService(bool useVSSPrintingService);
    }
}
