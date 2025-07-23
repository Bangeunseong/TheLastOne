using System.Collections;
using _1.Scripts.Manager.Subs;
using TMPro;
using UnityEngine;

namespace _1.Scripts.UI.InGame.Dialogue
{
    public struct DialogueData
    {
        public string Speaker;
        public string Message;
        public float FadeInTime;

        public DialogueData(string speaker, string message, float fadeInTime = 0.2f)
        {
            Speaker = speaker;
            Message = message;
            FadeInTime = fadeInTime;
        }
    }
    public class DialogueUI : UIBase
    {
        [Header("Dialogue UI")]
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private CanvasGroup canvasGroup;
        
        private Coroutine typingCoroutine;
        public override void Init(UIManager manager)
        {
            base.Init(manager);
            gameObject.SetActive(false);
        }

        public override void Initialize(object param = null)
        {
            ClearText();
        }

        public override void Show()
        {
            canvasGroup.alpha = 0;
            gameObject.SetActive(true);
            StartCoroutine(FadeIn(0.2f));
        }

        public override void Hide()
        {
            if (!gameObject.activeInHierarchy)
            {
                if (canvasGroup) canvasGroup.alpha = 0;
                gameObject.SetActive(false);
                return;
            }
            if (canvasGroup) StartCoroutine(FadeOut(0.15f));
            else gameObject.SetActive(false);
        }
        public override void ResetUI()
        {
            StopTyping();
            ClearText();
        }

        private IEnumerator TypeDialogue(string sentence)
        {
            dialogueText.text = "";
            foreach (char c in sentence)
            {
                dialogueText.text += c;
                yield return new WaitForSeconds(0.025f);
            }
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
        private void StopTyping()
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
            }
        }

        private void ClearText()
        {
            nameText.text = "";
            dialogueText.text = "";
        }
    }
}