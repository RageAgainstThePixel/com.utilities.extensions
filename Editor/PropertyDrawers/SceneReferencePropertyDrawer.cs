using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Utilities.Extensions.Editor
{
    /// <summary>
    /// This property drawer allows you to select a scene from the project in the Unity Inspector.
    /// It works with string fields that are decorated with the <see cref="SceneReferenceAttribute"/>.
    /// The selected scene's path will be stored as a string in the field.
    /// If the scene is not already in the build settings, it will be added automatically.
    /// </summary>
    [CustomPropertyDrawer(typeof(SceneReferenceAttribute))]
    public class SceneReferencePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (attribute is not SceneReferenceAttribute sceneReferenceAttribute)
            {
                return;
            }

            if (property.propertyType == SerializedPropertyType.String)
            {
                EditorGUI.BeginChangeCheck();
                var sceneAsset = GetSceneObject(property.stringValue);
                sceneAsset = (SceneAsset)EditorGUI.ObjectField(position, property.displayName, sceneAsset, typeof(SceneAsset), true);

                if (EditorGUI.EndChangeCheck())
                {
                    property.stringValue = AssetDatabase.GetAssetOrScenePath(sceneAsset);
                    ValidateSceneInBuildSettings(sceneAsset);
                }
            }
            else
            {
                EditorGUI.LabelField(position, label.text, $"Use {nameof(SceneReferenceAttribute)} with string fields only.");
            }
        }

        private static SceneAsset GetSceneObject(string scenePath) =>
            string.IsNullOrWhiteSpace(scenePath)
                ? null
                : AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);

        private static void ValidateSceneInBuildSettings(SceneAsset sceneAsset)
        {
            if (sceneAsset == null)
            {
                return;
            }

            var scenePath = AssetDatabase.GetAssetPath(sceneAsset);
            var sceneGuid = new GUID(AssetDatabase.AssetPathToGUID(scenePath));
            var editorSceneAsset = new EditorBuildSettingsScene(sceneGuid, true);

            if (EditorBuildSettings.scenes == null)
            {
                EditorBuildSettings.scenes = new[]
                {
                editorSceneAsset
            };
            }
            else
            {
                var inSceneList = false;
                var editorScenes = EditorBuildSettings.scenes.ToList();

                foreach (var editorScene in editorScenes)
                {
                    if (editorScene.guid == sceneGuid)
                    {
                        editorScene.enabled = true;
                        inSceneList = true;
                    }
                }

                if (!inSceneList)
                {
                    editorScenes.Add(editorSceneAsset);
                }

                EditorBuildSettings.scenes = editorScenes.ToArray();
            }
        }
    }
}
