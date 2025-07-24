using System.Collections.Generic;
using System.Linq;
using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Subs;
using _1.Scripts.Quests.Core;
using _1.Scripts.Quests.Data;
using _1.Scripts.UI.InGame.Mission;
using _1.Scripts.UI.InGame.Quest;
using UnityEngine;

namespace _1.Scripts.UI.InGame.HUD
{
    public class QuestUI : UIBase
    {
        [SerializeField] private Transform questSlotContainer;
        [SerializeField] private GameObject questSlotPrefab;
        [SerializeField] private QuestManager questManager;
        
        private readonly List<QuestSlot> questSlots = new();
        private List<QuestData> questListCache = new();
        private Dictionary<int, List<ObjectiveProgress>> objectiveDictCache = new();
        
        public override void Initialize(UIManager manager, object param = null)
        {
            base.Initialize(manager, param);
            
            var questManager = param as QuestManager ?? CoreManager.Instance.questManager;
            
            questListCache = questManager.activeQuests.Values.Select(q => q.data).ToList();
            objectiveDictCache = questManager.activeQuests.ToDictionary(kv => kv.Key, kv => kv.Value.Objectives.Values.ToList());
            foreach (var quest in questManager.activeQuests.Values)
            {
                Service.Log($"[QuestUI] QuestID={quest.data.questID}, currentObjectiveIndex={quest.currentObjectiveIndex}, objectives.Count={quest.Objectives.Count}");
                foreach (var obj in quest.Objectives)
                {
                    var prog = obj.Value;
                    Service.Log($"  [Objective] targetID={prog.data.targetID}, desc={prog.data.description}, current={prog.currentAmount}, required={prog.data.requiredAmount}, completed={prog.IsCompleted}");
                }
            }
            
            LoadQuestData();
            SetQuestSlots();
            SetMainQuestNavigation();
            Refresh();
            gameObject.SetActive(false);
        }

        public override void ResetUI()
        {
            ClearAll();
            questListCache.Clear();
            objectiveDictCache.Clear();
        }
        
        private void LoadQuestData()
        {
            var questManager = CoreManager.Instance.questManager;
            questListCache = questManager.activeQuests.Values.Select(q => q.data).ToList();
            objectiveDictCache = questManager.activeQuests.ToDictionary(
                kv => kv.Key,
                kv => kv.Value.Objectives.Values.ToList()
            );
        }
        
        private void SetQuestSlots()
        {
            if (questSlots.Count > 0) ClearAll();
            foreach (var questData in questListCache)
            {
                var go = Instantiate(questSlotPrefab, questSlotContainer, false);
                if (!go.TryGetComponent(out QuestSlot questSlot)) { Destroy(go); return; }
                if (objectiveDictCache.TryGetValue(questData.questID, out var obj))
                {
                    questSlot.Initialize(questData, obj);
                    questSlots.Add(questSlot);
                }
                else Destroy(go);
            }
        }

        private void ClearAll()
        {
            foreach (var slot in questSlots)
                if (slot) Destroy(slot.gameObject);
            questSlots.Clear();
        }

        public void Refresh()
        {
            for (int i = 0; i < questSlots.Count; i++)
            {
                var questSlot = questSlots[i];
                if (i < questListCache.Count && objectiveDictCache.TryGetValue(questListCache[i].questID, out var objectives))
                {
                    questSlot.UpdateQuestProgress();
                    questSlot.RefreshObjectiveSlots();
                }
            }
        }

        private void SetMainQuestNavigation()
        {
            var questManager = CoreManager.Instance.questManager;
            if (questManager.activeQuests.TryGetValue(0, out var mainQuest))
            {
                var targetObjective = mainQuest.Objectives.Values.FirstOrDefault(obj => !obj.IsCompleted);
                if (targetObjective != null)
                    QuestTargetBinder.Instance.SetCurrentTarget(targetObjective.data.targetID);
            }
        }

        public void ToggleObjectiveSlot()
        {
            foreach (var slot in questSlots)
                slot.ToggleObjectiveSlot();
        }
    }
}
