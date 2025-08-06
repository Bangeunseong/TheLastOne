using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.Npc.StatControllers.Base;
using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Data;
using _1.Scripts.Manager.Subs;
using _1.Scripts.Quests.Core;
using _1.Scripts.Util;
using UnityEngine;
using UnityEngine.Playables;

namespace _1.Scripts.Map.GameEvents
{
    public class SpawnEnemyByIndex : MonoBehaviour, IGameEventListener
    {
        [Header("Spawn Trigger Id")]
        [Tooltip("It should be same with corresponding Save Point Id")]
        [SerializeField] private int spawnIndex;

        [Header("Target Count")]
        [Tooltip("Target Count of Killed Enemies which corresponding with spawn index")]
        [SerializeField] private int targetCount;
        [Tooltip("This is for debugging. Do not touch this value!")]
        [SerializeField] private int killedCount;

        [Header("Invisible Wall")] 
        [SerializeField] private List<BoxCollider> invisibleWall = new();

        [Header("Timeline")] 
        [SerializeField] private PlayableDirector timeline;
        
        private bool isSpawned;
        private CoreManager coreManager;

        private void Awake()
        {
            if (invisibleWall.Count > 0) return;
            var list = this.TryGetChildComponents<BoxCollider>("InvisibleWalls");
            if (list is not { Length: > 0 }) return;
            invisibleWall.AddRange(list);
        }

        private void Reset()
        {
            if (invisibleWall.Count > 0) return;
            var list = this.TryGetChildComponents<BoxCollider>("InvisibleWalls");
            if (list is not { Length: > 0 }) return;
            invisibleWall.AddRange(list);
        }

        private void Start()
        {
            coreManager = CoreManager.Instance;
            
            DataTransferObject save = coreManager.gameManager.SaveData;
            if (save == null ||
                !save.stageInfos.TryGetValue(coreManager.sceneLoadManager.CurrentScene, out var info) ||
                !info.completionDict.TryGetValue(spawnIndex + BaseEventIndex.BaseSavePointIndex + 1, out var val)) return;

            if (!val) return;
            isSpawned = true;
            enabled = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isSpawned || !other.CompareTag("Player")) return;
            
            if (!coreManager.spawnManager.CurrentSpawnData.EnemySpawnPoints.TryGetValue(spawnIndex,
                    out var spawnPoints))
            {
                Debug.LogError("Couldn't find spawn point, Target Count is currently zero!");
                return;
            }
            
            Debug.Log("Spawned!");

            foreach (var point in spawnPoints)
                targetCount += point.Key is EnemyType.ShebotRifleDuo or EnemyType.ShebotSwordDogDuo ? point.Value.Count * 2 : point.Value.Count;
            
            GameEventSystem.Instance.RegisterListener(this);
            coreManager.spawnManager.SpawnEnemyBySpawnData(spawnIndex);
            if (invisibleWall.Count > 0)
                foreach (var wall in invisibleWall) wall.transform.parent.gameObject.SetActive(true);
            
            isSpawned = true;
            
            if (timeline) PlayCutScene(timeline);
        }
        
        public void OnEventRaised(int eventID)
        {
            if (eventID != BaseEventIndex.BaseSpawnEnemyIndex + spawnIndex) return;
            
            killedCount++;
            if (killedCount < targetCount) return;
            
            if (timeline) coreManager.soundManager.PlayBGM(BgmType.Stage2, 0);
            if (invisibleWall.Count > 0)
                foreach (var wall in invisibleWall) wall.transform.parent.gameObject.SetActive(false);
            GameEventSystem.Instance.UnregisterListener(this);
            enabled = false;
        }
        
        private void PlayCutScene(PlayableDirector director)
        {
            director.played += OnCutsceneStarted;
            director.stopped += OnCutsceneStopped;
            director.Play();
        }
        
        private void OnCutsceneStarted(PlayableDirector director)
        {
            coreManager.gameManager.Player.InputProvider.enabled = false;
            coreManager.gameManager.PauseGame();
            
            // BGM 변경 필요?
            coreManager.soundManager.PlayBGM(BgmType.Stage2, 1);
            
            coreManager.gameManager.Player.PlayerCondition.UpdateLowPassFilterValue(coreManager.gameManager.Player.PlayerCondition.HighestPoint);
            coreManager.uiManager.OnCutsceneStarted(director);
        }
        
        private void OnCutsceneStopped(PlayableDirector director)
        {
            var player = coreManager.gameManager.Player;
            player.PlayerCondition.UpdateLowPassFilterValue(player.PlayerCondition.LowestPoint + (player.PlayerCondition.HighestPoint - player.PlayerCondition.LowestPoint) * ((float)player.PlayerCondition.CurrentHealth / player.PlayerCondition.MaxHealth));
            player.InputProvider.enabled = true;
            
            coreManager.gameManager.ResumeGame();
            coreManager.uiManager.OnCutsceneStopped(director);
            
            director.played -= OnCutsceneStarted;
            director.stopped -= OnCutsceneStopped;
        }
    }
}
