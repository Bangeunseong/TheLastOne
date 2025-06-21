using System;
using System.Threading.Tasks;
using _1.Scripts.Entity.Scripts.Player.Core;
using _1.Scripts.Manager.Subs;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace _1.Scripts.Manager.Core
{
    public class CoreManager : MonoBehaviour
    {
        // Sub Managers
        [Header("Managers")]
        [SerializeField] public GameManager gameManager;
        [SerializeField] public SceneLoadManager sceneLoadManager;
        [SerializeField] public SpawnManager spawnManager;
        [SerializeField] public UIManager uiManager;
        [SerializeField] public ResourceManager resourceManager;
        
        // Properties
        public Task saveTask = Task.CompletedTask;
        
        // Singleton
        public static CoreManager Instance { get; private set; }

        private void Awake()
        {
            if (!Instance) { Instance = this; DontDestroyOnLoad(gameObject); } 
            else { if (Instance != this) Destroy(gameObject); }
            
            gameManager = new GameManager(this);
            sceneLoadManager = new SceneLoadManager(this);
            spawnManager = new SpawnManager(this);
            uiManager = new UIManager(this);
            resourceManager = new ResourceManager(this);
        }

        private void Reset()
        {
            gameManager = new GameManager(this);
            sceneLoadManager = new SceneLoadManager(this);
            spawnManager = new SpawnManager(this);
            uiManager = new UIManager(this);
            resourceManager = new ResourceManager(this);
        }

        // Start is called before the first frame update
        private void Start()
        {
            sceneLoadManager.Start();
            StartGame();
        }

        // Update is called once per frame
        private void Update()
        {
            if (sceneLoadManager.IsLoading){ sceneLoadManager.Update(); } 
        }

        /// <summary>
        /// Save User Data
        /// </summary>
        /// <remarks>Each Saving Process will be queued when a previous save or loading process is currently not done!</remarks>
        public void SaveData_QueuedAsync()
        {
            saveTask = saveTask.ContinueWith(_ => gameManager.TrySaveData()).Unwrap();
        }

        /// <summary>
        /// Load User Data
        /// </summary>
        /// <remarks>Each Loading Process will be queued when a previous save or loading process is currently not done!</remarks>
        public async Task LoadDataAndScene()
        {
            while(saveTask.Status != TaskStatus.RanToCompletion){ await Task.Yield(); }
            
            var loadedData = await gameManager.TryLoadData();
            SceneManager.sceneLoaded += OnSceneLoaded;
            
            if (loadedData == null)
            {
                await sceneLoadManager.OpenScene(SceneType.Stage1);
                return;
            }
            
            await sceneLoadManager.OpenScene(loadedData.CurrentSceneId);
            return;

            void OnSceneLoaded(Scene scene, LoadSceneMode mode)
            {
                var player = GameObject.FindWithTag("Player")?.GetComponent<PlayerCondition>();
                if (player != null)
                {
                    player.InitializeStat(loadedData);

                    // 위치 및 회전 적용
                    if (loadedData != null)
                    {
                        player.transform.position = loadedData.CurrentCharacterPosition.ToVector3();
                        player.transform.rotation = loadedData.CurrentCharacterRotation.ToQuaternion();
                    }
                }

                // 중복 호출 방지를 위해 제거
                SceneManager.sceneLoaded -= OnSceneLoaded;
            }
        }

        /// <summary>
        /// Start Game with the latest saved data
        /// </summary>
        public void StartGame()
        {
            _ = LoadDataAndScene();
        }
        
        /// <summary>
        /// Back to Intro Scene
        /// </summary>
        public void MoveToIntroScene() 
        {
            _ = sceneLoadManager.OpenScene(SceneType.IntroScene);
        }

        /// <summary>
        /// Save data and move to the next scene
        /// </summary>
        /// <param name="nextScene"></param>
        public void MoveToNextGameScene(SceneType nextScene)
        {
            SaveData_QueuedAsync();
            _ = LoadDataAndScene();
        }
    }
}
