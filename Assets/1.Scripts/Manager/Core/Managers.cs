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
        }

        // Update is called once per frame
        private void Update()
        {
        
        }
    }
}
