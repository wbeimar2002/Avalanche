using System.Collections.Generic;
using Avalanche.Shared.Infrastructure.Configuration;

namespace Avalanche.Api.Managers.Maintenance
{
    public interface IServerConfigurationManager
    {
        ProceduresSearchConfiguration GetProceduresSearchConfigurationSettings();
        VaultStreamServerConfiguration GetVaultStreamServerConfigurationSettings();

        void UpdateProceduresSearchConfigurationColumns(List<ColumnProceduresSearchConfiguration> columnProceduresSearchConfigurations);
    }
}
