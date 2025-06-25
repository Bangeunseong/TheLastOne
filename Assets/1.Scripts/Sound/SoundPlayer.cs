using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Manager.Core;
using UnityEngine;

namespace _1.Scripts.Sound
{
    public class SoundPlayer : MonoBehaviour
    {
        private AudioSource audioSource;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        public void Play(AudioClip clip, float volume = 1.0f, float spatialBlend = 0)
        {
            audioSource.clip = clip;
            audioSource.volume = volume;
            audioSource.spatialBlend = spatialBlend;
            audioSource.Play();
            
            StartCoroutine(ReturnToPool());
        }

        public void Play2D(AudioClip clip, float volume)
        {
            transform.position = Vector3.zero;
            Play(clip, volume, 0.0f);
        }

        public void Play3D(AudioClip clip, float volume, Vector3 position)
        {
            transform.position = position;
            Play(clip, volume, 1.0f);
        }

        private IEnumerator ReturnToPool()
        {
            yield return new WaitWhile(() => audioSource.isPlaying);
            CoreManager.Instance.objectPoolManager.Release(gameObject);
        }
    }
}
