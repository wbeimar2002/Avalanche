﻿using AutoMapper;
using Avalanche.Api.Services.Configuration;
using Avalanche.Api.Services.Maintenance;
using Avalanche.Api.Services.Media;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Helpers;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Devices
{
    [ExcludeFromCodeCoverage]
    public class MediaManager : IMediaManager
    {
        readonly IMediaService _mediaService;
        readonly IMapper _mapper;

        public MediaManager(IMediaService mediaService,
            IMapper mapper)
        {
            _mediaService = mediaService;
            _mapper = mapper;
        }

        public async Task SaveFileAsync(IFormFile file)
        {
            //TODO: Pending Service that upload file
            await Task.CompletedTask;
        }

        public async Task<IList<VideoDevice>> GetSourceStreams()
        {
            //TODO: Check this
            var result = await _mediaService.GetSourceStreamsAsync();
            IList<VideoDevice> listResult = _mapper.Map<IList<Ism.Streaming.V1.Protos.WebRtcSourceMessage>, IList<VideoDevice>>(result.Sources);
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
