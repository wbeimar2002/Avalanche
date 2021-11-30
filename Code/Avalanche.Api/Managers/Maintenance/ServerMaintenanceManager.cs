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

namespace Avalanche.Api.Managers.Maintenance
{
    public class ServerMaintenanceManager : MaintenanceManager
    {
        private readonly IStorageService _storageService;
        private readonly ConfigurationContext _configurationContext;

        private readonly IConfigurationManager _serverConfigurationManager;

        public ServerMaintenanceManager(IStorageService storageService,
            IDataManager dataManager, IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            ILibraryService libraryService,
            IFilesService filesService,
            IPrintingService printingService,
            ISharedConfigurationManager sharedConfigurationManager,
            IConfigurationManager serverConfigurationManager) : base(storageService, dataManager, mapper, httpContextAccessor, libraryService, filesService, printingService, sharedConfigurationManager)
        {
            _serverConfigurationManager = serverConfigurationManager;
            _storageService = storageService;

            var user = HttpContextUtilities.GetUser(httpContextAccessor.HttpContext);
            _configurationContext = mapper.Map<UserModel, ConfigurationContext>(user);
            _configurationContext.IdnId = Guid.NewGuid().ToString();
        }

        protected override async Task SaveEmbeddedList(string settingsKey, string jsonKey, string json, string schema = null)
        {
            if (string.IsNullOrEmpty(schema) || await _storageService.ValidateSchema(schema, json, 1, _configurationContext).ConfigureAwait(false))
            {
                await _storageService.UpdateJsonProperty(settingsKey, jsonKey, json, 1, _configurationContext, true).ConfigureAwait(false);

                switch (settingsKey)
                {
                    case "ProceduresSearchConfiguration":
                        _serverConfigurationManager.UpdateProceduresSearchConfigurationColumns(json.Get<List<ColumnProceduresSearchConfiguration>>());
                        break;
                }
            }
            else
            {
                throw new ValidationException("Json Schema Invalid for " + jsonKey);
            }
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