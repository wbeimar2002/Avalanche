using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Avalanche.Shared.Domain.Models.Media
{
    public class WebRTCSessionModel : WebRTCMessaggeModel
    { 
        public AliasIndexModel Sink { get; set; }

    }
}
