using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Manager.Subs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _1.Scripts.UI.InGame
{
    public class MinigameUI : UIBase
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Animator animator;
        [SerializeField] private GameObject enterText;
        [SerializeField] private TextMeshProUGUI countdownText;
        [SerializeField] private Transform alphabetsLayout;
        [SerializeField] private GameObject alphabetPrefab;
        [SerializeField] private Slider timeSlider;
        [SerializeField] private GameObject clearPanel;
        [SerializeField] private TextMeshProUGUI clearText;
        [SerializeField] private TextMeshProUGUI loopCountText;
        
        List<TextMeshProUGUI> alphabetCells = new List<TextMeshProUGUI>();
        List<Animator> alphabetAnimators = new List<Animator>();

        public override void Init(UIManager manager)
        {
            base.Init(manager);
            panel.SetActive(false);
        }
        public override void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }
        
        public void ShowPanel()
        {
            panel.SetActive(true);
            animator.Play("Window In");
            ShowEnterText(true);
            ShowCountdownText(false);
            ShowAlphabet(false);
            ShowTimeSlider(false);
            ShowClearText(false);
        }

        public void HidePanel()
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

        public void ShowEnterText(bool show)
        {
            enterText.SetActive(show);
        }

        public void ShowCountdownText(bool show)
        {
            countdownText.gameObject.SetActive(show);
        }

        public void SetCountdownText(float value)
        {
            countdownText.text = Mathf.CeilToInt(value).ToString();
        }

        public void ShowAlphabet(bool show)
        {
            alphabetsLayout.gameObject.SetActive(show);
        }
        
        public void CreateAlphabet(string alphabet)
        {
            foreach (Transform child in alphabetsLayout) Destroy(child.gameObject);
            alphabetCells.Clear();
            alphabetAnimators.Clear();

            for (int i = 0; i < alphabet.Length; i++)
            {
                var alphabetCell = Instantiate(alphabetPrefab, alphabetsLayout);
                var text = alphabetCell.GetComponentInChildren<TextMeshProUGUI>();
                text.text = alphabet[i].ToString();
                alphabetCells.Add(text);
                var animator = alphabetCell.GetComponent<Animator>();
                if (animator != null) alphabetAnimators.Add(animator);
            }
        }

        public void AlphabetAnim(int index, bool isCorrect)
        {
            if (index < 0 || index >= alphabetCells.Count) return;
            var animator = alphabetAnimators[index];
            if (animator != null)
                animator.SetTrigger(isCorrect ? "Correct" : "Wrong");
        }
        
        public void ShowTimeSlider(bool show)
        {
            timeSlider.gameObject.SetActive(show);
        }
        public void SetTimeSlider(float max, float value)
        {
            timeSlider.maxValue = max;
            timeSlider.value = value;
        }

        public void UpdateTimeSlider(float value)
        {
            timeSlider.value = value;
        }
        public void ShowClearText(bool show)
        {
            clearPanel.SetActive(show);
        }
        public void SetClearText(bool show, string text)
        {
            clearText.text = text;
            clearText.gameObject.SetActive(show);
        }

        public void UpdateLoopCount(int current, int max)
        {
            if (loopCountText != null)
                loopCountText.text = $"{current + 1}/{max}";
        }
        
    }
}