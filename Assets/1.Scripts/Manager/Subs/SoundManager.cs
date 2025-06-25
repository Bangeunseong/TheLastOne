using System;
using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Manager.Core;
using _1.Scripts.Sound;
using UnityEngine;

namespace _1.Scripts.Manager.Subs
{
    [Serializable]
    public class SoundManager
    {
        private AudioSource bgmSource;
        private ResourceManager resourceManager;
        private ObjectPoolManager poolManager;
        
        private float masterVolume = 1.0f;
        private float bgmVolume = 1.0f;
        private float sfxVolume = 1.0f;
        
        public float MasterVolume => masterVolume;
        public float BgmVolume => bgmVolume;
        public float SfxVolume => sfxVolume;
        
        private Dictionary<string, AudioClip> audioClips = new Dictionary<string, AudioClip>();

        public void Start()
        {
            resourceManager = CoreManager.Instance.resourceManager;
            poolManager = CoreManager.Instance.objectPoolManager;
            
            GameObject smObject = CoreManager.Instance.gameObject;
            bgmSource = smObject.AddComponent<AudioSource>();
            bgmSource.playOnAwake = false;
            bgmSource.loop = true;
            
            LoadVolumeSettings(); 
        }
        
        public void PlayBGM(string clipName)
        {
            AudioClip clip = resourceManager.GetAsset<AudioClip>(clipName);
            if (clip == null)
            {
                Debug.LogWarning($"BGM AudioClip을 찾을 수 없습니다: {clipName}");
                return;
            }

            if (bgmSource.clip == clip && bgmSource.isPlaying)
            {
                return;
            }

            bgmSource.clip = clip;
            bgmSource.Play();
        }
        
        public void PlayUISFX(string clipName)
        {
            AudioClip clip = resourceManager.GetAsset<AudioClip>(clipName);
            if (clip == null)
            {
                Debug.LogWarning($"SFX AudioClip을 찾을 수 없습니다: {clipName}");
                return;
            }
            
            GameObject soundPlayerObj = poolManager.Get("SoundPlayer");
            if (soundPlayerObj != null)
            {
                SoundPlayer soundPlayer = soundPlayerObj.GetComponent<SoundPlayer>();
                soundPlayer.Play2D(clip, masterVolume * sfxVolume);
            }
        }
        
        public void PlaySFX(string clipName, Vector3 position)
        {
            AudioClip clip = resourceManager.GetAsset<AudioClip>(clipName);
            if (clip == null)
            {
                Debug.LogWarning($"SFX AudioClip을 찾을 수 없습니다: {clipName}");
                return;
            }

            GameObject soundPlayerObj = poolManager.Get("SoundPlayer");
            if (soundPlayerObj != null)
            {
                SoundPlayer soundPlayer = soundPlayerObj.GetComponent<SoundPlayer>();
                soundPlayer.Play3D(clip, masterVolume * sfxVolume, position);
            }
        }
        
        public void StopBGM()
        {
            bgmSource.Stop();
        }

        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            AudioListener.volume = masterVolume;
            SetBGMVolume(bgmVolume);
            PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        }
        
        public void SetBGMVolume(float volume)
        {
            bgmVolume = Mathf.Clamp01(volume);
            bgmSource.volume = masterVolume * bgmVolume;
            PlayerPrefs.SetFloat("BGMVolume", bgmVolume);
        }

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        }

        private void LoadVolumeSettings()
        {
            masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1.0f);
            bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 1.0f);
            sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1.0f);
            
            AudioListener.volume = masterVolume;
            bgmSource.volume = masterVolume * bgmVolume;
        }
    }
}
