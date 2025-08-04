using System;
using _1.Scripts.Manager.Subs;
using _1.Scripts.UI.InGame.Dialogue;
using UnityEngine;

namespace _1.Scripts.Dialogue
{
    [Serializable]
    public struct DialogueData
    {
        public string Speaker;
        public string Message;
        public SpeakerType SpeakerType;
        public SfxType sfxType;
        public int sfxIndex;
    }
}