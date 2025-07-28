using UnityEngine;

namespace _1.Scripts.Weapon.Scripts.WeaponDetails
{
    public enum PartType
    {
        IronSight,
        Sight,
        FlameArrester,
        Silencer,
    }
    
    [CreateAssetMenu(fileName = "New WeaponPart Data", menuName = "ScriptableObjects/Weapon/Create New WeaponPart Data", order = 0)]
    public class WeaponPartData : ScriptableObject
    {
        [field: Header("Part Settings")]
        [field: SerializeField] public PartType Type { get; private set; }
        [field: SerializeField] public float IncreaseDistanceRate { get; private set; }
        [field: SerializeField] public float IncreaseAccuracyRate { get; private set; }
        [field: SerializeField] public float ReduceRecoilRate { get; private set; }
        [field: SerializeField] public bool IsBasicPart { get; private set; }
    }
}