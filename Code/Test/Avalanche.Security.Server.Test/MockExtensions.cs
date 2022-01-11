using System;
using Microsoft.Extensions.Logging;
using Moq;

namespace Avalanche.Security.Server.Test
{
    public static class MockExtensions
    {
        public static void AssertLoggerCalled<T>(this Mock<ILogger<T>> mock, LogLevel level, Times times) =>
            mock.Verify(
                x => x.Log(
                    level,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
                ),
                times
            );
    }
}
