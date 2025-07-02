using System.Threading.Tasks;
using _1.Scripts.Manager.Data;
using _1.Scripts.Manager.Subs;
using _1.Scripts.UI;
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
            soundManager = new SoundManager();
        }

        // Start is called before the first frame update
        private void Start()
        {
            uiManager.Start();
            gameManager.Start();
            sceneLoadManager.Start();
            objectPoolManager.Start();
            resourceManager.Start();
            soundManager.Start(audioSource);
            timeScaleManager.Start();

            // gameManager.TryLoadSettingData().Wait();
            // if (gameManager.SettingData != null) { }
        }

        // Update is called once per frame
        private void Update()
        {
            if (sceneLoadManager.IsLoading) { sceneLoadManager.Update(); }
            timeScaleManager.Update();
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
            
            await gameManager.TryLoadData();
            DataTransferObject loadedData = gameManager.SaveData;
            if (loadedData == null)
            {
                Service.Log("DataTransferObject is null");
                await sceneLoadManager.OpenScene(SceneType.Stage1);
                return;
            }
            Service.Log("DataTransferObject is not null");
            await sceneLoadManager.OpenScene(loadedData.currentSceneId);
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
