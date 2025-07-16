using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Manager.Core;
using UnityEngine;

namespace _1.Scripts.UI.InGame.Mission
{
    public class MissionUI : MonoBehaviour
    {
        [SerializeField] private Transform slotContainer;
        [SerializeField] private GameObject slotPrefabGO;
        
        private List<MissionSlot> slotList = new List<MissionSlot>();
        private Dictionary<int, MissionSlot> slotMap = new Dictionary<int, MissionSlot>();

        public void ResetUI()
        {
            foreach (var slot in slotList)
                Destroy(slot.gameObject);
            slotList.Clear();
            slotMap.Clear();
        }

        public void Initialize()
        {
            var questManager = CoreManager.Instance.questManager;
            foreach (var kv in questManager.activeQuests)
            {
                var quest = kv.Value;
                AddMission(quest.data.questID, quest.CurrentObjective.data.description, quest.CurrentObjective.currentAmount, quest.CurrentObjective.data.requiredAmount);
            }
        }
        
        public void AddMission(int questID, string missionText, int currentAmount, int requiredAmount)
        {
            if (slotMap.ContainsKey(questID)) return;

            Debug.Log($"AddMission 호출: {questID} / {missionText}");
            Debug.Log($"slotPrefabGO: {slotPrefabGO}");
            
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
            if (!gameObject.activeInHierarchy)
            {
                slotList.Remove(slot);
                Destroy(slot.gameObject);
                SortSlots();
                yield break;
            }
            slot.PlayCompleteAnimation();
            yield return new WaitForSeconds(0.5f);
            slotList.Remove(slot);
            Destroy(slot.gameObject, 1f);
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