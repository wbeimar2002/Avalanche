using Avalanche.Shared.Infrastructure.Models;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Text;
using Ism.Common.Core.Grpc.Extensions;
using Google.Protobuf.Collections;
using static Google.Rpc.BadRequest.Types;
using Newtonsoft.Json;
using Ism.Common.Core.Protos.V1;

namespace Avalanche.Shared.Infrastructure.Extensions
{
    public static class ExceptionExtensions
    {
        public static Error Get(this Exception exception, bool isDevelopment)
        {
            if ((exception is RpcException rpcException))
            {
                var customError = rpcException.GetDetail<CustomError>();

                if (customError == null)
                {
                    return new Error()
                    {
                        Code = -1,
                        Description = rpcException.Message,
                        StackTrace = isDevelopment ? rpcException.StackTrace : string.Empty
                    };
                }
                else
                {
                    var fieldViolations = new Dictionary<string, string>();
                    if (customError.FieldViolations != null)
                    {
                        foreach (var item in customError.FieldViolations)
                        {
                            fieldViolations.Add(item.Key, item.Value);
                        }
                    }

                    return new Error()
                    {
                        Code = customError.Code,
                        Description = customError.Description,
                        StackTrace = customError.StackTrace,
                        FieldViolations = fieldViolations
                    };
                }
            }
            else
            {
                return new Error()
                {
                    Code = -1,
                    Description = exception.Message,
                    StackTrace = isDevelopment ? exception.StackTrace : string.Empty
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
