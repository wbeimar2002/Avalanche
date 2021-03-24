using Avalanche.Shared.Infrastructure.Models;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Text;
using Ism.Common.Core.Grpc.Extensions;
using Google.Protobuf.Collections;
using static Google.Rpc.BadRequest.Types;

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

        public static Error Get(this RpcException exception, string path, bool isDevelopment)
        {
            var info = exception.GetDetail<Google.Rpc.ErrorInfo>();
            var badRequest = exception.GetDetail<Google.Rpc.BadRequest>();

            if (info == null)
            {
                return new Error()
                {
                    Code = -1,
                    RequestUrl = path,
                    Description = exception.Message,
                    StackTrace = isDevelopment ? exception.StackTrace : string.Empty
                };
            }
            else
            {
                return new Error()
                {
                    Code = (int)exception.StatusCode,
                    RequestUrl = path,
                    ReasonPhrase = info.Reason,
                    StackTrace = isDevelopment ? exception.StackTrace : string.Empty,
                    FieldViolations = badRequest == null ? null : badRequest.FieldViolations.GetDictionary()
                };
            }
        }

        public static Dictionary<string, string> GetDictionary(this RepeatedField<FieldViolation> fieldViolations)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            foreach (var item in fieldViolations)
            {
                result.Add(item.Field, item.Description);
            }

            return result;
        }
    }
}
