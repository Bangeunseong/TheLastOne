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
        [SerializeField] private TextMeshProUGUI pressEnterText;
        [SerializeField] private TextMeshProUGUI countdownText;
        [SerializeField] private Transform alphabetsLayout;
        [SerializeField] private GameObject alphabetPrefab;
        [SerializeField] private Slider timeSlider;
        [SerializeField] private GameObject clearPanel;
        [SerializeField] private TextMeshProUGUI clearText;
        [SerializeField] private TextMeshProUGUI loopCountText;
        
        List<TextMeshProUGUI> alphabetCells = new List<TextMeshProUGUI>();

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
            animator.Play("Window Out");
            panel.SetActive(false);
        }

        public void ShowEnterText(bool show)
        {
            enterText.SetActive(show);
            pressEnterText.gameObject.SetActive(show);
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

            for (int i = 0; i < alphabet.Length; i++)
            {
                var alphabetCell = Instantiate(alphabetPrefab, alphabetsLayout);
                var text = alphabetCell.GetComponentInChildren<TextMeshProUGUI>();
                text.text = alphabet[i].ToString();
                alphabetCells.Add(text);
            }
        }

        public void AlphabetAnim(int index, bool isCorrect)
        {
            if (index < 0 || index >= alphabetCells.Count) return;
            var animator = alphabetCells[index].GetComponent<Animator>();
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
            clearText.gameObject.SetActive(show);
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