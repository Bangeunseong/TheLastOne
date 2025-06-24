using _1.Scripts.Interfaces;
using UnityEngine;

namespace _1.Scripts.Weapon.Scripts
{
    public class DummyGun : MonoBehaviour, IInteractable
    {
        public void OnInteract(GameObject ownerObj)
        {
            throw new System.NotImplementedException();
        }
    }
}