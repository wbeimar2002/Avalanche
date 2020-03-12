﻿using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Shared.Infrastructure.Helpers
{
    public class ApplicationNameEnricher : ILogEventEnricher
    {
        readonly string _appName = "undefined";
        public ApplicationNameEnricher(string appName)
        {
            _appName = appName;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                    "ApplicationName", _appName));
        }
    }
}