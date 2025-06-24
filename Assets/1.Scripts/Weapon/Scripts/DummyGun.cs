using _1.Scripts.Interfaces;
using UnityEngine;

namespace _1.Scripts.Weapon.Scripts
{
    public class DummyGun : MonoBehaviour, IInteractable
    {
        [field: Header("DummyGun Settings")]
        [field: SerializeField] public GunType Type { get; private set; }
        
        public void OnInteract(GameObject ownerObj)
        {
            
        }
    }
}