using System;
using System.Threading.Tasks;
using _1.Scripts.Entity.Scripts.Player.Core;
using _1.Scripts.Manager.Core;
using _1.Scripts.UI.InGame.HUD;
using _1.Scripts.UI.Inventory;
using _1.Scripts.UI.Loading;
using _1.Scripts.UI.Lobby;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Playables;
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
            IsLoading = true;
            PreviousScene = CurrentScene;
            
            // Reset All Managers
            coreManager.soundManager.StopBGM();
            coreManager.objectPoolManager.ReleaseAll();
            coreManager.spawnManager.ClearAllSpawnedEnemies();
            coreManager.uiManager.HideHUD();
            coreManager.uiManager.ResetUIByGroup(UIType.InGame);
            uiManager.UnregisterDynamicUIByGroup(UIType.InGame);
            
            // Remove all remain resources that belongs to previous scene
            if (PreviousScene != sceneName)
            {
                await coreManager.objectPoolManager.DestroyUnusedStagePools(PreviousScene.ToString());
                await coreManager.resourceManager.UnloadAssetsByLabelAsync(PreviousScene.ToString());
                CurrentScene = sceneName;
                if (CurrentScene == SceneType.IntroScene)
                {
                    await coreManager.objectPoolManager.DestroyUnusedStagePools("Common");
                    await coreManager.resourceManager.UnloadAssetsByLabelAsync("Common");
                }
            }
            
            // Hide Lobby and Show Loading
            uiManager.HideUI<LobbyUI>();
            uiManager.ShowUI<LoadingUI>();
            
            var loadingScene = 
                SceneManager.LoadSceneAsync(coreManager.IsDebug ? 
                    coreManager.DebugPrefix + nameof(SceneType.Loading) : nameof(SceneType.Loading));
            while (!loadingScene!.isDone) { await Task.Yield(); }
            
            // Update Loading Progress
            LoadingProgress = 0f;
            var loadingUI = uiManager.GetUI<LoadingUI>();
            loadingUI.UpdateLoadingProgress(LoadingProgress);
            
            Debug.Log("Resource and Scene Load Started!");
            if (PreviousScene == SceneType.IntroScene)
            {
                // Load Common Resource used in Game Scene
                await coreManager.resourceManager.LoadAssetsByLabelAsync("Common");
            }
            else
            {
                LoadingProgress = 0.4f;
                loadingUI.UpdateLoadingProgress(LoadingProgress);
            }
            
            // Load Resources & Create Pool used in Current Scene
            await coreManager.resourceManager.LoadAssetsByLabelAsync(CurrentScene.ToString());
            coreManager.dialogueManager.CacheDialogueData();
            coreManager.soundManager.CacheSoundGroup();
            await coreManager.soundManager.LoadClips();
            await coreManager.objectPoolManager.CreatePoolsFromResourceBySceneLabelAsync(CurrentScene.ToString());
            
            // Load Scene
            await LoadSceneWithProgress(CurrentScene);
        }
        
        /// <summary>
        /// Current Scene이 로드되는 Task (sceneLoaded event가 실행된다)
        /// </summary>
        /// <param name="sceneName"></param>
        private async Task LoadSceneWithProgress(SceneType sceneName)
        {
            var loadingUI = uiManager.GetUI<LoadingUI>();
            
            SceneManager.sceneLoaded += OnSceneLoaded;
            sceneLoad = SceneManager.LoadSceneAsync(coreManager.IsDebug ? coreManager.DebugPrefix + sceneName : sceneName.ToString());
            sceneLoad!.allowSceneActivation = false;
            while (sceneLoad.progress < 0.9f)
            {
                loadingUI.UpdateLoadingProgress(LoadingProgress + sceneLoad.progress * 0.2f);
                await Task.Yield();
            }
            LoadingProgress = 1f;
            
            // Wait for user input
            isInputAllowed = true;
            loadingUI.UpdateLoadingProgress(LoadingProgress);
            loadingUI.UpdateProgressText("Press any key to continue...");
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
                case SceneType.IntroScene: 
                    coreManager.soundManager.PlayBGM(BgmType.Lobby, 0);
                    uiManager.HideUI<LoadingUI>();
                    uiManager.ShowUI<LobbyUI>();
                    break;
                case SceneType.Loading:
                case SceneType.EndingScene: 
                    break;
            }

            coreManager.spawnManager.DisposeAllUniTasksFromSpawnedEnemies();
            coreManager.CreateNewCTS();
            
            // Notice!! : 이 밑에 넣을 코드들은 본 게임에서 쓰일 것들만 넣기
            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj == null || !playerObj.TryGetComponent(out Player player)) return;
            coreManager.gameManager.Initialize_Player(player);
            coreManager.spawnManager.ChangeSpawnDataAndInstantiate(CurrentScene);
            coreManager.questManager.Initialize(coreManager.gameManager.SaveData);
            
            uiManager.HideUI<LoadingUI>();
            uiManager.RegisterDynamicUIByGroup(UIType.InGame);

            switch (CurrentScene)
            {
                case SceneType.Stage1: 
                    // Play Cutscene If needed
                    var introGo = GameObject.Find("IntroOpening");
                    var playable = introGo?.GetComponentInChildren<PlayableDirector>();
                    if (playable && coreManager.gameManager.SaveData == null)
                    {
                        playable.played += OnCutsceneStarted_IntroOfStage1;
                        playable.played += uiManager.OnCutsceneStarted;
                        playable.stopped += OnCutsceneStopped_IntroOfStage1;
                        playable.stopped += uiManager.OnCutsceneStopped;
                        playable.Play();
                    }
                    else
                    {
                        player.PlayerCondition.OnEnablePlayerMovement();
                        coreManager.spawnManager.SpawnEnemyBySpawnData(1);
                        if (!coreManager.uiManager.ShowHUD()) throw new MissingReferenceException();
                    }
                    if (Enum.TryParse(CurrentScene.ToString(), out BgmType type)) 
                        coreManager.soundManager.PlayBGM(type, index: 0);
                    break;
                case SceneType.Stage2:
                    if (Enum.TryParse(CurrentScene.ToString(), out BgmType bgmType)) 
                        coreManager.soundManager.PlayBGM(bgmType, index: 0);
                    break;
            }
            
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

        private void OnCutsceneStarted_IntroOfStage1(PlayableDirector director)
        {
            coreManager.gameManager.PauseGame();
            coreManager.gameManager.Player.PlayerCondition.UpdateLowPassFilterValue(coreManager.gameManager.Player.PlayerCondition.HighestPoint);
            coreManager.uiManager.OnCutsceneStarted(director);
        }

        private void OnCutsceneStopped_IntroOfStage1(PlayableDirector director)
        {
            var playerGo = GameObject.FindWithTag("Player");
            if (playerGo == null || !playerGo.TryGetComponent(out Player player)) return;
            player.PlayerCondition.UpdateLowPassFilterValue(player.PlayerCondition.LowestPoint + (player.PlayerCondition.HighestPoint - player.PlayerCondition.LowestPoint) * ((float)player.PlayerCondition.CurrentHealth / player.PlayerCondition.MaxHealth));
            
            coreManager.spawnManager.SpawnEnemyBySpawnData(1);
            coreManager.gameManager.ResumeGame();
            coreManager.uiManager.OnCutsceneStopped(director);
            
            director.played -= OnCutsceneStarted_IntroOfStage1;
            director.stopped -= OnCutsceneStopped_IntroOfStage1;
        }

        // Scene Loading Test Method (Deprecated)
        // private IEnumerator LoadMainScene()
        // {
        //     yield return new WaitForSeconds(1);
        //     _ = OpenScene(nameof(CurrentScene.Main));
        // }
    }
}
