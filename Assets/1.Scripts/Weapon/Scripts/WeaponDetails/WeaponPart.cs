using _1.Scripts.Interfaces.Weapon;
using _1.Scripts.Weapon.Scripts.Common;
using _1.Scripts.Weapon.Scripts.Guns;
using _1.Scripts.Weapon.Scripts.Hack;
using UnityEngine;

namespace _1.Scripts.Weapon.Scripts.WeaponDetails
{
    public class WeaponPart : MonoBehaviour, IWearable
    {
        [field: Header("WeaponPart Data")]
        [field: SerializeField] public WeaponPartData Data { get; private set; }
        [field: SerializeField] public bool IsWorn { get; private set; }
        
        [field: Header("Components")]
        [field: SerializeField] public BaseWeapon ParentWeapon { get; private set; }
        [field: SerializeField] public Transform Parent { get; private set; }
        [field: SerializeField] public Transform IronSight_A { get; private set; }
        [field: SerializeField] public Transform IronSight_B { get; private set; }
        
        private void Awake()
        {
            if (!ParentWeapon) ParentWeapon = GetComponentInParent<BaseWeapon>(true);
            if (!Parent) Parent = this.TryGetComponent<Transform>();
        }

        private void Reset()
        {
            if (!ParentWeapon) ParentWeapon = GetComponentInParent<BaseWeapon>(true);
            if (!Parent) Parent = this.TryGetComponent<Transform>();
        }

        public void OnWear()
        {
            if (IsWorn) return;
            if (Data.Type == PartType.IronSight)
            {
                IronSight_A.localRotation = Quaternion.Euler(IronSight_A.localRotation.x, IronSight_A.localRotation.y, IronSight_A.localRotation.z + 90f);
                IronSight_B.localRotation = Quaternion.Euler(IronSight_B.localRotation.x, IronSight_B.localRotation.y, IronSight_B.localRotation.z - 90f);
            }
            else Parent.gameObject.SetActive(true);

            switch (ParentWeapon)
            {
                case Gun gun: gun.UpdateStatValues(Data); break;
                case HackGun hackGun: hackGun.UpdateStatValues(Data); break;
            }
            IsWorn = true;
        }

        public void OnUnWear()
        {
            if (!IsWorn) return;
            if (Data.Type == PartType.IronSight)
            {
                IronSight_A.localRotation = Quaternion.Euler(IronSight_A.localRotation.x, IronSight_A.localRotation.y, IronSight_A.localRotation.z - 90f);
                IronSight_B.localRotation = Quaternion.Euler(IronSight_B.localRotation.x, IronSight_B.localRotation.y, IronSight_B.localRotation.z + 90f);
            }
            else
            {
                Parent.gameObject.SetActive(false);
            }
            
            switch (ParentWeapon)
            {
                case Gun gun: gun.UpdateStatValues(Data, false); break;
                case HackGun hackGun: hackGun.UpdateStatValues(Data, false); break;
            }
            IsWorn = false;
        }
    }
}