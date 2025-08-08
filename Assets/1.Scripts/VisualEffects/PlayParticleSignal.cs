using System;
using System.Collections.Generic;
using UnityEngine;

namespace _1.Scripts.VisualEffects
{
    public class PlayParticleSignal : MonoBehaviour
    {
        [field: Header("Components")]
        [field: SerializeField] public List<ParticleSystem> ParticleSystems { get; private set; } = new();

        public void Play()
        {
            foreach (var val in ParticleSystems) { val.Play(); }
        }

        public void Stop()
        {
            foreach (var val in ParticleSystems) { val.Stop(); }
        }
    }
}
