// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Utilities.Extensions.Editor
{
    public static class EditorGUILayoutExtensions
    {
        /// <summary>
        /// Draws a divider.
        /// </summary>
        /// <param name="height">Height in pixels</param>
        /// <param name="color">Optional, color.</param>
        public static void Divider(int height = 1, Color? color = null)
        {
            var rect = EditorGUILayout.GetControlRect(false, height);
            rect.height = height;
            color ??= EditorGUIUtility.isProSkin ? new Color(0.1f, 0.1f, 0.1f) : new Color(0.5f, 0.5f, 0.5f, 1);
            EditorGUI.DrawRect(rect, color.Value);
        }

        private static readonly Dictionary<string, Vector2> scrollCache = new Dictionary<string, Vector2>();

        private static MethodInfo scrollableTextAreaInternal;

        private static MethodInfo ScrollableTextAreaInternal
        {
            get
            {
                if (scrollableTextAreaInternal != null) { return scrollableTextAreaInternal; }
                var types = new[]
                {
                    typeof(Rect),
                    typeof(string),
                    typeof(Vector2).MakeByRefType(),
                    typeof(GUIStyle)
                };
                return scrollableTextAreaInternal = typeof(EditorGUI)
                    .GetMethod("ScrollableTextAreaInternal", BindingFlags.Static | BindingFlags.NonPublic, null, types, null);
            }
        }

        public static void DrawTextArea(Rect rect, SerializedProperty property)
        {
            if (property.propertyType == SerializedPropertyType.String)
            {
                scrollCache.TryAdd(property.propertyPath, Vector2.zero);
                var parameters = new object[]
                {
                rect,
                property.stringValue,
                scrollCache[property.propertyPath],
                EditorStyles.textArea
                };
                property.stringValue = (string)ScrollableTextAreaInternal.Invoke(null, parameters);
                scrollCache[property.propertyPath] = (Vector2)parameters[2];
            }
            else
            {
                EditorGUI.LabelField(rect, $"Use {nameof(DrawTextArea)} with string.");
            }
        }

        private static Dictionary<string, TextAreaHeight> cachedTextAreaHeights = new Dictionary<string, TextAreaHeight>();

        private class TextAreaHeight
        {
            public TextAreaHeight(GUIContent content, int height)
            {
                this.content = content;
                this.height = height;
            }

            public GUIContent content;
            public float height;
        }

        public static float GetTextAreaHeight(SerializedProperty property, float width, int minLines = 1, int maxLines = 10)
        {
            property.serializedObject.Update();
            if (string.IsNullOrWhiteSpace(property.stringValue))
            {
                return EditorGUIUtility.singleLineHeight;
            }

            const float lineHeight = 13f;
            cachedTextAreaHeights.TryAdd(property.propertyPath, new TextAreaHeight(new GUIContent(property.stringValue), 0));
            var cachedContent = cachedTextAreaHeights[property.propertyPath].content;
            var cachedHeight = cachedTextAreaHeights[property.propertyPath].height;
            var textAreaHeight = EditorStyles.textArea.CalcHeight(cachedContent, width);
            var baseTextAreaHeight = EditorGUIUtility.singleLineHeight;
            var result = baseTextAreaHeight + ((Mathf.Clamp(Mathf.CeilToInt(textAreaHeight / lineHeight), minLines, maxLines) - 1) * lineHeight);

            if (!Mathf.Approximately(result, cachedHeight))
            {
                if (cachedHeight > 0 &&
                    property.stringValue.Equals(cachedContent.text))
                {
                    return cachedHeight;
                }
            }

            cachedTextAreaHeights[property.propertyPath].content.text = property.stringValue;
            cachedTextAreaHeights[property.propertyPath].height = result;
            return result;
        }
    }
}
