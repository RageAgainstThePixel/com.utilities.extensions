// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Utilities.Extensions.Editor
{
    [CustomPropertyDrawer(typeof(SerializedDictionary<,>), true)]
    public class SerializedDictionaryPropertyDrawer : PropertyDrawer
    {
        private static readonly Dictionary<string, ReorderableList> reorderableListCache = new Dictionary<string, ReorderableList>();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);

            var serializedDictionary = new SerializedDictionaryObject(property);

            if (!reorderableListCache.TryGetValue(serializedDictionary.guid, out var list) ||
                list.count != serializedDictionary.size)
            {
                list = new ReorderableList(serializedDictionary.ToList(), typeof(KeyValuePair<,>), false, true, true, true);
                list.drawHeaderCallback += rect => { EditorGUI.LabelField(rect, label); };
                list.drawElementCallback += (rect, index, active, focused) =>
                {
                    OnListDrawElementCallback(rect, index, active, focused, serializedDictionary);
                };
                list.elementHeightCallback += index => OnListElementHeightCallback(index, serializedDictionary);
                list.onCanAddCallback += _ => serializedDictionary.CanAddNewItem();
                list.onAddCallback += reorderableList =>
                {
                    OnListOnAddCallback(reorderableList, serializedDictionary);
                };
                list.onRemoveCallback += reorderableList =>
                {
                    OnListRemoveCallback(reorderableList, serializedDictionary);
                };
                reorderableListCache[serializedDictionary.guid] = list;
            }

            list.DoList(position);

            EditorGUI.EndProperty();
        }

        #region List Callbacks

        private static void OnListDrawElementCallback(Rect rect, int index, bool active, bool focused, SerializedDictionaryObject serializedDictionary)
        {
            serializedDictionary.UpdateSerializedObject();
            var (key, value) = serializedDictionary.GetArrayElementAtIndex(index);

            if (focused || active)
            {
                serializedDictionary.selectedElement = index;
            }

            EditorGUI.BeginChangeCheck();

            var lineHeight = EditorGUIUtility.singleLineHeight;
            var keyRect = new Rect(rect.x, rect.y, rect.width, lineHeight);
            var valueHeight = EditorGUILayoutExtensions.GetTextAreaHeight(value, rect.width);
            var valueRect = new Rect(rect.x, rect.y + lineHeight + EditorGUIUtility.standardVerticalSpacing, rect.width, valueHeight);

            EditorGUI.PropertyField(keyRect, key, GUIContent.none);

            if (value.propertyType == SerializedPropertyType.String)
            {
                EditorGUILayoutExtensions.DrawTextArea(valueRect, value);
            }
            else
            {
                EditorGUI.PropertyField(valueRect, value, GUIContent.none);
            }

            if (EditorGUI.EndChangeCheck())
            {
                serializedDictionary.ApplyModifiedProperties();
            }
        }

        private static float OnListElementHeightCallback(int index, SerializedDictionaryObject serializedDictionary)
        {
            serializedDictionary.UpdateSerializedObject();
            var (key, value) = serializedDictionary.GetArrayElementAtIndex(index);

            // Calculate heights taking into account multi-line serialized properties for value property
            var keyHeight = EditorGUI.GetPropertyHeight(key, true);
            var valueHeight = value.propertyType switch
            {
                SerializedPropertyType.String => EditorGUILayoutExtensions.GetTextAreaHeight(value, EditorGUIUtility.currentViewWidth),
                _ => EditorGUI.GetPropertyHeight(value, true)
            };

            // Key and value property heights and twice the spacing (above and below the value property)
            return keyHeight + valueHeight + (EditorGUIUtility.standardVerticalSpacing * 2);
        }

        private static void OnListOnAddCallback(ReorderableList reorderableList, SerializedDictionaryObject serializedDictionary)
        {
            if (serializedDictionary.TryAddNewEmptyItem(out var item))
            {
                reorderableList.list.Add(item);
                reorderableList.Select(reorderableList.count - 1);
            }
        }

        private static void OnListRemoveCallback(ReorderableList reorderableList, SerializedDictionaryObject serializedDictionary)
        {
            if (serializedDictionary.selectedElement.HasValue)
            {
                serializedDictionary.RemoveItemAt(serializedDictionary.selectedElement.Value);
                reorderableList.list.RemoveAt(serializedDictionary.selectedElement.Value);
            }
            else
            {
                serializedDictionary.RemoveLastItem();
                reorderableList.list.RemoveAt(reorderableList.list.Count - 1);
            }

            reorderableList.Select(reorderableList.count - 1);
        }

        #endregion List Callbacks

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            => reorderableListCache.TryGetValue(property.GetUniqueIdentifier(), out var list)
                ? list.GetHeight()
                : base.GetPropertyHeight(property, label);
    }
}
