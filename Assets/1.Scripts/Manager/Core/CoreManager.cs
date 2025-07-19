using System;
using System.Linq;
using System.Threading.Tasks;
using _1.Scripts.Manager.Data;
using _1.Scripts.Manager.Subs;
using UnityEngine;

namespace _1.Scripts.Manager.Core
{
    public class CoreManager : MonoBehaviour
    {
        [Header("Components")] 
        [SerializeField] private AudioSource audioSource;
        
        // Sub Managers
        [Header("Managers")]
        [SerializeField] public GameManager gameManager;
        [SerializeField] public SceneLoadManager sceneLoadManager;
        [SerializeField] public SpawnManager spawnManager;
        [SerializeField] public UIManager uiManager;
        [SerializeField] public ResourceManager resourceManager;
        [SerializeField] public ObjectPoolManager objectPoolManager;
        [SerializeField] public QuestManager questManager;
        [SerializeField] public SoundManager soundManager;
        [SerializeField] public TimeScaleManager timeScaleManager;
        
        [field: Header("Debug")]
        [field: SerializeField] public bool IsDebug { get; private set; } = true;
        [field: SerializeField] public string DebugPrefix { get; private set; } = "TestScene_";
        
        private Task saveTask = Task.CompletedTask;
        
        // Singleton
        public static CoreManager Instance { get; private set; }

        private void Awake()
        {
            if (!Instance) { Instance = this; DontDestroyOnLoad(gameObject); } 
            else { if (Instance != this) Destroy(gameObject); }

            if (!audioSource) audioSource = this.TryGetComponent<AudioSource>();
            
            gameManager = new GameManager();
            sceneLoadManager = new SceneLoadManager();
            spawnManager = new SpawnManager();
            uiManager = new UIManager();
            resourceManager = new ResourceManager();
            objectPoolManager = new ObjectPoolManager();
            questManager = new QuestManager();
            soundManager = new SoundManager();
            timeScaleManager = new TimeScaleManager();
        }

        private void Reset()
        {
            if (!audioSource) audioSource = this.TryGetComponent<AudioSource>();
            
            gameManager = new GameManager();
            sceneLoadManager = new SceneLoadManager();
            spawnManager = new SpawnManager();
            uiManager = new UIManager();
            resourceManager = new ResourceManager();
            objectPoolManager = new ObjectPoolManager();
            questManager = new QuestManager();
            soundManager = new SoundManager();
            timeScaleManager = new TimeScaleManager();
        }

        // Start is called before the first frame update
        private async void Start()
        {
            uiManager.Start();
            gameManager.Start();
            sceneLoadManager.Start();
            objectPoolManager.Start();
            resourceManager.Start();
            soundManager.Start(audioSource);
            timeScaleManager.Start();
            spawnManager.Start();
            questManager.Start();
            
            await resourceManager.LoadAssetsByLabelAsync("IntroScene");
            soundManager.CacheSoundGroup();
            await soundManager.LoadClips();
        }

        // Update is called once per frame
        private void Update()
        {
            if (sceneLoadManager.IsLoading) { sceneLoadManager.Update(); }
        }

        private void OnDestroy()
        {
            resourceManager.OnDestroy();
        }

        /// <summary>
        /// Save User Data
        /// </summary>
        /// <remarks>Each Saving Process will be queued when a previous save or loading process is currently not done!</remarks>
        public void SaveData_QueuedAsync()
        {
            saveTask = saveTask.ContinueWith(_ => gameManager.TrySaveData()).Unwrap();
        }

        public async Task LoadScene(SceneType sceneType)
        {
            while(saveTask.Status != TaskStatus.RanToCompletion){ await Task.Yield(); }
            await sceneLoadManager.OpenScene(sceneType);
        }

        /// <summary>
        /// Load User Data
        /// </summary>
        /// <remarks>Each Loading Process will be queued when a previous save or loading process is currently not done!</remarks>
        public async Task LoadDataAndScene()
        {
            while(saveTask.Status != TaskStatus.RanToCompletion){ await Task.Yield(); }
            
            await gameManager.TryLoadData();
            DataTransferObject loadedData = gameManager.SaveData;
            if (loadedData == null)
            {
                // Service.Log("DataTransferObject is null");
                await sceneLoadManager.OpenScene(SceneType.Stage1);
                return;
            }
            // Service.Log("DataTransferObject is not null");
            await sceneLoadManager.OpenScene(loadedData.currentSceneId);
        }

        /// <summary>
        /// Start Game with new game data
        /// </summary>
        public void StartGame()
        {
            gameManager.TryRemoveSavedData();
            _ = LoadScene(SceneType.Stage1);
        }

        /// <summary>
        /// Reload Game with the latest saved data
        /// </summary>
        public void ReloadGame()
        {
            questManager.Reset();
            spawnManager.Reset();
            timeScaleManager.Reset();
            uiManager.InGameUI?.ResetUI();
            gameManager.ExitGame();
            _ = LoadDataAndScene();
        }
        
        /// <summary>
        /// Back to Intro Scene
        /// </summary>
        public void MoveToIntroScene()
        {
            questManager.Reset();
            spawnManager.Reset();
            timeScaleManager.Reset();
            uiManager.InGameUI?.ResetUI();
            gameManager.ExitGame();
            _ = LoadScene(SceneType.IntroScene);
        }

        /* - Extensions for Sub Managers - */
        public T GetComponentOfTarget<T>(GameObject target) where T : Component
        {
            if (!target) return null;
            if (target.TryGetComponent(out T component)) return component;
            Service.Log($"Can't find component of type {typeof(T)} in {target.name}");
            return null;
        }

        public T GetComponentInChildrenOfTarget<T>(GameObject target, string childrenName = null, bool inActive = false) where T : Component
        {
            if (!target) return null;
            if (childrenName == null)
            {
                var component = target.GetComponentInChildren<T>(inActive);
                if (component != null) return component;
                Service.Log($"Can't find component of type {typeof(T)} in children of {target.name}");
                return null;
            }

            var specificComponent = target.GetComponentsInChildren<T>(inActive);
            return specificComponent.FirstOrDefault(component => component.gameObject.name == childrenName);
        }

        public T GetComponentInParentOfTarget<T>(GameObject target, string parentName = null, bool inActive = false)
            where T : Component
        {
            if (!target) return null;
            if (parentName == null)
            {
                var component = target.GetComponentInParent<T>(inActive);
                if (component != null) return component;
                Service.Log($"Can't find component of type {typeof(T)} in parent of {target.name}");
                return null;
            }
            
            var specificComponent = target.GetComponentsInParent<T>(inActive);
            
            return specificComponent.FirstOrDefault(component => component.gameObject.name == parentName);
        }
        /* ------------------------------------- */
    }
}
