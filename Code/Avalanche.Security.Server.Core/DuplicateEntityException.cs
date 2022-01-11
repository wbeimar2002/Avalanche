using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Avalanche.Security.Server.Core
{
    [Serializable]
    public class DuplicateEntityException : Exception
    {
        public DuplicateEntityException()
        {
        }

        public DuplicateEntityException(Type entity, string entityId, string constraintName, string duplicateValue, Exception? innerException = null) : base($"Insert or Update for entity '{entity.Name}' with Id '{entityId}' failed because it would violate uniqueness constraint on '{constraintName}' with value: '{duplicateValue}'.  See Inner Exception for more details", innerException)
        {
            Entity = entity;
            EntityId = entityId;
            ConstraintName = constraintName;
            DuplicateValue = duplicateValue;
        }

        public DuplicateEntityException(string message) : base(message)
        {
        }

        public DuplicateEntityException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Without this constructor, deserialization will fail
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected DuplicateEntityException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public string? ConstraintName { get; }
        public string? DuplicateValue { get; }
        public Type? Entity { get; }

        public string? EntityId { get; }
    }
}
