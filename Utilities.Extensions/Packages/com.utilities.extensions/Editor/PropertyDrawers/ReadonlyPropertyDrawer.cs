// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEditor;
using UnityEngine;

namespace Utilities.Extensions.Editor
{
    /// <summary>
    /// Property drawer that renders fields marked with <see cref="ReadonlyLabelAttribute"/> as read-only.
    /// </summary>
    [CustomPropertyDrawer(typeof(ReadonlyLabelAttribute))]
    public class ReadonlyPropertyDrawer : PropertyDrawer
    {
        /// <summary>
        /// Draws the property in read-only mode, optionally as a selectable/copyable label.
        /// </summary>
        /// <param name="position">The drawing rectangle.</param>
        /// <param name="property">The serialized property to render.</param>
        /// <param name="label">The field label.</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (attribute is not ReadonlyLabelAttribute readonlyLabelAttribute) { return; }

            EditorGUI.BeginProperty(position, label, property);

            if (readonlyLabelAttribute.CanCopyLabel)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUI.LabelField(position, label);
                var rect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, position.width - EditorGUIUtility.labelWidth, position.height);
                EditorGUI.SelectableLabel(rect, property.stringValue);
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUI.PropertyField(position, property, label);
            }

            EditorGUI.EndProperty();
        }
    }
}
