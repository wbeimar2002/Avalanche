﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Shared.Domain.Models.Media
{
    public class WebRTCMessaggeModel
    {
        public string SessionId { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
    }
}