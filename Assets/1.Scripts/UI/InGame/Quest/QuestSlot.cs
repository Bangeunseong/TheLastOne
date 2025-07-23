using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Quests.Core;
using _1.Scripts.Quests.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _1.Scripts.UI.InGame.Quest
{
    public class QuestSlot : MonoBehaviour
    {
        [Header("Quest Info")]
        [SerializeField] private TextMeshProUGUI questTitleText;
        [SerializeField] private Slider questProgressSlider;
        [SerializeField] private TextMeshProUGUI questProgressText;
        
        [Header("Quest Objectives")]
        [SerializeField] private GameObject objectiveSlotPrefab;
        [SerializeField] private Transform objectiveSlotContainer;

        [SerializeField] private RectTransform panelRect;
        
        private List<ObjectiveSlot> objectiveSlots = new();
        private bool isExpanded = false;
        private QuestData questData;
        private List<ObjectiveProgress> objectives;
        
        private Coroutine expandCoroutine;
        private float collapsedHeight = 80f;
        private float expandedHeight = 0f;

        public void Initialize(QuestData data, List<ObjectiveProgress> objectiveList)
        {
            questData = data;
            objectives = objectiveList;
            questTitleText.text = questData.title;
            UpdateQuestProgress();
            CreateObjectiveSlots();
            SetPanelHeight(collapsedHeight);
        }

        private void CreateObjectiveSlots()
        {
            foreach (var slot in objectiveSlots)
                if (slot) Destroy(slot.gameObject);
            objectiveSlots.Clear();

            foreach (var obj in objectives)
            {
                var go = Instantiate(objectiveSlotPrefab, objectiveSlotContainer);
                var slot = go.GetComponent<ObjectiveSlot>();
                slot.Initialize(obj);
                objectiveSlots.Add(slot);
            }
        }

        public void UpdateQuestProgress()
        {
            int completed = objectives.FindAll(o => o.IsCompleted).Count;
            float progress = objectives.Count > 0 ? (float)completed / objectives.Count : 0;
            questProgressSlider.value = progress;
            questProgressText.text = $"{completed}/{objectives.Count}";
        }

        public void RefreshObjectiveSlots()
        {
            for (int i = objectiveSlots.Count - 1; i >= 0; i--)
            {
                var slot = objectiveSlots[i];
                if (slot.IsCompleted())
                {
                    slot.PlayCompleteAndDestroy();
                    objectiveSlots.RemoveAt(i);
                }
                else
                {
                    slot.UpdateProgress();
                }
            }
        }
        
        public void ToggleObjectiveSlot()
        {
            isExpanded = !isExpanded;
            foreach (var slot in objectiveSlots)
            {
                if (isExpanded) slot.Expand();
                else slot.Collapse();
            }
            float slotHeight = 40f;
            expandedHeight = collapsedHeight + (isExpanded ? objectiveSlots.Count * slotHeight : 0);
            AnimatePanelHeight(isExpanded ? expandedHeight : collapsedHeight);
        }
        private void SetPanelHeight(float height)
        {
            panelRect.sizeDelta = new Vector2(panelRect.sizeDelta.x, height);
        }

        private void AnimatePanelHeight(float targetHeight, float duration = 0.2f)
        {
            if (expandCoroutine != null) StopCoroutine(expandCoroutine);
            expandCoroutine = StartCoroutine(PanelHeightLerp(targetHeight, duration));
        }

        private IEnumerator PanelHeightLerp(float target, float duration)
        {
            float start = panelRect.sizeDelta.y;
            float time = 0f;
            while (time < duration)
            {
                time += Time.unscaledDeltaTime;
                float t = time / duration;
                float newHeight = Mathf.Lerp(start, target, t);
                panelRect.sizeDelta = new Vector2(panelRect.sizeDelta.x, newHeight);
                yield return null;
            }
            panelRect.sizeDelta = new Vector2(panelRect.sizeDelta.x, target);
        }
    }
}