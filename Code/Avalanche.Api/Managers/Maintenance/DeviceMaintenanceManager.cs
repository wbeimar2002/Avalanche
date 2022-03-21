using System;
using System.Threading.Tasks;
using AutoMapper;
using Avalanche.Api.Managers.Data;
using Avalanche.Api.Services.Health;
using Avalanche.Api.Services.Maintenance;
using Avalanche.Api.Services.Media;
using Avalanche.Api.Services.Printing;
using Avalanche.Api.Utilities;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using Ism.Common.Core.Configuration.Models;
using Microsoft.AspNetCore.Http;
using Avalanche.Api.Managers.Medpresence;
using Avalanche.Api.Helpers;
using Avalanche.Shared.Infrastructure.Configuration;
using Avalanche.Shared.Infrastructure.Extensions;

namespace Avalanche.Api.Managers.Maintenance
{
    public class DeviceMaintenanceManager : MaintenanceManager
    {
        private readonly ConfigurationContext _configurationContext;

        private readonly IConfigurationManager _deviceConfigurationManager;
        private readonly IMedpresenceManager _medpresenceManager;

        public DeviceMaintenanceManager(IStorageService storageService,
            IDataManager dataManager,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            ILibraryService libraryService,
            IFilesService filesService,
            IPrintingService printingService,
            ISharedConfigurationManager sharedConfigurationManager,
            IConfigurationManager deviceConfigurationManager,
            IMedpresenceManager medpresenceManager) : base(storageService, dataManager, mapper, httpContextAccessor, libraryService, filesService, printingService, sharedConfigurationManager)
        {
            _deviceConfigurationManager = deviceConfigurationManager;

            var user = HttpContextUtilities.GetUser(httpContextAccessor.HttpContext);
            _configurationContext = mapper.Map<UserModel, ConfigurationContext>(user);
            _configurationContext.IdnId = Guid.NewGuid().ToString();
        }

        public async Task ProvisionMedpresence(DynamicSectionViewModel category)
        {
            var json = DynamicSettingsHelper.GetJsonValues(category);

            var request = new MedpresenceProvisioningViewModel
            {
                InputParameters = new ProvisioningInputParametersViewModel
                {
                    CustomerName = json.Get<MedPresenceProvisioningConfiguration>().InputParameters.CustomerName,
                    Environment = json.Get<MedPresenceProvisioningConfiguration>().InputParameters.Environment,
                    Name = json.Get<MedPresenceProvisioningConfiguration>().InputParameters.Name,
                    SerialNumber = json.Get<MedPresenceProvisioningConfiguration>().InputParameters.SerialNumber,
                    Department = json.Get<MedPresenceProvisioningConfiguration>().InputParameters.Department,
                    Speciality = json.Get<MedPresenceProvisioningConfiguration>().InputParameters.Specialty
                },
                EnvironmentSettings = new ProvisioiningEnvironmentSettingsViewModel
                {
                    ApiUrl = json.Get<MedPresenceProvisioningConfiguration>().EnvironmentSettings.ApiUrl,
                    IdentityUrl = json.Get<MedPresenceProvisioningConfiguration>().EnvironmentSettings.IdentityUrl,
                    ClientId = json.Get<MedPresenceProvisioningConfiguration>().EnvironmentSettings.ClientId
                }
            };

            await _medpresenceManager.ProvisionMedpresence(request).ConfigureAwait(false);
        }

        public override async Task SaveCategory(DynamicSectionViewModel category)
        {
            if (category.JsonKey.Equals("medpresenceprovisioningconfiguration", StringComparison.CurrentCultureIgnoreCase))
            {
                await ProvisionMedpresence(category).ConfigureAwait(false);
            }

            await base.SaveCategory(category);
        }

        protected override void CheckLinks(DynamicListViewModel category)
        {
            switch (category.SourceKey)
            {
                case "ProcedureTypes":
                    for (var i = 0; i < category.Links.Count; i++)
                    {
                        var link = category.Links[i];
                        switch (link.Key)
                        {
                            case "AutoLabels":
                                if (!_deviceConfigurationManager.GetLabelsConfigurationSettings().AutoLabelsEnabled)
                                {
                                    category.Links.Remove(link);
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        protected override async Task SetIsRequired(string key, DynamicPropertyViewModel item)
        {
            //It is a switch because this can grow on time
            switch (key)
            {
                default:
                    break;
            }
        }
    }
}
