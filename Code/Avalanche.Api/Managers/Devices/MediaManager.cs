using AutoMapper;
using Avalanche.Api.Services.Configuration;
using Avalanche.Api.Services.Media;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Helpers;
using Avalanche.Shared.Infrastructure.Models;
using Ism.IsmLogCommon.Core;
using Ism.Streaming.Common.Core;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Devices
{
    [ExcludeFromCodeCoverage]
    public class MediaManager : IMediaManager
    {
        readonly ISettingsService _settingsService;
        readonly IMediaService _mediaService;
        readonly IMapper _mapper;

        public MediaManager(ISettingsService settingsService, 
            IMediaService mediaService,
            IMapper mapper)
        {
            _settingsService = settingsService;
            _mediaService = mediaService;
            _mapper = mapper;
        }

        public async Task SaveFileAsync(IFormFile file)
        {
            //TODO: Pending Service that upload file
            await Task.CompletedTask;
        }

        public async Task<IList<Device>> GetSourceStreams()
        {
            var result = await _mediaService.GetSourceStreams();
            IList<Device> listResult = _mapper.Map<IList<WebRtcSourceMessage>, IList<Device>>(result);
            return listResult;
        }

        public Task<Content> GetContent(string contentType)
        {
            Preconditions.ThrowIfNull<string>(nameof(contentType), contentType);

            Content content;
            switch (contentType)
            {
                case "P":
                    content = new Content()
                    {
                        Copyright = "All rights reserved",
                        Description = "Sample of content for kids",
                        Title = "Pediatric content",
                        Url = "http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ElephantsDream.mp4"
                    };
                    break;
                case "G":
                    content = new Content()
                    {
                        Copyright = "Undefined",
                        Description = "Empty content",
                        Title = "No signal",
                        Url = "http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/TearsOfSteel.mp4"
                    };
                    break;
                default:
                    content = new Content()
                    {
                        Copyright = "Undefined",
                        Description = "Empty content",
                        Title = "No signal",
                        Url = "http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/WeAreGoingOnBullrun.mp4"
                    };
                    break;
            }

            return Task.FromResult(content);
        }
    }
}
