using Avalanche.Shared.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Shared.Infrastructure.Extensions
{
    public static class ExceptionExtensions
    {
        public static Error Get(this Exception exception, bool isDevelopment)
        {
            return new Error()
            {
                Code = -1,
                Description = exception.Message,
                StackTrace = isDevelopment ? exception.StackTrace : string.Empty
            };
        }
    }
}
