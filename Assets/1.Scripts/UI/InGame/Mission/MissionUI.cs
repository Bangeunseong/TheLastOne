using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _1.Scripts.UI.InGame.Mission
{
    public class MissionUI : MonoBehaviour
    {
        [SerializeField] private Transform slotContainer;
        [SerializeField] private MissionSlot slotPrefab;
        
        private List<MissionSlot> slotList = new List<MissionSlot>();

        public void AddMission(string missionText)
        {
            MissionSlot slot = Instantiate(slotPrefab, slotContainer);
            slot.SetMission(missionText, true);
            slot.PlayNewMissionAnimation();
            slotList.Add(slot);
            
            SortSlots();
        }

        public void CompleteMission(string missionText)
        {
            var slot = slotList.Find(s => s.missionText.text == missionText);
            if (slot != null)
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
    }
}