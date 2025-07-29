using System;
using System.Collections.Generic;
using System.Linq;
using _1.Scripts.Entity.Scripts.Player.Core;
using _1.Scripts.Interfaces.Player;
using _1.Scripts.Manager.Core;
using _1.Scripts.Weapon.Scripts.Common;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _1.Scripts.Map.PartBox
{
    public enum BoxType
    {
        Ammo,
        Item,
        Parts,
    }
    
    public class PartBox : MonoBehaviour, IInteractable
    {
        [field: Header("Components")]
        [field: SerializeField] public Animator Animator { get; private set; }
        
        [field: Header("References")]
        [field: SerializeField] public int Id { get; private set; }
        [field: SerializeField] public SerializedDictionary<BoxType, int> GenSettings { get; private set; } = new();
        
        private CoreManager coreManager;
        private Player player;
        private bool isAlreadyOpened;

        private void Awake()
        {
            if (!Animator) Animator = this.TryGetComponent<Animator>();
        }

        private void Reset()
        {
            if (!Animator) Animator = this.TryGetComponent<Animator>();
        }

        private void Start()
        {
            coreManager = CoreManager.Instance;
            player = coreManager.gameManager.Player;
            
            // TODO: Check Stage info. that this box is already opened
        }

        public void OnInteract(GameObject ownerObj)
        {
            int partId = CheckMissingWeaponPart();
            if (!ownerObj.TryGetComponent(out Player p) || isAlreadyOpened) return;
            
            isAlreadyOpened = true;

            foreach (var gen in GenSettings.Where(gen => gen.Value > 0))
            {
                switch (gen.Key)
                {
                    case BoxType.Ammo: 
                        
                        break;
                    case BoxType.Item: 
                        
                        break;
                    case BoxType.Parts:
                        if (p.PlayerWeapon.Weapons.Any(weapon => weapon.Value.TryCollectWeaponPart(partId))) 
                            coreManager.SaveData_QueuedAsync(); 
                        break;
                }
            }
        }

        public void OnCancelInteract() { }

        private int CheckMissingWeaponPart()
        {
            var missingPartIds = new List<int>();
            foreach(var weapon in player.PlayerWeapon.Weapons)
                missingPartIds.AddRange(weapon.Value.EquipableWeaponParts.Where(val => !val.Value).Select(val => val.Key));

            if (missingPartIds.Count <= 0) return -1;
            return missingPartIds[Random.Range(0, missingPartIds.Count)];
        }
    }
}
