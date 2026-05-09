// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEditor;
using UnityEngine;

namespace Utilities.Extensions.Editor
{
    /// <summary>
    /// Extension methods for inspecting and mutating <see cref="SerializedProperty"/> values.
    /// </summary>
    public static class SerializedPropertyExtensions
    {
        /// <summary>
        /// Checks if the underlying <see cref="Object"/> reference is null or missing.
        /// </summary>
        /// <param name="property">The serialized property to inspect.</param>
        /// <returns>True, if <see cref="Object"/> reference is null or missing.</returns>
        public static bool IsMissingObjectReference(this SerializedProperty property)
        {
            if (property == null) { return true; }
            return property.propertyType switch
            {
                SerializedPropertyType.ObjectReference => ReferenceEquals(property.objectReferenceValue, null),
                SerializedPropertyType.ExposedReference => ReferenceEquals(property.exposedReferenceValue, null),
                _ => false
            };
        }

        /// <summary>
        /// Determines whether the property currently contains its default value for supported property types.
        /// </summary>
        /// <param name="property">The serialized property to inspect.</param>
        /// <returns><see langword="true"/> if the property has its default value; otherwise <see langword="false"/>.</returns>
        public static bool IsDefaultValue(this SerializedProperty property)
        {
            if (property == null) { return false; }
            return property.propertyType switch
            {
                SerializedPropertyType.Integer => property.intValue == 0,
                SerializedPropertyType.Float => Mathf.Approximately(property.floatValue, 0f),
                SerializedPropertyType.Boolean => property.boolValue == false,
                SerializedPropertyType.String => string.IsNullOrWhiteSpace(property.stringValue),
                SerializedPropertyType.Vector2 => property.vector2Value == Vector2.zero,
                SerializedPropertyType.Vector3 => property.vector3Value == Vector3.zero,
                SerializedPropertyType.Vector4 => property.vector4Value == Vector4.zero,
                SerializedPropertyType.Quaternion => property.quaternionValue == Quaternion.identity,
                SerializedPropertyType.ObjectReference => ReferenceEquals(property.objectReferenceValue, null),
                SerializedPropertyType.ExposedReference => ReferenceEquals(property.exposedReferenceValue, null),
                _ => false
            };
        }

        /// <summary>
        /// Sets the property value to its default for supported property types.
        /// </summary>
        /// <param name="property">The serialized property to mutate.</param>
        public static void SetDefaultValue(this SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    property.intValue = default;
                    break;
                case SerializedPropertyType.Float:
                    property.floatValue = default;
                    break;
                case SerializedPropertyType.Boolean:
                    property.boolValue = default;
                    break;
                case SerializedPropertyType.String:
                    property.stringValue = default;
                    break;
                case SerializedPropertyType.Vector2:
                    property.vector2Value = default;
                    break;
                case SerializedPropertyType.Vector3:
                    property.vector3Value = default;
                    break;
                case SerializedPropertyType.Vector4:
                    property.vector4Value = default;
                    break;
                case SerializedPropertyType.Quaternion:
                    property.quaternionValue = Quaternion.identity;
                    break;
                case SerializedPropertyType.ObjectReference:
                    property.objectReferenceValue = null;
                    break;
                case SerializedPropertyType.ExposedReference:
                    property.exposedReferenceValue = null;
                    break;
            }
        }

        /// <summary>
        /// Builds a stable identifier for the property using an editor-safe object key and the property path.
        /// </summary>
        /// <param name="property">The serialized property to identify.</param>
        /// <returns>A string identifier that is stable for a given target object and property path.</returns>
        public static string GetUniqueIdentifier(this SerializedProperty property)
        {
            var targetObject = property?.serializedObject?.targetObject;
            var objectId = "null";

            if (targetObject != null)
            {
                // Unity 6.4+ deprecates Object.GetInstanceID() (warn-as-error in CI). Prefer GetEntityId when available.
                // For Unity 6.0–6.3 (and any 6.x where the 6.4 define is unavailable), use GlobalObjectId — never InstanceID here.
#if UNITY_6000_4_OR_NEWER
                objectId = targetObject.GetEntityId().ToString();
#elif UNITY_6000_0_OR_NEWER
                objectId = GlobalObjectId.GetGlobalObjectIdSlow(targetObject).ToString();
#else
                objectId = targetObject.GetInstanceID().ToString();
#endif
            }

            return $"{objectId}/{property?.propertyPath}";
        }
    }
}
