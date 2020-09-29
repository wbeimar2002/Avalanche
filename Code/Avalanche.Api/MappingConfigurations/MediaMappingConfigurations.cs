using AutoMapper;
using Avalanche.Shared.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.MappingConfigurations
{
    public class MediaMappingConfigurations : Profile
    {
        /* Command to
         * InitSeesion 
         * command.Device
         * 
         * new Ism.Streaming.V1.Protos.HandleMessageRequest()
            {
                SessionId = command.AdditionalInfo,
                Offer = new Ism.Streaming.V1.Protos.WebRtcInfoMessage()
                { 
                    Message = command.Message,
                    Type = command.Type,
                }
            }

        new Ism.Streaming.V1.Protos.DeInitSessionRequest()
            {
                SessionId = command.AdditionalInfo,
            }


        new Ism.Streaming.V1.Protos.InitSessionRequest
            {
                AccessInfo = new Ism.Streaming.V1.Protos.AccessInfoMessage
                {
                    ApplicationName = applicationName,
                    Details = "Initialize webrtc stream",
                    Id = Guid.NewGuid().ToString(),
                    Ip = accessInfo.Ip,
                    MachineName = Environment.MachineName,
                    UserName = Environment.UserName
                },
                //Quality = RxStreamQualityEnum.RxStreamQualityHdVideo,
                //RouteToStreamingEncoder = true,
                StreamId = command.Device.Id,
                SessionId = command.AdditionalInfo,
                Offer = new Ism.Streaming.V1.Protos.WebRtcInfoMessage
                {
                    Aor = "AOR",
                    BypassMaxStreamRestrictions = true,
                    Type = command.Type,
                    Message = command.Message
                }
            });

            var response = new CommandResponse()
            {
                Device = command.Device,
                ResponseCode = (int)actionResponse.ResponseCode,
                Messages = new List<string>()
            };

            foreach (var item in actionResponse.Answer)
            {
                response.Messages.Add(item.Message);
            }

            return response;

        new SetTimeoutPageRequest()
            {
                PageNumber = Convert.ToInt32(command.Message)
            }

        new SetPgsTimeoutModeRequest()
            {
                Mode = (PgsTimeoutModeEnum)Convert.ToInt32(command.Message)
            }
         */
        public MediaMappingConfigurations()
        {
            CreateMap<Ism.Streaming.V1.Protos.WebRtcSourceMessage, Device>()
                .ForMember(dest =>
                    dest.InternalIndex,
                    opt => opt.MapFrom(src => src.PreviewIndex))
                .ForMember(dest =>
                    dest.Name,
                    opt => opt.MapFrom(src => src.StreamName))
                .ForMember(dest =>
                    dest.IsActive,
                    opt => opt.MapFrom(src => true))
                .ForMember(dest =>
                    dest.PositionInScreen,
                    opt => opt.MapFrom(src => 0))
                .ForMember(dest =>
                    dest.Id,
                    opt => opt.MapFrom(src => string.Empty))
                .ForMember(dest =>
                    dest.Type,
                    opt => opt.MapFrom(src => src.StreamType))
                .ReverseMap();
        }
    }
}
