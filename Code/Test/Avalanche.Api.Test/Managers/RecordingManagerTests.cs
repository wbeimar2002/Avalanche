using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Avalanche.Api.Managers.Media;
using Avalanche.Api.Mapping;
using Avalanche.Api.Services.Media;
using Ism.Recorder.Core.V1.Protos;
using Ism.SystemState.Client;
using Ism.SystemState.Models.Procedure;
using Ism.SystemState.Models.Recorder;
using Moq;
using NUnit.Framework;

namespace Avalanche.Api.Test.Managers
{
    [TestFixture]
    public class RecordingManagerTests
    {
        private IMapper _mapper;
        private Mock<IRecorderService> _recorderService;
        private Mock<IStateClient> _stateClient;

        private IRecordingManager _recordingManager;

        [SetUp]
        public void Setup()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new PatientMappingConfiguration());
                cfg.AddProfile(new DataMappingConfiguration());
                cfg.AddProfile(new MediaMappingConfiguration());
                cfg.AddProfile(new RecorderMappingConfiguration());
                cfg.AddProfile(new ProceduresMappingConfiguration());
            });

            _mapper = config.CreateMapper();
            _recorderService = new Mock<IRecorderService>();
            _stateClient = new Mock<IStateClient>();

            // fake some record channels
            _recorderService.Setup(x => x.GetRecordingChannels()).Returns(Task.FromResult(new List<RecordChannelMessage>
            {
                new RecordChannelMessage
                {
                    ChannelName = "Rec1",
                    Is4K = false,
                    VideoSink = new Ism.Recorder.Core.V1.Protos.AliasIndexMessage
                    {
                        Alias = "BX4Comp",
                        Index= "a_enc"
                    }
                },
                new RecordChannelMessage
                {
                    ChannelName = "Rec2",
                    Is4K = false,
                    VideoSink = new Ism.Recorder.Core.V1.Protos.AliasIndexMessage
                    {
                        Alias = "BX4Comp",
                        Index= "b_enc"
                    }
                }
            }.AsEnumerable()));

            _recordingManager = new RecordingManager(_recorderService.Object, _stateClient.Object, _mapper);
        }

        [Test]
        public async Task RecordingManager_StartRecord_EventPublishes()
        {
            await _recordingManager.StartRecording().ConfigureAwait(false);
            _stateClient.Verify(x => x.PublishEvent(It.IsAny<StartRecordingEvent>()), Times.Once());
        }

        [Test]
        public async Task RecordingManager_StopRecord_EventPublishes()
        {
            await _recordingManager.StopRecording().ConfigureAwait(false);
            _stateClient.Verify(x => x.PublishEvent(It.IsAny<StopRecordingEvent>()), Times.Once());
        }

        [Test]
        public async Task RecordingManager_CaptureImage_EventPublishes()
        {
            await _recordingManager.CaptureImage().ConfigureAwait(false);
            _stateClient.Verify(x => x.PublishEvent(It.IsAny<CaptureImageEvent>()), Times.Once());
        }

        [Test]
        public async Task RecordingManager_GetRecordChannels_CallsService()
        {
            _ = await _recordingManager.GetRecordingChannels().ConfigureAwait(false);
            _recorderService.Verify(x => x.GetRecordingChannels(), Times.Once());
        }

        [Test]
        public async Task RecordingManager_CaptureImageFromVideo_EventPublishes()
        {
            var videoId = Guid.NewGuid();
            var timestamp = TimeSpan.FromSeconds(5);

            // arrange
            // fake enough of the procedure to capture an image from video
            var procedure = new ActiveProcedureState
            {
                LibraryId = "libId",
                RepositoryId = "repoId",
                Videos = new List<ProcedureVideo>
                {
                    new ProcedureVideo
                    {
                        VideoId = videoId,
                        VideoDuration = timestamp * 2
                    }
                }
            };
            _stateClient.Setup(x => x.GetData<ActiveProcedureState>()).ReturnsAsync(procedure);

            // act
            await _recordingManager.CaptureImageFromVideo(videoId, timestamp).ConfigureAwait(false);

            // assert
            _stateClient.Verify(x => x.PublishEvent(It.Is<CaptureImageFromVideoEvent>(e => e.VideoId == videoId && e.VideoPosition == timestamp)));
        }

        [Test]
        public void RecordingManager_CaptureImageFromVideoNoProcedure_Throws()
        {
            // capture image from video with no patient should throw
            var videoId = Guid.NewGuid();
            var timestamp = TimeSpan.FromSeconds(5);
            Assert.ThrowsAsync<InvalidOperationException>(() => _recordingManager.CaptureImageFromVideo(videoId, timestamp));
        }

        [Test]
        public async Task RecordingManager_GetTimelineByImageId_ReturnsTimeStamp()
        {
            var imageId = Guid.NewGuid();
            var videoId = Guid.NewGuid();
            var timestamp = TimeSpan.FromSeconds(5);

            // arrange
            // fake enough of the procedure to have an image captured while video was being recorded
            var procedure = new ActiveProcedureState
            {
                LibraryId = "libId",
                RepositoryId = "repoId",
                Videos = new List<ProcedureVideo>
                {
                    new ProcedureVideo
                    {
                        VideoId = videoId,
                        VideoDuration = timestamp * 2
                    }
                },
                RecordingEvents = new List<VideoRecordingEvent>
                {
                    new VideoRecordingEvent
                    {
                        EventType = EventType.imageCapture,
                        ImageId = imageId,
                        VideoId = videoId,
                        VideoOffset = timestamp
                    }
                },
                Images = new List<ProcedureImage>
                {
                    new ProcedureImage
                    {
                        ImageId = imageId,
                        VideoReference = new VideoReference(timestamp, videoId)
                    }
                }
            };
            _stateClient.Setup(x => x.GetData<ActiveProcedureState>()).ReturnsAsync(procedure);

            // act
            var timelineViewModel = await _recordingManager.GetRecordingTimelineByImageId(imageId).ConfigureAwait(false);

            // assert
            Assert.NotNull(timelineViewModel);
            Assert.True(timelineViewModel!.VideoId == videoId);
            Assert.True(timelineViewModel.VideoOffset == timestamp);
        }

        [Test]
        public async Task RecordingManager_GetTimelineByImageIdNoVideo_ReturnsNull()
        {
            var imageId = Guid.NewGuid();

            // arrange
            // we have an image, but no video or procedure events
            var procedure = new ActiveProcedureState
            {
                LibraryId = "libId",
                RepositoryId = "repoId",
                Videos = new List<ProcedureVideo>(),
                RecordingEvents = new List<VideoRecordingEvent>(),
                Images = new List<ProcedureImage>
                {
                    new ProcedureImage
                    {
                         ImageId = imageId,
                         VideoReference = null
                    }
                }
            };
            _stateClient.Setup(x => x.GetData<ActiveProcedureState>()).ReturnsAsync(procedure);

            // act
            var timelineViewModel = await _recordingManager.GetRecordingTimelineByImageId(imageId).ConfigureAwait(false);

            // assert
            Assert.Null(timelineViewModel);
        }

        [Test]
        public async Task RecordingManager_GetTimelineByImageIdNoProcedure_Throws()
        {
            // can only do this from an active procedure
            var imageId = Guid.NewGuid();
            Assert.ThrowsAsync<InvalidOperationException>(() => _recordingManager.GetRecordingTimelineByImageId(imageId));
        }
    }
}
