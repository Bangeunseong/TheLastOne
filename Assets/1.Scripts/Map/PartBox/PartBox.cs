using System;
using System.Collections.Generic;
using System.Linq;
using _1.Scripts.Entity.Scripts.Player.Core;
using _1.Scripts.Interfaces.Player;
using _1.Scripts.Item.Common;
using _1.Scripts.Item.Items;
using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Data;
using _1.Scripts.Quests.Core;
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
    
    public class PartBox : MonoBehaviour, IInteractable, IGameEventListener
    {
        public const int BaseItemBoxIndex = 1200;
        
        [field: Header("Components")]
        [field: SerializeField] public Animator Animator { get; private set; }
        [field: SerializeField] public ParticleSystem ParticleSystem { get; private set; }
        [field: SerializeField] public List<Transform> SpawnPoints { get; private set; } = new();
        
        [field: Header("References")]
        [field: Tooltip("Target Id is for the Quest")]
        [field: SerializeField] public int TargetId { get; private set; }
        [field: Tooltip("Instance Id is for the check if is opened last time.")]
        [field: SerializeField] public int InstanceId { get; private set; }
        [field: Tooltip("Changing Value greater than 0 will spawn corresponding prop.")]
        [field: SerializeField] public SerializedDictionary<BoxType, int> GenSettings { get; private set; } = new();
        
        private CoreManager coreManager;
        private Player player;
        private bool isAlreadyOpened;
        
        private static readonly int Open = Animator.StringToHash("Open");

        private void Awake()
        {
            if (!Animator) Animator = this.TryGetComponent<Animator>();
            if (!ParticleSystem) ParticleSystem = this.TryGetChildComponent<ParticleSystem>();
            if (SpawnPoints.Count <= 0)
            {
                SpawnPoints.AddRange(this.TryGetChildComponents<Transform>("SpawnPoints"));
                SpawnPoints.RemoveAt(0);
            }
        }

        private void Reset()
        {
            if (!Animator) Animator = this.TryGetComponent<Animator>();
            if (!ParticleSystem) ParticleSystem = this.TryGetChildComponent<ParticleSystem>();
            if (SpawnPoints.Count <= 0)
            {
                SpawnPoints.AddRange(this.TryGetChildComponents<Transform>("SpawnPoints"));
                SpawnPoints.RemoveAt(0);
            }
        }

        private void Start()
        {
            coreManager = CoreManager.Instance;
            player = coreManager.gameManager.Player;
            GameEventSystem.Instance.RegisterListener(this);
            
            var save = CoreManager.Instance.gameManager.SaveData;
            if (save is not { stageInfos: not null } ||
                !save.stageInfos.TryGetValue(CoreManager.Instance.sceneLoadManager.CurrentScene, out var info)) return;
            if (!info.completionDict.TryGetValue(BaseItemBoxIndex + InstanceId, out var value) || value) return;
            
            GameEventSystem.Instance.UnregisterListener(this);
            isAlreadyOpened = true;
            gameObject.layer = LayerMask.NameToLayer("Default");
        }

        public void OnInteract(GameObject ownerObj)
        {
            if (!ownerObj.TryGetComponent(out Player p) || isAlreadyOpened) return;
            
            isAlreadyOpened = true;
            
            PlayOpeningAnimation();
            
            p.PlayerCondition.LastSavedPosition = p.transform.position;
            p.PlayerCondition.LastSavedRotation = p.transform.rotation;
            
            GameEventSystem.Instance.RaiseEvent(TargetId);
            GameEventSystem.Instance.RaiseEvent(BaseItemBoxIndex + InstanceId);
        }

        public void OnCancelInteract() { }
        
        public void OnEventRaised(int eventID)
        {
            if (eventID != BaseItemBoxIndex + InstanceId) return;
            
            var save = coreManager.gameManager.SaveData;
            if (save is { stageInfos: not null } && save.stageInfos.TryGetValue(coreManager.sceneLoadManager.CurrentScene, out var info))
                info.completionDict.TryAdd(BaseItemBoxIndex + InstanceId, true);
            
            GameEventSystem.Instance.UnregisterListener(this);
            coreManager.SaveData_QueuedAsync();
        }
        
        private void PlayOpeningAnimation()
        {
            gameObject.layer = LayerMask.NameToLayer("Default");
            ParticleSystem.Play();
            
            foreach (var gen in GenSettings.Where(gen => gen.Value > 0))
            {
                switch (gen.Key)
                {
                    case BoxType.Ammo:
                        var type = Random.Range(0f, 1f) > 0.5f ? WeaponType.GrenadeLauncher : WeaponType.HackGun;
                        var ammo = coreManager.objectPoolManager.Get(type + "_Dummy");
                        ammo.transform.SetPositionAndRotation(SpawnPoints.First().position, SpawnPoints.First().rotation);
                        if (ammo.TryGetComponent(out DummyWeapon ammoComp)) 
                            ammoComp.Initialize(false, coreManager.spawnManager.GetInstanceHashId(ammo, (int)type, ammo.transform)); 
                        coreManager.spawnManager.DynamicSpawnedWeapons.TryAdd(ammoComp.InstanceId, new SerializableWeaponProp
                        {
                            type = type, 
                            transform = new SerializableTransform
                            {
                                position = new SerializableVector3(ammo.transform.position), 
                                rotation = new SerializableQuaternion(ammo.transform.rotation)
                            }
                        });
                        break;
                    case BoxType.Item:
                        var item = coreManager.objectPoolManager.Get(ItemType.Shield + "_Prefab");
                        item.transform.SetPositionAndRotation(SpawnPoints[1].position, SpawnPoints[1].rotation);
                        if (item.TryGetComponent(out DummyItem itemComp)) 
                            itemComp.Initialize(false, coreManager.spawnManager.GetInstanceHashId(item, (int)ItemType.Shield, item.transform)); 
                        coreManager.spawnManager.DynamicSpawnedItems.TryAdd(itemComp.InstanceId, new SerializableItemProp
                        {
                            type = ItemType.Shield, 
                            transform = new SerializableTransform
                            {
                                position = new SerializableVector3(item.transform.position), 
                                rotation = new SerializableQuaternion(item.transform.rotation)
                            }
                        });
                        break;
                    case BoxType.Parts:
                        foreach (var weapon in player.PlayerWeapon.Weapons)
                            if (weapon.Value.TryCollectWeaponPart(CheckMissingWeaponPart())) break;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            
            Animator.SetTrigger(Open);
        }
        
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
