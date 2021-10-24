using System;
using System.Collections.Generic;
using AutoMapper;
using Avalanche.Api.Utilities;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Configuration;
using Ism.Common.Core.Configuration.Models;
using Microsoft.AspNetCore.Http;

namespace Avalanche.Api.Managers.Maintenance
{
    public class ServerConfigurationManager : IServerConfigurationManager
    {
        private readonly IMapper _mapper;
        private readonly UserModel _user;
        private readonly ConfigurationContext _configurationContext;

        private readonly ProceduresSearchConfiguration _proceduresSearchConfiguration;
        private readonly VaultStreamServerConfiguration _vaultStreamServerConfiguration;

        public ServerConfigurationManager(
            ProceduresSearchConfiguration proceduresSearchConfiguration,
            VaultStreamServerConfiguration vaultStreamServerConfiguration,
            IMapper mapper)
        {
            _proceduresSearchConfiguration = proceduresSearchConfiguration;
            _vaultStreamServerConfiguration = vaultStreamServerConfiguration;
            _mapper = mapper;
        }

        public ProceduresSearchConfiguration GetProceduresSearchConfigurationSettings() => _proceduresSearchConfiguration;

        public VaultStreamServerConfiguration GetVaultStreamServerConfigurationSettings() => _vaultStreamServerConfiguration;

        public void UpdateProceduresSearchConfigurationColumns(List<ColumnProceduresSearchConfiguration> columnProceduresSearchConfigurations)
        {
            _proceduresSearchConfiguration.Columns = columnProceduresSearchConfigurations;
        }
    }
}
