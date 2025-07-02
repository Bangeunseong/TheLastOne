using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _1.Scripts.UI.InGame
{
    public class WeaponUI : MonoBehaviour
    {
        [Header("무기 정보")] [SerializeField] private TextMeshProUGUI weaponNameText;
        [SerializeField] private TextMeshProUGUI ammoText;
        [SerializeField] private Image weaponImage;
        [SerializeField] private Image ammoImage;
    }
}