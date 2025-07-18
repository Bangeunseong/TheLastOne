using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _1.Scripts.UI.Common
{
    public class MenuHandler : MonoBehaviour
    {
        private InventoryHandler inventoryHandler;
        private PauseHandler pauseHandler;
        
        public void SetInventoryHandler(InventoryHandler handler) => inventoryHandler = handler;
        public void SetPauseHandler(PauseHandler handler) => pauseHandler = handler;
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab)) inventoryHandler?.ToggleInventory();
            if (Input.GetKeyDown(KeyCode.Escape)) pauseHandler?.TogglePause();
        }
    }
}