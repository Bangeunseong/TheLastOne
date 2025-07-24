using System.Collections;
using System.Collections.Generic;
using _1.Scripts.UI.InGame;
using _1.Scripts.UI.InGame.HUD;
using _1.Scripts.Weapon.Scripts.Common;
using UnityEngine;

namespace _1.Scripts.UI.Inventory
{
    public class PreviewWeaponHandler : MonoBehaviour
    {
        
        [SerializeField] private float speed = 40f;
        public SlotType slotType;
        void Update()
        {
            transform.Rotate(Vector3.up, speed * Time.unscaledDeltaTime, Space.Self);
        }
    }
}