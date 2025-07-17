using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Subs;
using UnityEngine;

namespace _1.Scripts.UI.InGame.Mission
{
    public class MissionUI : UIBase
    {
        [SerializeField] private Transform slotContainer;
        [SerializeField] private GameObject slotPrefabGO;
        
        private List<MissionSlot> slotList = new();
        private Dictionary<int, MissionSlot> slotMap = new();

        public override void Init(UIManager manager)
        {
            base.Init(manager);
            RefreshMissions();
        }

        public override void ResetUI()
        {
            foreach (var slot in slotList)
                Destroy(slot.gameObject);
            slotList.Clear();
            slotMap.Clear();
            RefreshMissions();
        }
        public override void Show()
        {
            base.Show();
            RefreshMissions();
        }

        public override void Initialize(object param = null)
        {
            RefreshMissions();
        }
        
        private void RefreshMissions()
        {
            var questManager = CoreManager.Instance.questManager;
            foreach (var kv in questManager.activeQuests)
            {
                var quest = kv.Value;
                AddMission(
                    quest.data.questID,
                    quest.CurrentObjective.data.description,
                    quest.CurrentObjective.currentAmount,
                    quest.CurrentObjective.data.requiredAmount
                );
            }
        }
        
        public void AddMission(int questID, string missionText, int currentAmount, int requiredAmount)
        {
            if (slotMap.ContainsKey(questID)) return;

            var go = Instantiate(slotPrefabGO, slotContainer);
            var slot = go.GetComponent<MissionSlot>();

            slot.Initialize(questID, missionText, currentAmount, requiredAmount);
            slot.PlayNewMissionAnimation();

            slotList.Add(slot);
            slotMap[questID] = slot;

            SortSlots();
        }

        public void CompleteMission(int questID)
        {
            if (!slotMap.TryGetValue(questID, out var slot)) return;
            
            slotMap.Remove(questID);

            if (!gameObject.activeInHierarchy)
            {
                slotList.Remove(slot);
                Destroy(slot.gameObject);
                SortSlots();
            }
            else
            {
                StartCoroutine(RemoveSlot(slot));
            }
        }
        
        private IEnumerator RemoveSlot(MissionSlot slot)
        {
            slot.PlayCompleteAnimation();
            yield return new WaitForSeconds(0.5f);
            slotList.Remove(slot);
            Destroy(slot.gameObject);
            SortSlots();
        }

        private void SortSlots()
        {
            for (int i = 0; i < slotList.Count; i++)
            {
                slotList[i].transform.SetSiblingIndex(i);
            }
        }
        
        public void UpdateMissionProgress(int questID, int currentAmount, int requiredAmount)
        {
            if (!slotMap.TryGetValue(questID, out var slot)) return;
            slot.UpdateProgress(currentAmount, requiredAmount);
        }
    }
}