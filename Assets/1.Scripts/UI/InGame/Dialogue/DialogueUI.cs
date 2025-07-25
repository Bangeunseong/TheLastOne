using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Dialogue;
using _1.Scripts.Manager.Subs;
using TMPro;
using UnityEngine;

namespace _1.Scripts.UI.InGame.Dialogue
{
    public class DialogueUI : UIBase
    {
        [Header("Dialogue UI")]
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private CanvasGroup canvasGroup;
        
        private Coroutine dialogueRoutine;
        private Coroutine fadeRoutine;
        private bool isShowing = false;

        public override void Initialize(UIManager manager, object param = null)
        {
            base.Initialize(manager, param);
            ClearTexts();
            gameObject.SetActive(false);
        }

        public override void Show()
        {
            if (fadeRoutine != null) { StopCoroutine(fadeRoutine); fadeRoutine = null; }
            canvasGroup.alpha = 0;
            gameObject.SetActive(true);
        }

        public override void Hide()
        {
            if (!gameObject.activeInHierarchy)
            {
                if (canvasGroup) canvasGroup.alpha = 0;
                gameObject.SetActive(false);
                return;
            }
            if (fadeRoutine != null) { StopCoroutine(fadeRoutine); fadeRoutine = null; }
            fadeRoutine = StartCoroutine(FadeOut(0.15f));
        }
        public override void ResetUI()
        {
            StopDialogueRoutine();
            ClearTexts();
            if (fadeRoutine != null) { StopCoroutine(fadeRoutine); fadeRoutine = null; }
            canvasGroup.alpha = 0;
            gameObject.SetActive(false);
        }
        
        private void ClearTexts()
        {
            nameText.text = "";
            dialogueText.text = "";
        }
        
        private void StopDialogueRoutine()
        {
            if (dialogueRoutine != null)
            {
                StopCoroutine(dialogueRoutine);
                dialogueRoutine = null;
            }
        }
        
        public void ShowSequence(List<DialogueData> sequence)
        {
            if (sequence == null || sequence.Count == 0) return;
            StopDialogueRoutine();
            if (fadeRoutine != null) { StopCoroutine(fadeRoutine); fadeRoutine = null; }

            gameObject.SetActive(true);
            canvasGroup.alpha = 0;
            ClearTexts();
            dialogueRoutine = StartCoroutine(PlayDialogueSequence(sequence, 0));
        }

        private IEnumerator PlayDialogueSequence(List<DialogueData> sequence, int index)
        {
            if (index >= sequence.Count)
            {
                yield return StartCoroutine(FadeOut(0.15f));
                gameObject.SetActive(false);
                yield break;
            }

            var data = sequence[index];
            nameText.text = data.Speaker;
            dialogueText.text = "";
            
            yield return StartCoroutine(FadeIn(0.15f));

            foreach (char c in data.Message)
            {
                dialogueText.text += c;
                yield return new WaitForSeconds(0.01f);
            }

            float minTime = 1.5f;
            float perChar = 0.01f;
            float wait = minTime + data.Message.Length * perChar;
            yield return new WaitForSeconds(wait);

            for (int i = dialogueText.text.Length - 1; i >= 0; i--)
            {
                dialogueText.text = dialogueText.text.Substring(0, i);
                yield return new WaitForSeconds(0.01f);
            }
            
            yield return PlayDialogueSequence(sequence, index + 1);
        }

        private IEnumerator FadeIn(float duration)
        {
            float t = 0;
            while (t < duration)
            {
                canvasGroup.alpha = Mathf.Lerp(0, 1, t / duration);
                t += Time.deltaTime;
                yield return null;
            }
            canvasGroup.alpha = 1;
        }

        private IEnumerator FadeOut(float duration)
        {
            float t = 0;
            while (t < duration)
            {
                canvasGroup.alpha = Mathf.Lerp(1, 0, t / duration);
                t += Time.deltaTime;
                yield return null;
            }
            canvasGroup.alpha = 0;
            gameObject.SetActive(false);
        }
    }
}