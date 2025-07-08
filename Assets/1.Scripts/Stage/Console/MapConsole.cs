using System;
using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.Player.Core;
using _1.Scripts.Interfaces.Player;
using UnityEngine;

namespace _1.Scripts.Map.Console
{
    public class MapConsole : MonoBehaviour, IInteractable
    {
        public event Action OnInteractEvent;
        public void OnInteract(GameObject ownerObj)
        {
            if (!ownerObj.TryGetComponent(out Player player))
                return;
            Service.Log("Console");
            
            //TODO: 미니게임 시작
        }
    }
}