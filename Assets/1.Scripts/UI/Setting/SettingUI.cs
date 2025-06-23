using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using _1.Scripts.UI;
using _1.Scripts.Manager.Subs;

namespace _1.Scripts.UI.Setting
{
    public class SettingUI : UIPopup
    {
        [Header("Tap Button")]
        [SerializeField] private Button graphicTabButton;
        [SerializeField] private Button soundTabButton;
        [SerializeField] private Button controlTabButton;
        
        [Header("Tab UI")]
        [SerializeField] private GameObject graphicTabPanel;
        [SerializeField] private GameObject soundTabPanel;
        [SerializeField] private GameObject controlTabPanel;
        
        [Header("그래픽 설정")]
        [SerializeField] private TMP_Dropdown resolutionDropdown;
        [SerializeField] private Toggle fullscreenToggle;
        
        [Header("사운드 설정")]
        [SerializeField] private Slider masterVolumeSlider;
        
        [Header("조작 설정")]
        [SerializeField] private Slider mouseSensitivitySlider;
        //키 설정
        
        [SerializeField] private Button closeButton;
        
        private List<Resolution> resolutions;

        public override void Init(UIManager manager)
        {
            base.Init(manager);
            
            InitResolutions();
            LoadSettings();
            AddListeners();
            
            OnTabClicked(graphicTabPanel);
        }

        private void AddListeners()
        {
            graphicTabButton.onClick.AddListener(() => OnTabClicked(graphicTabPanel));
            soundTabButton.onClick.AddListener(() => OnTabClicked(soundTabPanel));
            controlTabButton.onClick.AddListener(() => OnTabClicked(controlTabPanel));
            
            closeButton.onClick.AddListener(ClosePopup);
            
            fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
            resolutionDropdown.onValueChanged.AddListener(SetResolution);
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
            mouseSensitivitySlider.onValueChanged.AddListener(SetMouseSensitivity);
        }

        private void InitResolutions()
        {
            resolutions = Screen.resolutions.Select(res => new Resolution {width = res.width, height = res.height}).ToList();
            resolutions.Sort((a, b) => a.width.CompareTo(b.width));
            resolutionDropdown.ClearOptions();
            resolutionDropdown.AddOptions(resolutions.Select(res => $"{res.width} x {res.height}").ToList());
            resolutionDropdown.value = 0;
            resolutionDropdown.RefreshShownValue();
        }

        private void LoadSettings()
        {
            fullscreenToggle.isOn = PlayerPrefs.GetInt("IsFullscreen", 1) == 1;
            resolutionDropdown.value = PlayerPrefs.GetInt("ResolutionIndex", 0);
            masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1);
            mouseSensitivitySlider.value = PlayerPrefs.GetFloat("MouseSensitivity", 1);
        }

        private void OnTabClicked(GameObject activePanel)
        {
            graphicTabPanel.SetActive(activePanel == graphicTabPanel);
            soundTabPanel.SetActive(activePanel == soundTabPanel);
            controlTabPanel.SetActive(activePanel == controlTabPanel);
        }
        
        private void SetFullscreen(bool isFullscreen)
        {
            Screen.fullScreen = isFullscreen;
            PlayerPrefs.SetInt("IsFullscreen", isFullscreen ? 1 : 0);
        }

        private void SetResolution(int resolutionIndex)
        {
            Resolution resolution = resolutions[resolutionIndex];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
            PlayerPrefs.SetInt("ResolutionIndex", resolutionIndex);
        }

        private void SetMasterVolume(float volume)
        {
            // SoundManager.Instance.SetMasterVolume(volume);
            PlayerPrefs.SetFloat("MasterVolume", volume);
        }

        private void SetMouseSensitivity(float sensitivity)
        {
            PlayerPrefs.SetFloat("MouseSensitivity", sensitivity);
        }
    }
}