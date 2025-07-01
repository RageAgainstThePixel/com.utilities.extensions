// Licensed under the MIT License. See LICENSE in the project root for license information.

#if UNITY_UGUI
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Utilities.Extensions
{
    public class LoadSceneButton : Button
    {
        [SerializeField]
        [SceneReference]
        private string scene;

        [SerializeField]
        private LoadSceneMode loadSceneMode = LoadSceneMode.Single;

        protected override void OnEnable()
        {
            base.OnEnable();
            onClick.AddListener(LoadScene);
        }

        protected override void OnDisable()
        {
            onClick.RemoveListener(LoadScene);
            base.OnDisable();
        }

        private void LoadScene()
        {
            if (!string.IsNullOrEmpty(scene))
            {
                SceneManager.LoadScene(scene, loadSceneMode);
            }
            else
            {
                Debug.LogError("Scene name is empty. Please assign a valid scene name.");
            }
        }
    }
}
#endif
