using System;
using UnityEngine;

namespace Utilities.Extensions
{
    /// <summary>
    /// This attribute can be added to a string field to enable scene selection in the Unity Inspector.
    /// It allows you to select a scene from the project, and the selected scene's name
    /// will be stored as a string in the field. Using the <see cref="Utilities.Extensions.Editor.ScenePropertyDrawer"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class SceneReferenceAttribute : PropertyAttribute
    {
    }
}
