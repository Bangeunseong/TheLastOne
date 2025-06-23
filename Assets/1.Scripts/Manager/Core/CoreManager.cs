using System.Threading.Tasks;
using _1.Scripts.Manager.Data;
using _1.Scripts.Manager.Subs;
using UnityEngine;

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
        [SerializeField] public ObjectPoolManager objectPoolManager;
        
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
            
            gameManager = new GameManager();
            sceneLoadManager = new SceneLoadManager();
            spawnManager = new SpawnManager();
            uiManager = new UIManager();
            resourceManager = new ResourceManager();
            objectPoolManager = new ObjectPoolManager();
        }

        private void Reset()
        {
            gameManager = new GameManager();
            sceneLoadManager = new SceneLoadManager();
            spawnManager = new SpawnManager();
            uiManager = new UIManager();
            resourceManager = new ResourceManager();
            objectPoolManager = new ObjectPoolManager();
        }

        // Start is called before the first frame update
        private void Start()
        {
            gameManager.Start();
            sceneLoadManager.Start();
            objectPoolManager.Start();
            
            StartGame();
        }

        // Update is called once per frame
        private void Update()
        {
            if (sceneLoadManager.IsLoading) { sceneLoadManager.Update(); }
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
                Debug.Log("DataTransferObject is null");
                await sceneLoadManager.OpenScene(SceneType.Stage1);
                return;
            }
            Debug.Log("DataTransferObject is not null");
            await sceneLoadManager.OpenScene(loadedData.CurrentSceneId);
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
