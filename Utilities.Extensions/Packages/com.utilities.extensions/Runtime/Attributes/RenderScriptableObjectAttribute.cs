// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using UnityEngine;

namespace Utilities.Extensions
{
    /// <summary>
    /// This attribute can be added to a <see cref="ScriptableObject"/> field to draw the nested properties inline in the Unity Inspector.
    /// It contains a picker field to select a <see cref="ScriptableObject"/> asset, then displays its properties directly below the picker.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class RenderScriptableObjectAttribute : PropertyAttribute
    {
    }
}
