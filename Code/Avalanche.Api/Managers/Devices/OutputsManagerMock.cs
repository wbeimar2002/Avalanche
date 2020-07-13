using AutoFixture;
using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Devices
{
    [ExcludeFromCodeCoverage]
    public class OutputsManagerMock : IOutputsManager
    {
        public Task<List<Output>> GetAllAvailable()
        {
            List<Output> outputs = new List<Output>();
            outputs.Add(new Output()
            {
                Id = Guid.NewGuid().ToString(),
                IsActive = true,
                Name = "MAIN TV 1",
                Thumbnail = "https://www.olympus-oste.eu/media/innovations/images_5/2n_research_and_development/entwicklung_produktinnovationen_intro.jpg"
            });

            outputs.Add(new Output()
            {
                Id = Guid.NewGuid().ToString(),
                IsActive = false,
                Name = "MAIN TV 2",
                Thumbnail = "https://www.olympus-oste.eu/media/innovations/images_5/2n_research_and_development/entwicklung_produktinnovationen_intro.jpg"
            });

            outputs.Add(new Output()
            {
                Id = Guid.NewGuid().ToString(),
                IsActive = true,
                Name = "AUX TV 1",
                Thumbnail = "https://www.olympus-oste.eu/media/innovations/images_5/2n_research_and_development/entwicklung_produktinnovationen_systemintegration_6.jpg"
            });

            outputs.Add(new Output()
            {
                Id = Guid.NewGuid().ToString(),
                IsActive = false,
                Name = "MAIN TV 2",
                Thumbnail = "https://www.olympus-oste.eu/media/innovations/images_5/2n_research_and_development/entwicklung_produktinnovationen_systemintegration_6.jpg"
            });


            return Task.FromResult(outputs);
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
                        Description = "Sample for content for kids",
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

        public Task<State> GetCurrentState(string id, StateTypes stateType)
        {
            Preconditions.ThrowIfNull<string>(nameof(id), id);

            State state;
            switch (stateType)
            {
                case StateTypes.Volume:
                    state = new State()
                    { 
                        StateType = stateType,
                        Value = "8"
                    };
                    break;
                case StateTypes.SilentMode:
                    state = new State()
                    {
                        StateType = stateType,
                        Value = "True"
                    };
                    break;
                case StateTypes.CurrentMainSignal:
                    state = new State()
                    {
                        StateType = stateType,
                        Value = "http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/VolkswagenGTIReview.mp4"
                    };
                    break;
                default:
                    state = new State()
                    {
                        StateType = stateType,
                        Value = null
                    };
                    break;
            }

            return Task.FromResult(state);
        }

        public Task<List<State>> GetCurrentStates(string id)
        {
            Preconditions.ThrowIfNull<string>(nameof(id), id);

            List<State> states = new List<State>();
            states.Add(new State()
            {
                StateType = StateTypes.Volume,
                Value = "8"
            });

            states.Add(new State()
            {
                StateType = StateTypes.SilentMode,
                Value = "True"
            });

            states.Add(new State()
            {
                StateType = StateTypes.CurrentMainSignal,
                Value = "http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/VolkswagenGTIReview.mp4"
            });

            return Task.FromResult(states);
        }
    }
}
