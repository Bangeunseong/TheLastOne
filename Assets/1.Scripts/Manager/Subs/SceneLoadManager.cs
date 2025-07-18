using System;
using System.Linq;
using System.Threading.Tasks;
using _1.Scripts.Entity.Scripts.Player.Core;
using _1.Scripts.Manager.Core;
using _1.Scripts.UI.Common;
using _1.Scripts.UI.InGame;
using _1.Scripts.UI.InGame.Mission;
using _1.Scripts.UI.Loading;
using Unity.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _1.Scripts.Manager.Subs
{
    [Serializable] public enum SceneType
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
        public float LoadingProgress { get; set; }
        
        // Methods
        public void Start()
        {
            coreManager = CoreManager.Instance;
            uiManager = CoreManager.Instance.uiManager;
            CurrentScene = SceneType.IntroScene;
        }

        public void Update()
        {
            if (!isInputAllowed) return;
            if (Input.anyKeyDown) isKeyPressed = true;
        }

        public async Task OpenScene(SceneType sceneName)
        {
            Service.Log($"OpenScene : {sceneName}");
            Service.Log($"CurrentScene : {CurrentScene}, PreviousScene : {PreviousScene}");
            IsLoading = true;
            PreviousScene = CurrentScene;
            
            coreManager.soundManager.StopBGM();
            coreManager.objectPoolManager.ReleaseAll();
            coreManager.spawnManager.ClearAllSpawnedEnemies();
            uiManager.ShowUI<LoadingUI>();

            if (PreviousScene != sceneName)
            {
                await coreManager.objectPoolManager.DestroyUnusedStagePools(PreviousScene.ToString());
                await coreManager.resourceManager.UnloadAssetsByLabelAsync(PreviousScene.ToString());


                if (CurrentScene == SceneType.IntroScene)
                {
                    try
                    {
                        await coreManager.objectPoolManager.DestroyUnusedStagePools("Common");
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }
                    await coreManager.resourceManager.UnloadAssetsByLabelAsync("Common");
                    Cursor.lockState = CursorLockMode.None;
                }
                
                CurrentScene = sceneName;
            }
            Debug.Log($"씬 전환 직전, 현재 활성 씬: {SceneManager.GetActiveScene().name}");
            Debug.Log($"로드할 씬 이름: {(coreManager.IsDebug ? coreManager.DebugPrefix + nameof(SceneType.Loading) : nameof(SceneType.Loading))}");
            var loadingScene = 
                SceneManager.LoadSceneAsync(coreManager.IsDebug ? 
                    coreManager.DebugPrefix + nameof(SceneType.Loading) : nameof(SceneType.Loading));
            Debug.Log($"SceneManager.LoadSceneAsync 반환값: {loadingScene}, isDone: {loadingScene?.isDone}");
            if (loadingScene == null) Debug.LogError("씬 로드 핸들이 null임. 씬 이름, Build Settings 확인 필요!");
            while (!loadingScene!.isDone)
            {
                await Task.Yield();
            }
            Debug.Log("로딩 씬 전환 완료! (while 루프 탈출)");
            LoadingProgress = 0f;
            uiManager.GetUI<LoadingUI>()?.UpdateLoadingProgress(LoadingProgress);
            Debug.Log("Resource and Scene Load Started!");
            if (PreviousScene == SceneType.IntroScene)
            {
                await coreManager.resourceManager.LoadAssetsByLabelAsync("Common");
                await coreManager.objectPoolManager.CreatePoolsFromResourceBySceneLabelAsync("Common");
            }
            else
            {
                LoadingProgress = 0.4f;
                uiManager.GetUI<LoadingUI>()?.UpdateLoadingProgress(LoadingProgress);
            }
            
            await coreManager.resourceManager.LoadAssetsByLabelAsync(CurrentScene.ToString());
            await coreManager.objectPoolManager.CreatePoolsFromResourceBySceneLabelAsync(CurrentScene.ToString());
            coreManager.soundManager.CacheSoundGroup();
            await coreManager.soundManager.LoadClips();
            await LoadSceneWithProgress(CurrentScene);
        }
        
        private async Task LoadSceneWithProgress(SceneType sceneName)
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            sceneLoad = SceneManager.LoadSceneAsync(coreManager.IsDebug ? coreManager.DebugPrefix + sceneName : sceneName.ToString());
            sceneLoad!.allowSceneActivation = false;
            while (sceneLoad.progress < 0.9f)
            {
                uiManager.GetUI<LoadingUI>()?.UpdateLoadingProgress(LoadingProgress + sceneLoad.progress * 0.2f);
                await Task.Yield();
            }
            LoadingProgress = 1f;
            
            // Wait for user input
            isInputAllowed = true;
            uiManager.GetUI<LoadingUI>()?.UpdateLoadingProgress(LoadingProgress);
            uiManager.GetUI<LoadingUI>()?.UpdateProgressText("Press any key to continue...");
            await WaitForUserInput();
            isInputAllowed = false;
            isKeyPressed = false;
            
            sceneLoad!.allowSceneActivation = true;
            while (sceneLoad is { isDone: false })
            {
                await Task.Yield();
            }
            
            IsLoading = false;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        /// <summary>
        /// 씬 이동 시 내부에 있는 더미 건 컴포넌트를 찾아 저장 기능 부여, 나중에 특정 트리거를 찾아 저장하는 기능도 추가 가능
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="mode"></param>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            switch (CurrentScene)
            {
                case SceneType.IntroScene: break;
                case SceneType.Loading: break;
                case SceneType.EndingScene: break;
            }

            // Notice!! : 이 밑에 넣을 코드들은 본 게임에서 쓰일 것들만 넣기
            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj == null || !playerObj.TryGetComponent(out Player player)) return;
            coreManager.gameManager.Initialize_Player(player);
            player.PlayerCondition.IsPlayerHasControl = true;

            switch (CurrentScene)
            {
                case SceneType.Stage1:
                case SceneType.Stage2:
                    if (Enum.TryParse(CurrentScene.ToString(), out BgmType bgmType)) 
                        coreManager.soundManager.PlayBGM(bgmType, index: 0);
                    uiManager.HideUI<LoadingUI>();
                    uiManager.ShowUI<InGameUI>();
                    uiManager.ShowUI<MissionUI>();
                    uiManager.ShowUI<WeaponUI>();
                    uiManager.ShowUI<PauseMenuUI>();
                    break;
            }

            coreManager.questManager.Initialize(coreManager.gameManager.SaveData);
            coreManager.spawnManager.ChangeSpawnDataAndInstantiate(CurrentScene);
            if (CurrentScene == SceneType.Stage1) coreManager.spawnManager.SpawnEnemyBySpawnData(1);
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
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
