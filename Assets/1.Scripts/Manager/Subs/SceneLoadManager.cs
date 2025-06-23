using System;
using System.Resources;
using System.Threading.Tasks;
using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Data;
using Unity.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = System.Object;

namespace _1.Scripts.Manager.Subs
{
    public enum SceneType
    {
        IntroScene, 
        Loading, 
        Stage1,
        Stage2,
        EndingScene,
    }
    
    [Serializable] public class SceneLoadManager
    {
        [Header("Core")]
        [SerializeField] private CoreManager coreManager;
        
        [Header("Scene Info.")]
        [SerializeField, ReadOnly] private SceneType previousScene;
        [SerializeField, ReadOnly] private SceneType currentScene;
        
        // Fields
        private AsyncOperation sceneLoad;
        private bool isInputAllowed;
        private bool isKeyPressed;
        private UIManager uiManager;
        
        // Properties
        public bool IsLoading { get; private set; }
        
        // Constructor
        public SceneLoadManager(CoreManager core){ coreManager = core; }
        
        // Methods
        public void Start()
        {
            // uiManager = UIManager.Instance;
            
            currentScene = SceneType.IntroScene;
            // StartCoroutine(LoadMainScene());
        }

        public void Update()
        {
            if (!isInputAllowed) return;
            if (Input.anyKeyDown) isKeyPressed = true;
        }

        public async Task OpenScene(SceneType sceneName)
        {
            IsLoading = true;
            
            previousScene = currentScene;
            if (previousScene != sceneName)
            {
                await coreManager.resourceManager.UnloadAssetsByLabelAsync(previousScene.ToString());
                currentScene = sceneName;
            }
            
            var loadingScene = SceneManager.LoadSceneAsync(coreManager.IsDebug ? coreManager.DebugPrefix + nameof(SceneType.Loading) : nameof(SceneType.Loading));
            while (!loadingScene!.isDone)
            {
                await Task.Yield();
            }
            
            // uiManager.ChangeState(CurrentScene.Loading);
            
            Debug.Log("Resource and Scene Load Started!");
            await coreManager.resourceManager.LoadAssetsByLabelAsync(currentScene.ToString());
            await LoadSceneWithProgress(currentScene);
        }
        
        private async Task LoadSceneWithProgress(SceneType sceneName)
        {
            sceneLoad = SceneManager.LoadSceneAsync(coreManager.IsDebug ? coreManager.DebugPrefix + sceneName : sceneName.ToString());
            sceneLoad!.allowSceneActivation = false;
            while (sceneLoad.progress < 0.9f)
            {
                // uiManager.LoadingUI.UpdateLoadingProgress(sceneLoad.progress);
                await Task.Yield();
            }
            
            // Wait for user input
            isInputAllowed = true;
            // uiManager.LoadingUI.UpdateLoadingProgress(1f);
            // uiManager.LoadingUI.UpdateProgressText("Press any key to continue...");
            await WaitForUserInput();
            isInputAllowed = false;
            
            sceneLoad!.allowSceneActivation = true;
            while (sceneLoad is { isDone: false }) 
            {
                await Task.Yield();
            }

            switch (sceneName)
            { 
                case SceneType.IntroScene: // uiManager.ChangeState(SceneType.Intro);
                    break;
                case SceneType.Stage1:
                case SceneType.Stage2:// uiManager.ChangeState(SceneType.Game);
                    break;
            }

            IsLoading = false;
        }
        
        private async Task WaitForUserInput()
        {
            while (!isKeyPressed)
            {
                await Task.Yield();
            }
        }

        // Scene Loading Test Method (Deprecated)
        // private IEnumerator LoadMainScene()
        // {
        //     yield return new WaitForSeconds(1);
        //     _ = OpenScene(nameof(CurrentScene.Main));
        // }
    }
}
