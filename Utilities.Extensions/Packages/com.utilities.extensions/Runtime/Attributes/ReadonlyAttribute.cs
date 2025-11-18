// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using UnityEngine;

namespace Utilities.Extensions
{
    /// <summary>
    /// This attribute can be added to a field to make it read-only in the Unity Inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ReadonlyLabelAttribute : PropertyAttribute
    {
        public readonly bool CanCopyLabel;

        /// <summary>
        /// Makes the field read-only in the Unity Inspector.
        /// </summary>
        /// <param name="canCopyLabel">Optional, enables the label to be copied to clipboard.</param>
        public ReadonlyLabelAttribute(bool canCopyLabel = false)
        {
            CanCopyLabel = canCopyLabel;
        }
    }
}
