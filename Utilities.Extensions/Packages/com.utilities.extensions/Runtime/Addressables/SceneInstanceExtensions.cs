// Licensed under the MIT License. See LICENSE in the project root for license information.

#if UNITY_ADDRESSABLES && UTILITIES_ASYNC

using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Utilities.Extensions
{
    /// <summary>
    /// Extension methods for validating and unloading addressable scene instances.
    /// </summary>
    public static class SceneInstanceExtensions
    {
        /// <summary>
        /// Unloads an addressable scene instance if it is valid and currently loaded.
        /// </summary>
        /// <param name="instance">The scene instance to unload.</param>
        /// <param name="options">Scene unload options.</param>
        /// <returns>A task that completes when the unload operation finishes.</returns>
        public static async Task UnloadAsync(this SceneInstance instance, UnloadSceneOptions options = UnloadSceneOptions.None)
        {
            if (instance.IsValid())
            {
                if (instance.Scene.isLoaded)
                {
                    var unloadOp = Addressables.UnloadSceneAsync(instance, options, false);
                    await unloadOp.Task;
                    unloadOp.Release();
                }
                else
                {
                    Debug.LogWarning("Scene was not unloaded");
                }
            }
            else
            {
                throw new Exception("Invalid Scene Instance!");
            }
        }

        /// <summary>
        /// Determines whether the scene instance points to a valid scene path and scene handle.
        /// </summary>
        /// <param name="sceneInstance">The scene instance to validate.</param>
        /// <returns><see langword="true"/> if valid; otherwise <see langword="false"/>.</returns>
        public static bool IsValid(this SceneInstance sceneInstance)
            => !string.IsNullOrWhiteSpace(sceneInstance.Scene.path) && sceneInstance.Scene.IsValid();
    }
}
#endif // UNITY_ADDRESSABLES && UTILITIES_ASYNC
