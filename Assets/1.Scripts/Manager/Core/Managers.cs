using System.Threading.Tasks;
using _1.Scripts.Manager.Subs;
using UnityEngine;

namespace _1.Scripts.Manager.Core
{
    public class Managers : MonoBehaviour
    {
        // Sub Managers
        public static GameManager gameManager;
        public static SceneLoadManager sceneLoadManager;
        public static SpawnManager spawnManager;
        public static UIManager uiManager;
        
        // Properties
        public Task saveLoadTask = Task.CompletedTask;
        
        // Singleton
        public static Managers Instance { get; private set; }

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
            LoadData_QueuedAsync();
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
            saveLoadTask = saveLoadTask.ContinueWith(_ => gameManager.TrySaveData()).Unwrap();
        }

        /// <summary>
        /// Load User Data
        /// </summary>
        /// <remarks>Each Loading Process will be queued when a previous save or loading process is currently not done!</remarks>
        public void LoadData_QueuedAsync()
        {
            saveLoadTask = saveLoadTask.ContinueWith(_ => gameManager.TryLoadData()).Unwrap();
        }
    }
}
