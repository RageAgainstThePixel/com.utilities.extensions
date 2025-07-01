// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Utilities.Extensions.Editor
{
    public static class ComponentEditorUtility
    {
        private class ComponentUpgradePopup : EditorWindow
        {
            private bool _initializedPosition;

            private Component _component;
            private TypeCache.TypeCollection _types;

            private void OnEnable()
            {
                GetWindow(typeof(ComponentUpgradePopup));
            }

            public void Init(Component component, TypeCache.TypeCollection types)
            {
                _component = component;
                _types = types;
            }

            private void OnGUI()
            {
                if (!_initializedPosition)
                {
                    Vector2 mousePos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
                    position = new Rect(mousePos.x - (position.width * 0.5f), mousePos.y, position.width, position.height);
                    _initializedPosition = true;
                }

                GUILayout.Space(20);

                foreach (var type in _types.Where(type => GUILayout.Button(new GUIContent(type.Name))))
                {
                    ReplaceComponent(type, _component);
                    Close();
                }
            }
        }

        [MenuItem("CONTEXT/Component/Upgrade Component", true, 9998)]
        private static bool UpgradeValidate(MenuCommand command)
        {
            if (command.context is not Component component)
            {
                return false;
            }

            var types = TypeCache.GetTypesDerivedFrom(component.GetType());
            return types.Count > 0;
        }

        [MenuItem("CONTEXT/Component/Upgrade Component", false, 9998)]
        private static void Upgrade(MenuCommand command)
        {
            if (command.context is not Component component)
            {
                return;
            }

            var types = TypeCache.GetTypesDerivedFrom(component.GetType());

            if (types.Count == 1)
            {
                ReplaceComponent(types[0], component);
            }
            else
            {
                var typePicker = ScriptableObject.CreateInstance<ComponentUpgradePopup>();
                typePicker.Init(component, types);
                typePicker.ShowPopup();
            }
        }

        [MenuItem("CONTEXT/Component/Downgrade Component", true, 9999)]
        private static bool DowngradeValidate(MenuCommand command)
        {
            if (command.context is not Component component)
            {
                return false;
            }

            var baseType = component.GetType().BaseType;
            return baseType is { IsAbstract: false, IsClass: true } &&
                   baseType != typeof(MonoBehaviour) &&
                   baseType != typeof(Component) &&
                   baseType != typeof(Behaviour) &&
                   baseType != typeof(Transform) &&
                   baseType != typeof(AudioBehaviour);
        }

        [MenuItem("CONTEXT/Component/Downgrade Component", false, 9999)]
        private static void Downgrade(MenuCommand command)
        {
            if (command.context is not Component component)
            {
                return;
            }

            ReplaceComponent(component.GetType().BaseType, component);
        }

        private static void ReplaceComponent(Type type, Component @base)
        {
            var baseType = @base.GetType();
            var serializedSource = new SerializedObject(@base);
            var gameObject = @base.gameObject;
            Object.DestroyImmediate(@base);
            var component = gameObject.AddComponent(type);

            // if something bad happened revert
            if (component.IsNull())
            {
                component = gameObject.AddComponent(baseType);
            }

            var serializedTarget = new SerializedObject(component);
            var iterator = serializedSource.GetIterator();

            // jump into serialized object, this will skip script type so that we don't override the destination component's type
            if (iterator.NextVisible(true))
            {
                // iterate through all serializedProperties
                while (iterator.NextVisible(true))
                {
                    // try obtaining the property in destination component
                    var element = serializedTarget.FindProperty(iterator.name);

                    // validate that the properties are present in both components, and that they're the same type
                    if (element != null && element.propertyType == iterator.propertyType)
                    {
                        // copy value from source to destination component
                        serializedTarget.CopyFromSerializedProperty(iterator);
                    }
                }
            }

            serializedTarget.ApplyModifiedProperties();
        }
    }
}
