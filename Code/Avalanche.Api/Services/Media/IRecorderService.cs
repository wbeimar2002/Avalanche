using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Media
{
    public interface IRecorderService
    {
        Task StartRecording(Ism.Recorder.Core.V1.Protos.RecordMessage recordMessage);
        Task StopRecording();
    }
}
