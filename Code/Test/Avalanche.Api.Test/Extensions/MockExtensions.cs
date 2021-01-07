using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using System;
using System.Linq.Expressions;

namespace Avalanche.Api.Tests.Extensions
{
    static class MockExtensions
    {
        public static ISetup<ILogger<T>> Setup<T>(this Mock<ILogger<T>> mock, LogLevel level, string message)
        {
            return
                mock.Setup(x => x.Log(
                    level,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => string.Equals(message, o.ToString(), StringComparison.InvariantCultureIgnoreCase)),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()));
        }

        public static void Verify<T>(this Mock<ILogger<T>> mock, LogLevel level, string message, Times times)
        {
            mock.Verify(
                x => x.Log(
                    level,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => string.Equals(message, o.ToString(), StringComparison.InvariantCultureIgnoreCase)),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                    times);
        }
    }
}
