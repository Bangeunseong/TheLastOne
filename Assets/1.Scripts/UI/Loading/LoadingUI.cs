using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Subs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace _1.Scripts.UI.Loading
{
    public class LoadingUI : UIBase
    {
        [Header("Loading UI")]
        public Slider progressSlider;
        public TextMeshProUGUI progressText;
        
        public override void Init(UIManager manager)
        {
            base.Init(manager);
        }
        
        /*
        public async void LoadScene(SceneType sceneType)
        {
            await uiManager.ShowPopup<LoadingUI>("LoadingUI");
            await uiManager.LoadScene(sceneType);
        }*/
        
        public override void SetActive(bool active)
        {
            gameObject.SetActive(active);
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