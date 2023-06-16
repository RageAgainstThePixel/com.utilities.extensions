using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Utilities.Extensions.Editor
{
    [CustomPropertyDrawer(typeof(SerializedDictionary<,>), true)]
    public class SerializedDictionaryPropertyDrawer : PropertyDrawer
    {
        private class SerializedDictionaryObject
        {
            public SerializedDictionaryObject(SerializedProperty property)
            {
                this.property = property;
                keyData = property.FindPropertyRelative(nameof(keyData));
                valueData = property.FindPropertyRelative(nameof(valueData));
                Assert.IsTrue(keyData.isArray && valueData.isArray && keyData.arraySize == valueData.arraySize);
            }

            private readonly SerializedProperty property;

            public readonly SerializedProperty keyData;

            public readonly SerializedProperty valueData;

            public int? selectedElement { get; set; }

            public int arraySize => keyData.arraySize;

            public KeyValuePair<SerializedProperty, SerializedProperty> tempItem;

            public KeyValuePair<SerializedProperty, SerializedProperty> GetArrayElementAtIndex(int index)
            {
                UpdateSerializedObject();
                return new KeyValuePair<SerializedProperty, SerializedProperty>(
                    keyData.GetArrayElementAtIndex(index),
                    valueData.GetArrayElementAtIndex(index));
            }

            public List<KeyValuePair<SerializedProperty, SerializedProperty>> ToList()
            {
                UpdateSerializedObject();
                var size = arraySize;
                var list = new List<KeyValuePair<SerializedProperty, SerializedProperty>>(size);

                for (var i = 0; i < size; i++)
                {
                    var keyProperty = keyData.GetArrayElementAtIndex(i);
                    var valueProperty = valueData.GetArrayElementAtIndex(i);
                    list.Add(new KeyValuePair<SerializedProperty, SerializedProperty>(keyProperty, valueProperty));
                }

                return list;
            }

            public bool TryAddNewItem(out KeyValuePair<SerializedProperty, SerializedProperty> item)
            {
                UpdateSerializedObject();

                var items = ToList();

                foreach (var (key, value) in items)
                {
                    if (key.IsDefaultValue())
                    {
                        item = default;
                        return false;
                    }
                }

                var index = keyData.arraySize;
                keyData.InsertArrayElementAtIndex(index);
                valueData.InsertArrayElementAtIndex(index);
                var keyProperty = keyData.GetArrayElementAtIndex(index);
                keyProperty.SetDefaultValue();
                var valueProperty = valueData.GetArrayElementAtIndex(index);
                valueProperty.SetDefaultValue();
                ApplyModifiedProperties();
                item = new KeyValuePair<SerializedProperty, SerializedProperty>(keyProperty, valueProperty);
                return true;
            }

            public void RemoveItemAt(int index)
            {
                UpdateSerializedObject();
                keyData.DeleteArrayElementAtIndex(index);
                valueData.DeleteArrayElementAtIndex(index);
                ApplyModifiedProperties();
            }

            public void RemoveLastItem() => RemoveItemAt(arraySize - 1);

            public void UpdateSerializedObject()
            {
                property.serializedObject.Update();
                keyData.serializedObject.Update();
                valueData.serializedObject.Update();
            }

            public void ApplyModifiedProperties()
            {
                property.serializedObject.ApplyModifiedProperties();
                keyData.serializedObject.ApplyModifiedProperties();
                valueData.serializedObject.ApplyModifiedProperties();
            }

            public void ApplyModifiedPropertiesWithoutUndo()
            {
                property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                keyData.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                valueData.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static readonly Dictionary<string, ReorderableList> reorderableListCache = new Dictionary<string, ReorderableList>();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);

            var serializedDictionary = new SerializedDictionaryObject(property);

            if (property.isExpanded)
            {
                if (!reorderableListCache.TryGetValue(property.propertyPath, out var list))
                {
                    list = new ReorderableList(serializedDictionary.ToList(), typeof(KeyValuePair<,>), true, true, true, true);
                    list.drawHeaderCallback += rect => { EditorGUI.LabelField(rect, label); };
                    list.drawElementCallback += (rect, index, active, focused) =>
                    {
                        OnListDrawElementCallback(rect, index, active, focused, list, serializedDictionary);
                    };
                    list.onAddCallback += reorderableList =>
                    {
                        OnListOnAddCallback(reorderableList, serializedDictionary);
                    };
                    list.onRemoveCallback += reorderableList =>
                    {
                        OnListRemoveCallback(reorderableList, serializedDictionary);
                    };
                    reorderableListCache[property.propertyPath] = list;
                }

                list.DoList(position);
            }

            EditorGUI.EndProperty();
        }

        private void OnListDrawElementCallback(Rect rect, int index, bool active, bool focused, ReorderableList reorderableList, SerializedDictionaryObject serializedDictionary)
        {
            serializedDictionary.UpdateSerializedObject();
            var item = serializedDictionary.GetArrayElementAtIndex(index);

            if (focused || active)
            {
                serializedDictionary.selectedElement = index;
            }

            EditorGUI.BeginChangeCheck();
            var keyRect = new Rect(rect)
            {
                width = rect.width * 0.45f
            };

            EditorGUI.PropertyField(keyRect, item.Key, GUIContent.none);
            var valueRect = new Rect(rect)
            {
                width = rect.width * 0.55f,
                position = new Vector2(rect.x + keyRect.width + 4, rect.y)
            };

            EditorGUI.PropertyField(valueRect, item.Value, GUIContent.none);

            if (EditorGUI.EndChangeCheck())
            {
                serializedDictionary.ApplyModifiedProperties();
            }
        }

        private void OnListOnAddCallback(ReorderableList reorderableList, SerializedDictionaryObject serializedDictionary)
        {
            if (serializedDictionary.TryAddNewItem(out var item))
            {
                reorderableList.list.Add(item);
                reorderableList.Select(reorderableList.count - 1);
            }
        }

        private void OnListRemoveCallback(ReorderableList reorderableList, SerializedDictionaryObject serializedDictionary)
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

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (reorderableListCache.TryGetValue(property.propertyPath, out var list))
            {
                return list.GetHeight() + EditorGUIUtility.singleLineHeight;
            }

            return base.GetPropertyHeight(property, label);
        }
    }
}
