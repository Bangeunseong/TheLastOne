using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace _1.Scripts.Sound
{
    [CreateAssetMenu(fileName = "New SoundGroup", menuName = "ScriptableObjects/Sound", order = 0)]
    public class SoundGroupSO : ScriptableObject
    {
        public string name;
        public List<AssetReferenceT<AudioClip>> audioClips;

        public AssetReferenceT<AudioClip> GetRandomClip()
        {
            if (audioClips.Count == 0) return null;
            int randomIndex = Random.Range(0, audioClips.Count);
            return audioClips[randomIndex];
        }
    }
}