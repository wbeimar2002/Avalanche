using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Shared.Domain.Models.Media
{
    public class WebRTCSessionModel : WebRTCMessaggeModel
    { 
        public AccessInfoModel AccessInformation { get; set; }
        public SinkModel Sink { get; set; }

    }
}
