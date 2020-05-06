using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Avalanche.Shared.Infrastructure.Helpers
{
    //TODO: Move this to Ism.Utility.Core
    public static class Preconditions
    {
        public static void AssertValidArgumentName(string argumentName)
        {
            if (string.IsNullOrWhiteSpace(argumentName))
            {
                throw new ArgumentNullException(nameof(argumentName));
            }
        }

        public static void ThrowIfNull<T>(string argumentName, T value)
        {
            AssertValidArgumentName(argumentName);
            if (value == null)
            {
                throw new ArgumentNullException(argumentName);
            }
        }

        public static void ThrowIfCountIsLessThan<T>(string argumentName, IEnumerable<T> enumerable, int expectedCount)
        {
            AssertValidArgumentName(argumentName);
            ThrowIfNull(nameof(enumerable), enumerable);

            var count = enumerable.Count();
            if (count < expectedCount)
            {
                throw new Exception($"Expected '{expectedCount}' item(s) in '{argumentName}', it has '{count}' item(s)");
            }
        }
    }
}
