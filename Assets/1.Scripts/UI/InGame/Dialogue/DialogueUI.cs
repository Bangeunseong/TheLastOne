using System.Collections;
using TMPro;
using UnityEngine;

namespace _1.Scripts.UI.InGame.Dialogue
{
    public class DialogueUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private CanvasGroup canvasGroup;
        private Coroutine typingCoroutine;

        public void ShowPrompt(string speaker, string message, float fadeInTime = 0.2f)
        {
            gameObject.SetActive(true);
            nameText.text = speaker;
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypeDialogue(message));

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0;
                StartCoroutine(FadeIn(fadeInTime));
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

        private IEnumerator TypeDialogue(string sentence)
        {
            dialogueText.text = "";
            foreach (char c in sentence)
            {
                dialogueText.text += c;
                yield return new WaitForSeconds(0.025f);
            }
        }

        public void HidePrompt(float fadeOutTime = 0.15f)
        {
            if (canvasGroup != null)
                StartCoroutine(FadeOut(fadeOutTime));
            else
            {
                gameObject.SetActive(false);
            }
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