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
        
        public override void Init(UIManager manager)
        {
            base.Init(manager);
            Hide();
        }
        
        public override void ResetUI()
        {
            progressSlider.value = 0f;
            progressText.text = "0.00%";
        }
        
        public override void Show()
        {
            Service.Log("Loading UI Show");
            panel.SetActive(true);
        }
        public override void Hide()
        {
            Service.Log("Loading UI Hide");
            panel.SetActive(false);
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