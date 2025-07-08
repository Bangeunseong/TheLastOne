using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Interfaces.Player;
using UnityEngine;

namespace _1.Scripts.Map.Console
{
    public class Console : MonoBehaviour, IInteractable
    {
        void IInteractable.OnInteract(GameObject ownerObj)
        {
            Service.Log("Console Start");
            //TODO: MiniGame Start
        }
    }
}