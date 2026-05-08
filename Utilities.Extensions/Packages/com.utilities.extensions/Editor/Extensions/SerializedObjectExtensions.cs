// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEditor;

namespace Utilities.Extensions.Editor
{
    /// <summary>
    /// Null-safe extension methods for <see cref="SerializedObject"/>.
    /// </summary>
    public static class SerializedObjectExtensions
    {
        /// <summary>
        /// Checks whether a serialized object or its target object is null.
        /// </summary>
        /// <param name="object">The serialized object to inspect.</param>
        /// <returns><see langword="true"/> if null; otherwise <see langword="false"/>.</returns>
        public static bool IsNull(this SerializedObject @object)
        {
            try
            {
                return @object == null || @object.targetObject == null;
            }
            catch
            {
                return true;
            }
        }

        /// <summary>
        /// Checks whether a serialized object and its target object are not null.
        /// </summary>
        /// <param name="object">The serialized object to inspect.</param>
        /// <returns><see langword="true"/> if not null; otherwise <see langword="false"/>.</returns>
        public static bool IsNotNull(this SerializedObject @object)
            => !IsNull(@object);
    }
}
