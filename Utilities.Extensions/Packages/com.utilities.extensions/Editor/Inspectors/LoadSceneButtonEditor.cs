// Licensed under the MIT License. See LICENSE in the project root for license information.

#if UNITY_UGUI
using UnityEditor;
using UnityEditor.UI;

namespace Utilities.Extensions.Editor
{
    [CustomEditor(typeof(LoadSceneButton))]
    public class LoadSceneButtonEditor : ButtonEditor
    {
        private SerializedProperty scene;
        private SerializedProperty loadSceneMode;

        protected override void OnEnable()
        {
            base.OnEnable();
            scene = serializedObject.FindProperty(nameof(scene));
            loadSceneMode = serializedObject.FindProperty(nameof(loadSceneMode));
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space();

            serializedObject.Update();
            EditorGUILayout.PropertyField(scene);
            EditorGUILayout.PropertyField(loadSceneMode);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
