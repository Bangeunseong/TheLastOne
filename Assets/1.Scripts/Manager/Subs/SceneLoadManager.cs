using System;
using System.Resources;
using System.Threading.Tasks;
using _1.Scripts.Manager.Core;
using Unity.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _1.Scripts.Manager.Subs
{
    public enum SceneType
    {
        Intro, 
        Loading, 
        Game,
    }
    
    [Serializable] public class SceneLoadManager
    {
        [Header("Core")]
        [SerializeField] private Managers coreManager;
        
        [Header("Scene Info.")]
        [SerializeField, ReadOnly] private string previousScene;
        [SerializeField, ReadOnly] private string currentScene;
        
        // Fields
        private AsyncOperation sceneLoad;
        private bool isInputAllowed;
        private bool isKeyPressed;
        private UIManager uiManager;
        
        // Properties
        public bool IsLoading { get; private set; }
        
        // Constructor
        public SceneLoadManager(Managers core){ coreManager = core; }
        
        // Methods
        public void Start()
        {
            // uiManager = UIManager.Instance;
            
            // currentScene = nameof(CurrentScene.Intro);
            // StartCoroutine(LoadMainScene());
        }

        public void Update()
        {
            if (!isInputAllowed) return;
            if (Input.anyKeyDown) isKeyPressed = true;
        }

        public async Task OpenScene(string sceneName)
        {
            IsLoading = true;
            
            previousScene = currentScene;
            if (previousScene != null && previousScene != sceneName)
            {
                // await ResourceManager.Instance.UnloadSceneResources(previousScene);
                currentScene = sceneName;
            }
            
            var loadingScene = SceneManager.LoadSceneAsync(nameof(SceneType.Loading));
            while (!loadingScene!.isDone)
            {
                await Task.Yield();
            }
            
            // uiManager.ChangeState(CurrentScene.Loading);
            
            Debug.Log("Resource and Scene Load Started!");
            // await ResourceManager.Instance.LoadSceneResourcesWithProgress(currentScene);
            // await GameManager.Instance.TryLoadData();
            await LoadSceneWithProgress(currentScene);
        }
        
        private async Task LoadSceneWithProgress(string sceneName)
        {
            sceneLoad = SceneManager.LoadSceneAsync(sceneName);
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
                case nameof(SceneType.Intro): // uiManager.ChangeState(SceneType.Intro);
                    break;
                case nameof(SceneType.Game): // uiManager.ChangeState(SceneType.Game);
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
