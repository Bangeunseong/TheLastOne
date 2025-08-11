using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Michsky.UI.Shift;
using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Subs;
using TMPro;
using UnityEngine.Localization.Settings;

namespace _1.Scripts.UI.Setting
{
    public class SettingUI : MonoBehaviour
    {
        [Header("Language")]
        [SerializeField] private HorizontalSelector languageSelector;

        [Header("Audio Volume")]
        [SerializeField] private SliderManager masterVolumeSlider;
        [SerializeField] private SliderManager bgmVolumeSlider;
        [SerializeField] private SliderManager sfxVolumeSlider;

        [Header("Sensitivity")]
        [SerializeField] private SliderManager lookSensitivitySlider;
        [SerializeField] private SliderManager aimSensitivitySlider;
        
        [Header("Graphics")]
        [SerializeField] private TMP_Dropdown resolutionDropdown;
        [SerializeField] private HorizontalSelector fullscreenModeSelector;

        private List<Vector2Int> resolutionSizes;
        private readonly List<string> fullscreenModes = new List<string> { "Fullscreen", "Borderless", "Windowed" };
        
        private void Start()
        {
            InitSensitivitySliders();
            InitResolutionSelector();
            InitFullscreenModeSelector();
            LoadSettings();
            AddListeners();
        }
        
        private void OnEnable()
        {
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
            RegisterLanguageSelectorEvents();
            SyncLanguageSelector();
        }
        
        private void OnDisable()
        {
            LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
        }

        private void OnLocaleChanged(UnityEngine.Localization.Locale newLocale)
        {
            SyncLanguageSelector();
        }
        
        private void RegisterLanguageSelectorEvents()
        {
            if (!languageSelector || languageSelector.itemList == null) return;

            foreach (var item in languageSelector.itemList)
                item.onValueChanged.RemoveAllListeners();

            for (int i = 0; i < languageSelector.itemList.Count; i++)
            {
                int idx = i;
                languageSelector.itemList[i].onValueChanged.AddListener(() => OnLanguageSelectorChanged(idx));
            }
        }
        
        private void SyncLanguageSelector()
        {
            var locales = LocalizationSettings.AvailableLocales.Locales;
            var currentLocale = LocalizationSettings.SelectedLocale;
            int idx = locales.IndexOf(currentLocale);
            if (idx < 0) idx = 0;
            languageSelector.index = idx;
            languageSelector.UpdateUI();
        }
        
        private void OnLanguageSelectorChanged(int idx)
        {
            var locales = LocalizationSettings.AvailableLocales.Locales;
            if (idx < 0 || idx >= locales.Count) return;
            LocalizationSettings.SelectedLocale = locales[idx];
            PlayerPrefs.SetInt("LanguageIndex", idx);
        }
        
        private void InitSensitivitySliders()
        {
            lookSensitivitySlider.enableSaving = false;
            aimSensitivitySlider.enableSaving  = false;
        }

        private void InitResolutionSelector()
        {
            if (!resolutionDropdown) return;

            resolutionSizes = Screen.resolutions
                .Select(r => new Vector2Int(r.width, r.height))
                .Distinct()
                .OrderBy(v => v.x)
                .ThenBy(v => v.y)
                .ToList();

            resolutionDropdown.ClearOptions();
            var opts = resolutionSizes
                .Select(size => new TMP_Dropdown.OptionData($"{size.x} x {size.y}"))
                .ToList();
            resolutionDropdown.AddOptions(opts);

            int idx = GetCurrentResolutionIndex();
            resolutionDropdown.SetValueWithoutNotify(idx);
            resolutionDropdown.RefreshShownValue();

            resolutionDropdown.onValueChanged.RemoveAllListeners();
            resolutionDropdown.onValueChanged.AddListener(OnResolutionDropdownChanged);
            
            OnResolutionDropdownChanged(idx);
        }

        private void InitFullscreenModeSelector()
        {
            if (!fullscreenModeSelector) return;
            fullscreenModeSelector.saveValue = false;
            fullscreenModeSelector.loopSelection = false;
            fullscreenModeSelector.itemList.Clear();

            foreach (var t in fullscreenModes)
            {
                fullscreenModeSelector.CreateNewItem(t);
            }

            for (int i = 0; i < fullscreenModes.Count; i++)
            {
                int idx = i;
                fullscreenModeSelector.itemList[i].onValueChanged.AddListener(() => OnFullscreenModeChanged(idx));
            }

            int defaultMode = PlayerPrefs.GetInt("FullscreenMode",
                Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen ? 0 :
                Screen.fullScreenMode == FullScreenMode.FullScreenWindow   ? 1 : 2);
            fullscreenModeSelector.index = defaultMode;
            fullscreenModeSelector.UpdateUI();
        }

        private int GetCurrentResolutionIndex()
        {
            var cur = Screen.currentResolution;
            int byCurrent = resolutionSizes.FindIndex(v => v.x == cur.width && v.y == cur.height);

            int saved = PlayerPrefs.GetInt("ResolutionIndex", byCurrent >= 0 ? byCurrent : 0);
            return Mathf.Clamp(saved, 0, resolutionSizes.Count - 1);
        }

        private void OnResolutionDropdownChanged(int idx)
        {
            if (idx < 0 || idx >= resolutionSizes.Count) return;
            var size = resolutionSizes[idx];

            Screen.SetResolution(size.x, size.y, Screen.fullScreenMode);

            PlayerPrefs.SetInt("ResolutionIndex", idx);
            PlayerPrefs.Save();
        }
        

        private void LoadSettings()
        {
            var sm = CoreManager.Instance.soundManager;

            float master01 = PlayerPrefs.GetFloat("MasterVolume", 1f);
            float bgm01    = PlayerPrefs.GetFloat("BGMVolume",    1f);
            float sfx01    = PlayerPrefs.GetFloat("SFXVolume",    1f);

            masterVolumeSlider.GetComponent<Slider>().SetValueWithoutNotify(master01 * 100f);
            bgmVolumeSlider   .GetComponent<Slider>().SetValueWithoutNotify(bgm01    * 100f);
            sfxVolumeSlider   .GetComponent<Slider>().SetValueWithoutNotify(sfx01    * 100f);

            sm.SetMasterVolume(master01);
            sm.SetBGMVolume(bgm01);
            sm.SetSFXVolume(sfx01);
        }

        private void AddListeners()
        {
            masterVolumeSlider.GetComponent<Slider>().onValueChanged.AddListener(v100 =>
                CoreManager.Instance.soundManager.SetMasterVolume(Mathf.Clamp01(v100 / 100f)));

            bgmVolumeSlider.GetComponent<Slider>().onValueChanged.AddListener(v100 =>
                CoreManager.Instance.soundManager.SetBGMVolume(Mathf.Clamp01(v100 / 100f)));

            sfxVolumeSlider.GetComponent<Slider>().onValueChanged.AddListener(v100 =>
                CoreManager.Instance.soundManager.SetSFXVolume(Mathf.Clamp01(v100 / 100f)));

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
