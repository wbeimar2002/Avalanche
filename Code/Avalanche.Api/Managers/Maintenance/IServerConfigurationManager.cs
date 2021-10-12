using System.Collections.Generic;
using Avalanche.Shared.Infrastructure.Configuration;

namespace Avalanche.Api.Managers.Maintenance
{
    public interface IServerConfigurationManager
    {
        ProceduresSearchConfiguration GetProceduresSearchConfigurationSettings();

        void UpdateProceduresSearchConfigurationColumns(List<ColumnProceduresSearchConfiguration> columnProceduresSearchConfigurations);
    }
}
