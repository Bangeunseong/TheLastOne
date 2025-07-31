using System.Collections;
using System.Collections.Generic;
using _1.Scripts.UI.InGame;
using _1.Scripts.UI.InGame.HUD;
using _1.Scripts.Weapon.Scripts.Common;
using _1.Scripts.Weapon.Scripts.WeaponDetails;
using UnityEngine;

namespace _1.Scripts.UI.Inventory
{
    [System.Serializable]
    public class PartRendererInfo
    {
        public PartType partType;
        public MeshRenderer renderer;
    }
    public class PreviewWeaponHandler : MonoBehaviour
    {
        
        [SerializeField] private float speed = 40f;
        public WeaponType weaponType;
        public List<PartRendererInfo> partRenderers = new();
        
        void Update()
        {
            transform.Rotate(Vector3.up, speed * Time.unscaledDeltaTime, Space.Self);
        }
        
        public MeshRenderer GetRendererOfPart(PartType partType)
        {
            foreach (var info in partRenderers)
            {
                if (info.partType == partType && info.renderer != null)
                    return info.renderer;
            }
            return null;
        }
    }
}