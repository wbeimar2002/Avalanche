using AutoMapper;
using Avalanche.Api.MappingConfigurations;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Api.Tests.MappingConfigurations
{
    [TestFixture]
    public class ProfileMappingTests
    {
        IMapper _mapper;

        [SetUp]
        public void SetUp()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new HealthMappingConfigurations());
                cfg.AddProfile(new RoutingMappingConfigurations());
                cfg.AddProfile(new MediaMappingConfigurations());
            });

            _mapper = config.CreateMapper();
        }

        [Test]
        public void HealthMappingConfigurations_IsValid()
        {
            AssertProfileIsValid<HealthMappingConfigurations>();      
        }

        [Test]
        public void VideoRoutingMappingConfigurations_IsValid()
        {
            AssertProfileIsValid<RoutingMappingConfigurations>();
        }

        [Test]
        public void MediaMappingConfigurations_IsValid()
        {
            AssertProfileIsValid<MediaMappingConfigurations>();
        }

        private void AssertProfileIsValid<TProfile>() where TProfile : Profile, new()
        {
            _mapper.ConfigurationProvider.AssertConfigurationIsValid<TProfile>();
        }

        [TearDown]
        public void TearDown()
        {

        }
    }
}
