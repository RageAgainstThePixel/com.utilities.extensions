// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Utilities.Extensions.Editor
{
    /// <summary>
    /// Wraps a serialized dictionary property and provides helper operations for editing key/value entries.
    /// </summary>
    public class SerializedDictionaryObject
    {
        /// <summary>
        /// Initializes a new wrapper for the provided serialized dictionary property.
        /// </summary>
        /// <param name="property">The root serialized dictionary property containing key and value arrays.</param>
        public SerializedDictionaryObject(SerializedProperty property)
        {
            this.property = property;
            guid = property.GetUniqueIdentifier();
            keyData = property.FindPropertyRelative(nameof(keyData));
            valueData = property.FindPropertyRelative(nameof(valueData));
            Debug.Assert(keyData.isArray && valueData.isArray && keyData.arraySize == valueData.arraySize);
        }

        /// <summary>
        /// Gets a stable identifier for this serialized dictionary instance.
        /// </summary>
        public readonly string guid;

        private readonly SerializedProperty property;

        /// <summary>
        /// Gets the serialized key array backing this dictionary.
        /// </summary>
        public readonly SerializedProperty keyData;

        /// <summary>
        /// Gets the serialized value array backing this dictionary.
        /// </summary>
        public readonly SerializedProperty valueData;

        internal int? selectedElement { get; set; }

        /// <summary>
        /// Gets the number of key/value entries currently stored in the dictionary.
        /// </summary>
        public int size => IsNull() ? 0 : keyData.arraySize;

        /// <summary>
        /// Checks whether the wrapped serialized object is null or no longer valid.
        /// </summary>
        /// <returns><see langword="true"/> when the wrapped serialized object is null; otherwise <see langword="false"/>.</returns>
        public bool IsNull() => property.serializedObject.IsNull();

        /// <summary>
        /// Gets the key/value serialized properties for the item at the given index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to retrieve.</param>
        /// <returns>A key/value pair of serialized properties for the requested element.</returns>
        public KeyValuePair<SerializedProperty, SerializedProperty> GetArrayElementAtIndex(int index)
        {
            UpdateSerializedObject();
            return new KeyValuePair<SerializedProperty, SerializedProperty>(
                keyData.GetArrayElementAtIndex(index),
                valueData.GetArrayElementAtIndex(index));
        }

        /// <summary>
        /// Materializes all key/value entry properties into a list.
        /// </summary>
        /// <returns>A list containing key/value serialized properties for each dictionary entry.</returns>
        public List<KeyValuePair<SerializedProperty, SerializedProperty>> ToList()
        {
            UpdateSerializedObject();
            var list = new List<KeyValuePair<SerializedProperty, SerializedProperty>>(size);

            for (var i = 0; i < size; i++)
            {
                var keyProperty = keyData.GetArrayElementAtIndex(i);
                var valueProperty = valueData.GetArrayElementAtIndex(i);
                list.Add(new KeyValuePair<SerializedProperty, SerializedProperty>(keyProperty, valueProperty));
            }

            return list;
        }

        internal bool CanAddNewItem()
        {
            if (IsNull()) { return false; }

            var items = ToList();

            foreach (var (key, _) in items)
            {
                if (key.IsDefaultValue())
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Tries to append a new default key/value entry to the serialized dictionary.
        /// </summary>
        /// <param name="item">When this method returns true, contains the newly created key/value serialized properties.</param>
        /// <returns><see langword="true"/> if a new entry was added; otherwise <see langword="false"/>.</returns>
        public bool TryAddNewEmptyItem(out KeyValuePair<SerializedProperty, SerializedProperty> item)
        {
            if (!CanAddNewItem())
            {
                item = default;
                return false;
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

        /// <summary>
        /// Removes the key/value entry at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the entry to remove.</param>
        public void RemoveItemAt(int index)
        {
            UpdateSerializedObject();
            keyData.DeleteArrayElementAtIndex(index);
            valueData.DeleteArrayElementAtIndex(index);
            ApplyModifiedProperties();
        }

        /// <summary>
        /// Removes the last key/value entry in the serialized dictionary.
        /// </summary>
        public void RemoveLastItem() => RemoveItemAt(size - 1);

        /// <summary>
        /// Updates all backing serialized objects before reading or writing properties.
        /// </summary>
        public void UpdateSerializedObject()
        {
            property.serializedObject.Update();
            keyData.serializedObject.Update();
            valueData.serializedObject.Update();
        }

        /// <summary>
        /// Applies pending serialized property modifications with undo support.
        /// </summary>
        public void ApplyModifiedProperties()
        {
            property.serializedObject.ApplyModifiedProperties();
            keyData.serializedObject.ApplyModifiedProperties();
            valueData.serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Applies pending serialized property modifications without recording undo.
        /// </summary>
        public void ApplyModifiedPropertiesWithoutUndo()
        {
            property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            keyData.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            valueData.serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
