using System;
using System.Resources;
using System.Threading.Tasks;
using _1.Scripts.Entity.Scripts.Player.Core;
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
        [field: Header("Scene Info.")]
        [field: SerializeField, ReadOnly] public SceneType PreviousScene { get; private set; }
        [field: SerializeField, ReadOnly] public SceneType CurrentScene { get; private set; }
        
        // Fields
        private AsyncOperation sceneLoad;
        private bool isInputAllowed;
        private bool isKeyPressed;
        private UIManager uiManager;
        private CoreManager coreManager;
        
        // Properties
        public bool IsLoading { get; private set; }
        
        // Methods
        public void Start()
        {
            // uiManager = UIManager.Instance;
            coreManager = CoreManager.Instance;
            CurrentScene = SceneType.IntroScene;
        }

        public void Update()
        {
            if (!isInputAllowed) return;
            if (Input.anyKeyDown) isKeyPressed = true;
        }

        public async Task OpenScene(SceneType sceneName)
        {
            IsLoading = true;
            
            PreviousScene = CurrentScene;
            if (PreviousScene != sceneName)
            {
                await coreManager.objectPoolManager.DestroyUnusedStagePools(PreviousScene.ToString());
                await coreManager.resourceManager.UnloadAssetsByLabelAsync(PreviousScene.ToString());
                if (CurrentScene == SceneType.IntroScene) await coreManager.resourceManager.UnloadAssetsByLabelAsync("Common");
                CurrentScene = sceneName;
            }
            
            var loadingScene = 
                SceneManager.LoadSceneAsync(coreManager.IsDebug ? 
                    coreManager.DebugPrefix + nameof(SceneType.Loading) : nameof(SceneType.Loading));
            while (!loadingScene!.isDone)
            {
                await Task.Yield();
            }
            
            // uiManager.LoadingUI.UpdateLoadingProgress(0f);
            // uiManager.ChangeState(CurrentScene.Loading);
            
            Debug.Log("Resource and Scene Load Started!");
            if (PreviousScene == SceneType.IntroScene) await coreManager.resourceManager.LoadAssetsByLabelAsync("Common");
            // uiManager.LoadingUI.UpdateLoadingProgress(0.2f);
            await coreManager.resourceManager.LoadAssetsByLabelAsync(CurrentScene.ToString());
            // uiManager.LoadingUI.UpdateLoadingProgress(0.4f);
            await coreManager.objectPoolManager.CreatePoolsFromResourceBySceneLabelAsync(CurrentScene.ToString());
            // uiManager.LoadingUI.UpdateLoadingProgress(0.6f);
            await LoadSceneWithProgress(CurrentScene);
        }
        
        private async Task LoadSceneWithProgress(SceneType sceneName)
        {
            sceneLoad = SceneManager.LoadSceneAsync(coreManager.IsDebug ? coreManager.DebugPrefix + sceneName : sceneName.ToString());
            sceneLoad!.allowSceneActivation = false;
            while (sceneLoad.progress < 0.9f)
            {
                // uiManager.LoadingUI.UpdateLoadingProgress(0.6f + sceneLoad.progress * 0.4f);
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
            
            var obj = GameObject.FindWithTag("Player");
            if (obj != null && obj.TryGetComponent(out Player player))
            {
                coreManager.gameManager.Initialize_Player(player);
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
