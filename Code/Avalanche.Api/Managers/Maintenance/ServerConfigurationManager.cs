using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Avalanche.Api.Services.Maintenance;
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
        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly ProceduresSearchConfiguration _proceduresSearchConfiguration;

        public ServerConfigurationManager(
            ProceduresSearchConfiguration proceduresSearchConfiguration,
            IMapper mapper)
        {
            _proceduresSearchConfiguration = proceduresSearchConfiguration;
            _mapper = mapper;

            _user = HttpContextUtilities.GetUser(_httpContextAccessor.HttpContext);
            _configurationContext = _mapper.Map<UserModel, ConfigurationContext>(_user);
            _configurationContext.IdnId = Guid.NewGuid().ToString();
        }

        public ProceduresSearchConfiguration GetProceduresSearchConfigurationSettings() => _proceduresSearchConfiguration;

        public void UpdateProceduresSearchConfigurationColumns(List<ColumnProceduresSearchConfiguration> columnProceduresSearchConfigurations)
        {
            _proceduresSearchConfiguration.Columns = columnProceduresSearchConfigurations;
        }
    }
}
