using System;
using _1.Scripts.Entity.Scripts.Player.Core;
using _1.Scripts.Interfaces.Player;
using _1.Scripts.MiniGame;
using UnityEngine;
using UnityEngine.Events;

namespace _1.Scripts.Map.Console
{
    public class Console : MonoBehaviour, IInteractable
    {
        [field: Header("Console Settings")]
        [field: SerializeField] public int Id { get; private set; }
        [field: SerializeField] public bool IsCleared { get; private set; }
        
        [field: Header("Minigames")]
        [field: SerializeField] public AlphabetMatching AlphabetGame { get; private set; }

        private void Awake()
        {
            if (!AlphabetGame) AlphabetGame = this.TryGetComponent<AlphabetMatching>();
        }

        private void Reset()
        {
            if (!AlphabetGame) AlphabetGame = this.TryGetComponent<AlphabetMatching>();
        }

        private void Start()
        {
            AlphabetGame.Initialize(this);
        }

        public void OnCleared()
        {
            IsCleared = true;
            // TODO: Save cleared info. to DTO
        }

        public void OnInteract(GameObject ownerObj)
        {
            if (!ownerObj.TryGetComponent(out Player player)) return;
            if (IsCleared) return;
            AlphabetGame.enabled = true;
        }
    }
}