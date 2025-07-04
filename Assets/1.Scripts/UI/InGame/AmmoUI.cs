using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Subs;
using UnityEngine;

namespace _1.Scripts.UI.InGame
{
    public class AmmoUI : MonoBehaviour
    {
        [Header("탄약 UI")] [SerializeField] private Transform ammoContainer;
        [SerializeField] private GameObject ammoPrefab;
        [SerializeField] private int maxAmmo = 10;

        private List<GameObject> ammoIcons = new List<GameObject>();
        private ObjectPoolManager poolManager;

        private void Start()
        {
            poolManager = CoreManager.Instance.objectPoolManager;
            poolManager.CreatePool(ammoPrefab, maxAmmo, maxAmmo);
            UpdateAmmoUI(maxAmmo);
        }

        public void UpdateAmmoUI(int currentAmmo)
        {
            currentAmmo = Mathf.Clamp(currentAmmo, 0, maxAmmo);

            while (ammoIcons.Count < currentAmmo)
            {
                GameObject ammoIcon = poolManager.Get(ammoPrefab.name);
                ammoIcon.transform.SetParent(ammoContainer, false);
                ammoIcons.Add(ammoIcon);
            }

            while (ammoIcons.Count > currentAmmo)
            {
                GameObject ammoIcon = ammoIcons[ammoIcons.Count - 1];
                ammoIcons.RemoveAt(ammoIcons.Count - 1);
                poolManager.Release(ammoIcon);
            }
        }
    }
}