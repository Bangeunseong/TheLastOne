using UnityEngine;

namespace _1.Scripts.Interfaces.Player
{
    public interface IInteractable
    {
        void OnInteract(GameObject ownerObj);
        void OnCancelInteract();
    }
}