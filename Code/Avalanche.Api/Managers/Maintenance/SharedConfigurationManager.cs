using System;
using AutoMapper;
using Avalanche.Api.Utilities;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Configuration;
using Ism.Common.Core.Configuration.Models;
using Microsoft.AspNetCore.Http;

namespace Avalanche.Api.Managers.Maintenance
{
    public class SharedConfigurationManager : ISharedConfigurationManager
    {
        private readonly GeneralApiConfiguration _generalApiConfiguration;

        private readonly IMapper _mapper;
        private readonly UserModel _user;
        private readonly ConfigurationContext _configurationContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        //These values can be changed in execution time
        private PrintingConfiguration _printingConfiguration;
        private readonly MedPresenceConfiguration _medPresenceConfiguration;

        public SharedConfigurationManager(
            GeneralApiConfiguration generalApiConfiguration,
            PrintingConfiguration printingConfiguration,
            MedPresenceConfiguration medPresenceConfiguration,
            IMapper mapper)
        {
            _generalApiConfiguration = generalApiConfiguration;
            _printingConfiguration = printingConfiguration;
            _medPresenceConfiguration = medPresenceConfiguration;
            _mapper = mapper;

            _user = HttpContextUtilities.GetUser(_httpContextAccessor.HttpContext);
            _configurationContext = _mapper.Map<UserModel, ConfigurationContext>(_user);
            _configurationContext.IdnId = Guid.NewGuid().ToString();
        }

        public GeneralApiConfiguration GetGeneralApiConfigurationSettings() => _generalApiConfiguration;

        public PrintingConfiguration GetPrintingConfigurationSettings() => _printingConfiguration;

        public MedPresenceConfiguration GetMedPresenceConfigurationSettings() => _medPresenceConfiguration;

        public void UseVSSPrintingService(bool useVSSPrintingService)
        {
            _printingConfiguration.UseVSSPrintingService = useVSSPrintingService;
        }
    }
}
