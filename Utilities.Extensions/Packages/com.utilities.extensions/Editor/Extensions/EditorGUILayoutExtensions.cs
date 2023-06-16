// Licensed under the MIT License. See LICENSE in the project root for license information.

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
    }
}
