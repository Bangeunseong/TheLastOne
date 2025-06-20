using System.Threading.Tasks;
using _1.Scripts.Manager.Subs;
using UnityEngine;

namespace _1.Scripts.Manager.Core
{
    public class CoreManager : MonoBehaviour
    {
        // Sub Managers
        public static GameManager gameManager;
        public static SceneLoadManager sceneLoadManager;
        public static SpawnManager spawnManager;
        public static UIManager uiManager;
        
        // Properties
        public Task saveTask = Task.CompletedTask;
        
        // Singleton
        public static CoreManager Instance { get; private set; }

        private void Awake()
        {
            if (!Instance) { Instance = this; DontDestroyOnLoad(gameObject); } 
            else { if (Instance != this) Destroy(gameObject); }
        }

        // Start is called before the first frame update
        private void Start()
        {
            gameManager = new GameManager(this);
            sceneLoadManager = new SceneLoadManager(this);
            spawnManager = new SpawnManager(this);
            uiManager = new UIManager(this);
            
            sceneLoadManager.Start();
            SaveData_QueuedAsync();
            _ = LoadData_QueuedAsync();
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
        public async Task LoadData_QueuedAsync()
        {
            while(saveTask.Status != TaskStatus.RanToCompletion){ await Task.Yield(); }
            
            var loadedData = await gameManager.TryLoadData();
            if (loadedData == null)
            {
                await sceneLoadManager.OpenScene(SceneType.Stage1);
                return;
            }
            gameManager.ApplyLoadedData(loadedData);
            await sceneLoadManager.OpenScene(loadedData.CurrentSceneId);
        }

        /// <summary>
        /// Start Game with the latest saved data
        /// </summary>
        public void StartGame()
        {
            _ = LoadData_QueuedAsync();
        }

        /// <summary>
        /// Save data and move to the next scene
        /// </summary>
        /// <param name="nextScene"></param>
        public void MoveToNextScene(SceneType nextScene)
        {
            SaveData_QueuedAsync();
            _ = LoadData_QueuedAsync();
        }
    }
}
