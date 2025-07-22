using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Manager.Subs;
using _1.Scripts.UI.InGame.Minigame;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _1.Scripts.UI.InGame
{
    public class MinigameUI : UIBase
    {
        [Header("Minigame UI")]
        [SerializeField] private GameObject panel;
        [SerializeField] private Animator animator;
        [SerializeField] private TextMeshProUGUI clearText;
        [SerializeField] private TextMeshProUGUI loopText;
        [SerializeField] private TextMeshProUGUI countdownText;
        [SerializeField] private GameObject enterText;
        [SerializeField] private Slider timeSlider;
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private Transform contentRoot;
        [SerializeField] private AlphabetMatchingUI alphabetMatchingUI;

        public override void ResetUI()
        {
            ShowPanel(false);
            ShowCountdownText(false);
            ShowClearText(false);
            ShowEnterText(false);
            ShowTimeSlider(false);
            ShowLoopText(false);
        }

        public void ShowPanel(bool show = true)
        {
            panel.SetActive(show);
            animator.Play("Window In");
            ShowEnterText(true);
            ShowCountdownText(false);
            ShowTimeSlider(false);
            ShowClearText(false);
        }
        public override void Hide()
        {
            StartCoroutine(HidePanelCoroutine());
        }

        private IEnumerator HidePanelCoroutine()
        {
            animator.Play("Window Out");
            yield return new WaitForSeconds(0.5f);
            panel.SetActive(false);
            yield return null;
        }

        public void ShowCountdownText(bool show = true) => countdownText.gameObject.SetActive(show);
        public void ShowClearText(bool show = true) => clearText.gameObject.SetActive(show);
        public void ShowEnterText(bool show = true) => enterText.gameObject.SetActive(show);
        public void ShowTimeSlider(bool show = true) => timeSlider.gameObject.SetActive(show);
        public void ShowLoopText(bool show = true) => loopText.gameObject.SetActive(show);

        public void SetMinigameContent(GameObject contentPrefab)
        {
            foreach (Transform child in contentRoot) Destroy(child.gameObject);
            var content = Instantiate(contentPrefab, contentRoot);
            content.SetActive(true);
        }

        public void SetCountdownText(float t)
        {
            countdownText.text = t > 0 ? t.ToString("F0") : "0";
        }

        public void SetClearText(bool success, string text)
        {
            clearText.text = text;
            clearText.color = success ? Color.cyan : Color.red;
        }

        public void SetTimeSlider(float current, float max)
        {
            timeSlider.maxValue = max;
            timeSlider.value = current;
            timeText.text = $"{current:0.00}s";
        }

        public void UpdateTimeSlider(float current)
        {
            timeSlider.value = current;
            timeText.text = $"{current:0.00}s";
        }

        public void UpdateLoopCount(int current, int max)
        {
            loopText.text = $"{current}/{max}";
        }

        public void ShowAlphabetMatching(bool show = true)
        {
            alphabetMatchingUI.gameObject.SetActive(show);
            if (!show) alphabetMatchingUI.ResetUI();
        }

        public AlphabetMatchingUI GetAlphabetMatchingUI() => alphabetMatchingUI;

        public void ShowMiniGame()
        {
            ShowPanel();
            ShowEnterText(true);
            ShowClearText(false);
            ShowTimeSlider(false);
            ShowCountdownText(false);
            ShowLoopText(true);
            ShowAlphabetMatching(true);
        }

        public void HideMiniGame()
        {
            
        }
    }
}