using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _1.Scripts.UI.InGame.Minigame
{
    public class AlphabetMatchingUI : UIBase
    {
        [Header("AlphabetMatchingUI")]
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_Text[] alphabetTexts;
        
        public override void Show() { panel.SetActive(true); }
        public override void Hide() { panel.SetActive(false); HideAll(); }
        public override void ResetUI() { Hide(); }
        
        public void CreateAlphabet(string s)
        {
            for (int i = 0; i < alphabetTexts.Length; i++)
            {
                if (i < s.Length) { alphabetTexts[i].text = s[i].ToString(); alphabetTexts[i].gameObject.SetActive(true); }
                else { alphabetTexts[i].text = ""; alphabetTexts[i].gameObject.SetActive(false); }
            }
        }
        public void ShowAlphabet(bool active) {
            foreach (var t in alphabetTexts) t.gameObject.SetActive(active);
        }
        public void AlphabetAnim(int index, bool correct)
        {
            if (index < 0 || index >= alphabetTexts.Length) return;
            var t = alphabetTexts[index];
            t.color = correct ? Color.green : Color.red;
        }
        private void HideAll()
        {
            foreach (var t in alphabetTexts) { t.text = ""; t.gameObject.SetActive(false); t.color = Color.white; }
        }
    }
}