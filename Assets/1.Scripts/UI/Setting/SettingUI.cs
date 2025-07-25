using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Michsky.UI.Shift;
using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Subs;

namespace _1.Scripts.UI.Setting
{
    public class SettingUI : MonoBehaviour
    {
        [Header("Tutorial")]
        [SerializeField] private SwitchManager tutorialSwitch;

        [Header("Language")]
        [SerializeField] private HorizontalSelector languageSelector;

        [Header("Audio Volume")]
        [SerializeField] private SliderManager masterVolumeSlider;
        [SerializeField] private SliderManager bgmVolumeSlider;
        [SerializeField] private SliderManager sfxVolumeSlider;

        [Header("Sensitivity")]
        [SerializeField] private SliderManager lookSensitivitySlider;
        [SerializeField] private SliderManager aimSensitivitySlider;

        [Header("Key Bindings")]
        // TODO: 키 바인딩 추가

        [Header("Graphics")]
        [SerializeField] private HorizontalSelector resolutionSelector;
        [SerializeField] private HorizontalSelector fullscreenModeSelector;

        private Resolution[] resolutions;
        private readonly List<string> fullscreenModes = new List<string> { "Fullscreen", "Borderless", "Windowed" };

        private void Start()
        {
            resolutions = Screen.resolutions; 
            InitSensitivitySliders(); 
            InitResolutionSelector(); 
            InitFullscreenModeSelector();
            
            LoadSettings();
            AddListeners();
        }
        
        /*private void InitLanguageOptions()
        {
            if (languageSelector == null) return;
            languageSelector.saveValue = false;
            languageSelector.loopSelection = false;
            languageSelector.itemList.Clear();

            var languages = new List<string> { "English", "한국어" };
            foreach (var lang in languages)
            {
                languageSelector.CreateNewItem(lang);
                int idx = languageSelector.itemList.Count - 1;
                languageSelector.itemList[idx].onValueChanged.AddListener(() => OnLanguageChanged(idx));
            }

            languageSelector.index = PlayerPrefs.GetInt("LanguageIndex", 0);
            languageSelector.UpdateUI();
        }*/

        private void InitSensitivitySliders()
        {
            //float lookDef = CoreManager.Instance.gameManager.LookSensitivity;
            //float aimDef  = CoreManager.Instance.gameManager.AimSensitivity;
            lookSensitivitySlider.enableSaving = false;
            aimSensitivitySlider.enableSaving  = false;
        }

        private void InitResolutionSelector()
        {
            if (resolutionSelector == null) return;
            resolutionSelector.saveValue = false;
            resolutionSelector.loopSelection = false;
            resolutionSelector.itemList.Clear();

            for (int i = 0; i < resolutions.Length; i++)
            {
                var r = resolutions[i];
                string option = $"{r.width}x{r.height} {r.refreshRateRatio}hz";
                resolutionSelector.CreateNewItem(option);
            }

            for (int i = 0; i < resolutionSelector.itemList.Count; i++)
            {
                int idx = i;
                resolutionSelector.itemList[i].onValueChanged.AddListener(() => OnResolutionChanged(idx));
            }
            
            int defaultIdx = PlayerPrefs.GetInt("ResolutionIndex", GetCurrentResolutionIndex());
            resolutionSelector.index = defaultIdx;
            resolutionSelector.UpdateUI();
        }

        private void InitFullscreenModeSelector()
        {
            if (fullscreenModeSelector == null) return;
            fullscreenModeSelector.saveValue = false;
            fullscreenModeSelector.loopSelection = false;
            fullscreenModeSelector.itemList.Clear();

            for (int i = 0; i < fullscreenModes.Count; i++)
            {
                fullscreenModeSelector.CreateNewItem(fullscreenModes[i]);
            }

            for (int i = 0; i < fullscreenModes.Count; i++)
            {
                int idx = i;
                fullscreenModeSelector.itemList[i].onValueChanged.AddListener(() => OnFullscreenModeChanged(idx));
            }

            int defaultMode = PlayerPrefs.GetInt("FullscreenMode", Screen.fullScreen ? 0 : 2);
            fullscreenModeSelector.index = defaultMode;
            fullscreenModeSelector.UpdateUI();
        }

        private int GetCurrentResolutionIndex()
        {
            for (int i = 0; i < resolutions.Length; i++)
            {
                var r = resolutions[i];
                if (r.width == Screen.currentResolution.width &&
                    r.height == Screen.currentResolution.height &&
                    r.refreshRateRatio.value.Equals(Screen.currentResolution.refreshRateRatio.value))
                    return i;
            }
            return 0;
        }
        

        private void LoadSettings()
        {
            SoundManager sm = CoreManager.Instance.soundManager;
            sm.SetMasterVolume(PlayerPrefs.GetFloat(masterVolumeSlider.sliderTag + "SliderValue",
                masterVolumeSlider.defaultValue));
            sm.SetBGMVolume(PlayerPrefs.GetFloat(bgmVolumeSlider.sliderTag + "SliderValue",
                bgmVolumeSlider.defaultValue));
            sm.SetSFXVolume(PlayerPrefs.GetFloat(sfxVolumeSlider.sliderTag + "SliderValue",
                sfxVolumeSlider.defaultValue));

            //float lookVal = PlayerPrefs.GetFloat("LookSensitivity", CoreManager.Instance.gameManager.LookSensitivity);
            //float aimVal  = PlayerPrefs.GetFloat("AimSensitivity", CoreManager.Instance.gameManager.AimSensitivity);
            //lookSensitivitySlider.GetComponent<Slider>().value = lookVal;
            //aimSensitivitySlider.GetComponent<Slider>().value  = aimVal;
        }

        private void AddListeners()
        {
            tutorialSwitch.OnEvents.AddListener(() => OnTutorialToggled(true));
            tutorialSwitch.OffEvents.AddListener(() => OnTutorialToggled(false));
            
            masterVolumeSlider.GetComponent<Slider>().onValueChanged.AddListener(OnMasterVolumeChanged);
            bgmVolumeSlider.GetComponent<Slider>().onValueChanged.AddListener(OnBGMVolumeChanged);
            sfxVolumeSlider.GetComponent<Slider>().onValueChanged.AddListener(OnSFXVolumeChanged);

            //lookSensitivitySlider.GetComponent<Slider>().onValueChanged.AddListener(OnLookSensitivityChanged);
            //aimSensitivitySlider.GetComponent<Slider>().onValueChanged.AddListener(OnAimSensitivityChanged);
        }
        

        private void OnTutorialToggled(bool on)
        {
            PlayerPrefs.SetInt("EnableTutorials", on ? 1 : 0);
        }

        /*private void OnLanguageChanged(int idx)
        {
            PlayerPrefs.SetInt("LanguageIndex", idx);
            LocalizationManager.SetLanguage(idx);
        }*/

        private void OnMasterVolumeChanged(float vol)
        {
            CoreManager.Instance.soundManager.SetMasterVolume(vol);
        }

        private void OnBGMVolumeChanged(float vol)
        {
            CoreManager.Instance.soundManager.SetBGMVolume(vol);
        }

        private void OnSFXVolumeChanged(float vol)
        {
            CoreManager.Instance.soundManager.SetSFXVolume(vol);
        }

        /*private void OnLookSensitivityChanged(float sens)
        {
            CoreManager.Instance.gameManager.LookSensitivity = sens;
            PlayerPrefs.SetFloat("LookSensitivity", sens);
        }*/

        /*private void OnAimSensitivityChanged(float sens)
        {
            CoreManager.Instance.gameManager.AimSensitivity = sens;
            PlayerPrefs.SetFloat("AimSensitivity", sens);
        }*/

        private void OnResolutionChanged(int idx)
        {
            var r = resolutions[idx];
            Screen.SetResolution(r.width, r.height, Screen.fullScreen);
            PlayerPrefs.SetInt("ResolutionIndex", idx);
        }

        private void OnFullscreenModeChanged(int idx)
        {
            switch (idx)
            {
                case 0:
                    Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                    Screen.fullScreen = true;
                    break;
                case 1:
                    Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                    Screen.fullScreen = true;
                    break;
                case 2:
                    Screen.fullScreenMode = FullScreenMode.Windowed;
                    Screen.fullScreen = false;
                    break;
            }
            PlayerPrefs.SetInt("FullscreenMode", idx);
        }
    }
}
