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
        [SerializeField] private Transform cellRoot;
        [SerializeField] private GameObject cellPrefab;
        
        private List<AlphabetCell> cells = new();
        
        public override void Show() { panel.SetActive(true); }
        public override void Hide() { panel.SetActive(false); HideAll(); }
        public override void ResetUI() { Hide(); }
        
        public void CreateAlphabet(string s)
        {
            foreach (var cell in cells)
                Destroy(cell.gameObject);
            cells.Clear();

            for (var i = 0; i < s.Length; i++)
            {
                var go = Instantiate(cellPrefab, cellRoot);
                if (!go.TryGetComponent(out AlphabetCell cell)) continue;
                cell.SetChar(s[i]);
                cells.Add(cell);
            }
        }

        public void ShowAlphabet(bool active)
        {
            foreach (var cell in cells) cell.gameObject.SetActive(active);
        }
        
        public void AlphabetAnim(int index, bool correct)
        {
            if (index < 0 || index >= cells.Count) return;
            
            if (correct) cells[index].PlayCorrectAnim();
            else cells[index].PlayWrongAnim();
        }
        
        private void HideAll()
        {
            foreach (var cell in cells) Destroy(cell.gameObject);
            cells.Clear();
        }
    }
}