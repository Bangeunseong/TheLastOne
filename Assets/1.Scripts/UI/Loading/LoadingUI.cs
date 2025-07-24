using _1.Scripts.Manager.Subs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _1.Scripts.UI.Loading
{
    public class LoadingUI : UIBase
    {
        [Header("Loading UI")]
        [SerializeField] private GameObject panel;
        public Slider progressSlider;
        public TextMeshProUGUI progressText;

        public override void Initialize(UIManager manager, object param = null)
        {
            base.Initialize(manager, param);
            Hide();
        }
        
        public override void ResetUI()
        {
            progressSlider.value = 0f;
            progressText.text = "0.00%";
        }
        
        public void UpdateLoadingProgress(float progress)
        {
            progressSlider.value = progress;
            progressText.text = $"{progress * 100:0.00}%";
        }
        
        public void UpdateProgressText(string text)
        {
            progressText.text = text;
        }
    }
}