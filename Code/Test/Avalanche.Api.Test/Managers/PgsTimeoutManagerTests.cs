using AutoMapper;
using Avalanche.Api.Managers.PgsTimeout;
using Avalanche.Api.Managers.Procedures;
using Avalanche.Api.MappingConfigurations;
using Avalanche.Api.Services.Maintenance;
using Avalanche.Api.Services.Media;
using Avalanche.Shared.Domain.Models;
using Ism.Common.Core.Configuration.Models;
using Ism.SystemState.Client;
using Ism.SystemState.Models.Procedure;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;


namespace Avalanche.Api.Tests.Managers
{
    [TestFixture]
    public class PgsTimeoutManagerTests
    {

        IPgsTimeoutManager _pgsTimeoutManager;

        [SetUp]
        public void Setup()
        {
            var storageMock = new Mock<IStorageService>();
            var routingMock = new Mock<IRoutingService>();
            var stateClientMock = new Mock<IStateClient>();
            var pgsTimeoutMock = new Mock<IPgsTimeoutService>();

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new RoutingMappingConfiguration());
                cfg.AddProfile(new MediaMappingConfiguration());
            });
            var mapper = mapperConfig.CreateMapper();


            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Development.json", false, true).Build();

            var pgsTimeoutConfig = config.GetSection(nameof(PgsTimeoutConfig)).Get<PgsTimeoutConfig>(x => x.BindNonPublicProperties = true);

            storageMock.Setup(x => x.GetJsonObject<PgsTimeoutConfig>(nameof(PgsTimeoutConfig), 1, It.IsAny<ConfigurationContext>())).ReturnsAsync(pgsTimeoutConfig);

            _pgsTimeoutManager =  new PgsTimeoutManager(storageMock.Object, routingMock.Object, stateClientMock.Object, pgsTimeoutMock.Object, mapper);
        }
    }
}
