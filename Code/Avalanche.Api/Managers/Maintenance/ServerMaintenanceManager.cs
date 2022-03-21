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

namespace Avalanche.Api.Managers.Maintenance
{
    public class ServerMaintenanceManager : MaintenanceManager
    {
        private readonly ConfigurationContext _configurationContext;

        public ServerMaintenanceManager(IStorageService storageService,
            IDataManager dataManager, IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            ILibraryService libraryService,
            IFilesService filesService,
            IPrintingService printingService,
            ISharedConfigurationManager sharedConfigurationManager) : base(storageService, dataManager, mapper, httpContextAccessor, libraryService, filesService, printingService, sharedConfigurationManager)
        {
            var user = HttpContextUtilities.GetUser(httpContextAccessor.HttpContext);
            _configurationContext = mapper.Map<UserModel, ConfigurationContext>(user);
            _configurationContext.IdnId = Guid.NewGuid().ToString();
        }

        protected override void CheckLinks(DynamicListViewModel category)
        {
            switch (category.SourceKey)
            {
                default:
                    break;
            }
        }

        protected override async Task SetIsRequired(string key, DynamicPropertyViewModel item)
        {
            switch (key)
            {
                default:
                    break;
            }
        }
    }
}
