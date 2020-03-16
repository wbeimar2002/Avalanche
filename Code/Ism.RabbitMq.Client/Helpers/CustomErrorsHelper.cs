using Ism.RabbitMq.Client.Enumerations;
using Ism.RabbitMq.Client.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ism.RabbitMq.Client.Helpers
{
    public static class CustomErrorsHelper
    {
        public static string GetError(HealthCheckStatus customError, string requestUrl, string errorDescription, string stackTrace)
        {
            Error error = new Error()
            {
                Code = (int)customError,
                ReasonPhrase = customError.ToString(),
                RequestUrl = requestUrl,
                Description = errorDescription,
                StackTrace = stackTrace
            };

            return JsonConvert.SerializeObject(error);
        }
    }
}
