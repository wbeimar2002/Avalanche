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
                cfg.AddProfile(new PieMappingConfigurations());
                cfg.AddProfile(new VideoRotingMappingConfigurations());
            });

            _mapper = config.CreateMapper();
        }

        [Test]
        public void PieMappingConfigurations_IsValid()
        {
            AssertProfileIsValid<PieMappingConfigurations>();      
        }

        [Test]
        public void VideoRotingMappingConfigurations_IsValid()
        {
            AssertProfileIsValid<VideoRotingMappingConfigurations>();
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
