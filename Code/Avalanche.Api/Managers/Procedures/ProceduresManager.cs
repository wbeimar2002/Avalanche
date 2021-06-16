using AutoFixture;
using AutoMapper;

using Avalanche.Api.Services.Health;
using Avalanche.Api.Services.Media;
using Avalanche.Api.Utilities;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Enumerations;
using Ism.Library.V1.Protos;
using Ism.SystemState.Client;
using Ism.SystemState.Models.Procedure;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Procedures
{
    public class ProceduresManager : IProceduresManager
    {
        private readonly IStateClient _stateClient;
        private readonly ILibraryService _libraryService;
        private readonly IMapper _mapper;
        private readonly IAccessInfoFactory _accessInfoFactory;
        private readonly IRecorderService _recorderService;

        public ProceduresManager(IStateClient stateClient, ILibraryService libraryService, IAccessInfoFactory accessInfoFactory, IMapper mapper, IRecorderService recorderService)
        {
            _stateClient = stateClient;
            _libraryService = libraryService;
            _accessInfoFactory = accessInfoFactory;
            _mapper = mapper;
            _libraryService = libraryService;
            _accessInfoFactory = accessInfoFactory;
            _recorderService = recorderService;
        }

        /// <summary>
        /// Load the active procedure (if exists)
        /// </summary>
        public async Task<ActiveProcedureViewModel> GetActiveProcedure()
        {
            var activeProcedure = await _stateClient.GetData<ActiveProcedureState>();
            var result = _mapper.Map<ActiveProcedureViewModel>(activeProcedure);

            if (result != null)
                result.RecorderState = (await _recorderService.GetRecorderState()).State;

            return result;
        }

        /// <summary>
        /// Set ActiveProcedure's "RequiresUserConfirmation" flag to false.
        /// </summary>
        public async Task ConfirmActiveProcedure()
        {
            var activeProcedure = await _stateClient.GetData<ActiveProcedureState>();

            activeProcedure.RequiresUserConfirmation = false;
            await _stateClient.PersistData(activeProcedure);
        }

        public async Task DeleteActiveProcedureMedia(ProcedureContentType procedureContentType, Guid contentId)
        {
            var accessInfo = _accessInfoFactory.GenerateAccessInfo();
            var activeProcedure = await _stateClient.GetData<ActiveProcedureState>();

            if (procedureContentType == ProcedureContentType.Video)
            {
                var video = activeProcedure.Videos.Single(v => v.VideoId == contentId);
                if (!video.VideoStopTimeUtc.HasValue)
                {
                    throw new InvalidOperationException("Can not delete video that is currently recording");
                }
            }

            var request = new DeleteActiveProcedureMediaRequest()
            {
                ContentId = contentId.ToString(),
                ContentType = _mapper.Map<ContentType>(procedureContentType),
                ProcedureId = _mapper.Map<ProcedureIdMessage>(activeProcedure),
                AccessInfo = _mapper.Map<AccessInfoMessage>(accessInfo)
            };

            await _libraryService.DeleteActiveProcedureMedia(request);
        }

        public async Task DiscardActiveProcedure()
        {
            var accessInfo = _accessInfoFactory.GenerateAccessInfo();

            var activeProcedure = await _stateClient.GetData<ActiveProcedureState>();
            var request = _mapper.Map<ActiveProcedureState, DiscardActiveProcedureRequest>(activeProcedure);

            request.AccessInfo = _mapper.Map<AccessInfoMessage>(accessInfo);

            if (await _recorderService.IsRecording())
                await _recorderService.StopRecording();

            await _libraryService.DiscardActiveProcedure(request);
        }

        public async Task FinishActiveProcedure()
        {
            var activeProcedure = await _stateClient.GetData<ActiveProcedureState>();
            var request = _mapper.Map<ActiveProcedureState, CommitActiveProcedureRequest>(activeProcedure);

            var accessInfo = _accessInfoFactory.GenerateAccessInfo();
            request.AccessInfo = _mapper.Map<AccessInfoMessage>(accessInfo);

            if (await _recorderService.IsRecording())
                await _recorderService.StopRecording();

            await _libraryService.CommitActiveProcedure(request);
        }

        public async Task<ProcedureAllocationViewModel> AllocateNewProcedure()
        {
            var accessInfo = _accessInfoFactory.GenerateAccessInfo();
            var response = await _libraryService.AllocateNewProcedure(new AllocateNewProcedureRequest
            {
                AccessInfo = _mapper.Map<AccessInfoMessage>(accessInfo),
                Clinical = true
            });

            return _mapper.Map<ProcedureAllocationViewModel>(response);
        }

        public async Task<ProceduresContainerViewModel> Search(ProcedureSearchFilterViewModel filter)
        {
            Fixture fixture = new Fixture();
            var result = fixture.CreateMany<ProcedureViewModel>(filter.Limit);

            foreach (var item in result)
            {
                foreach (var video in item.Videos)
                {
                    video.RelativePath = @"https://static.videezy.com/system/resources/previews/000/032/949/original/pink_pig3.mp4";
                    video.ThumbnailRelativePath = @"https://www.olympus.es/medical/rmt/media/Content/Content-MSD/Images/MSP-Pages/MSP-GS/MSP_GS_Hepato-Pancreato-Biliary-Surgery_Procedure_20172808_ProductHero_320.jpg";
                }

                foreach (var image in item.Images)
                {
                    image.RelativePath = @"https://www.olympus.es/medical/rmt/media/Content/Content-MSD/Images/MSP-Pages/MSP-GS/MSP_GS_Hepato-Pancreato-Biliary-Surgery_Procedure_20172808_ProductHero_320.jpg";
                }
            }

            return new ProceduresContainerViewModel()
            {
                TotalCount = 100,
                Procedures = result.ToList()
            };

            //var response = await _libraryService.GetFinishedProcedures(new GetFinishedProceduresRequest()
            //{
            //    Page = filter.Page,
            //    PageSize = filter.Limit,
            //    IsDescending = filter.IsDescending,
            //    ProcedureIndexSortingColumn = (ProcedureIndexSortingColumns)filter.ProcedureIndexSortingColumn
            //});

            //return new ProceduresContainerViewModel()
            //{
            //    TotalCount = response.TotalCount,
            //    Procedures = _mapper.Map<IList<ProcedureViewModel>>(response.Procedures) //TODO: Create Mapping
            //};
        }

        public async Task<ProcedureViewModel> GetProcedureDetails(string libraryId)
        {
            Fixture fixture = new Fixture();
            var result = fixture.Create<ProcedureViewModel>();
            result.Videos = fixture.CreateMany<ProcedureVideoViewModel>(15).ToList();

            foreach (var video in result.Videos)
            {
                video.RelativePath = @"https://static.videezy.com/system/resources/previews/000/032/949/original/pink_pig3.mp4";
                video.ThumbnailRelativePath = @"https://www.olympus.es/medical/rmt/media/Content/Content-MSD/Images/MSP-Pages/MSP-GS/MSP_GS_Hepato-Pancreato-Biliary-Surgery_Procedure_20172808_ProductHero_320.jpg";
            }

            foreach (var image in result.Images)
            {
                image.RelativePath = @"https://www.olympus.es/medical/rmt/media/Content/Content-MSD/Images/MSP-Pages/MSP-GS/MSP_GS_Hepato-Pancreato-Biliary-Surgery_Procedure_20172808_ProductHero_320.jpg";
            }

            return result;

            //var response = await _libraryService.GetFinishedProcedure(new GetFinishedProcedureRequest()
            //{
            //    LibraryId = libraryId
            //});

            //return _mapper.Map<ProcedureViewModel>(response.Procedure); //TODO: Create Mapping
        }
    }
}
