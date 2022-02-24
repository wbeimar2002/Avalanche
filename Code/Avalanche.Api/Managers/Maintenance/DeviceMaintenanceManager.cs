using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
using Avalanche.Shared.Infrastructure.Configuration;
using Avalanche.Shared.Infrastructure.Extensions;
using Ism.Common.Core.Configuration.Models;
using Microsoft.AspNetCore.Http;
using Avalanche.Api.Managers.Medpresence;

namespace Avalanche.Api.Managers.Maintenance
{
    public class DeviceMaintenanceManager : MaintenanceManager
    {
        private readonly IStorageService _storageService;
        private readonly ConfigurationContext _configurationContext;

        private readonly IConfigurationManager _deviceConfigurationManager;

        public DeviceMaintenanceManager(IStorageService storageService,
            IDataManager dataManager,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            ILibraryService libraryService,
            IFilesService filesService,
            IPrintingService printingService,
            ISharedConfigurationManager sharedConfigurationManager,
            IConfigurationManager deviceConfigurationManager,
            IMedpresenceManager medpresenceManager) : base(storageService, dataManager, mapper, httpContextAccessor, libraryService, filesService, printingService, sharedConfigurationManager, medpresenceManager)
        {
            _deviceConfigurationManager = deviceConfigurationManager;
            _storageService = storageService;

            var user = HttpContextUtilities.GetUser(httpContextAccessor.HttpContext);
            _configurationContext = mapper.Map<UserModel, ConfigurationContext>(user);
            _configurationContext.IdnId = Guid.NewGuid().ToString();
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
