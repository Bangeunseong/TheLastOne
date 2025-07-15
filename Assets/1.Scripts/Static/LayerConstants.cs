using UnityEngine;

namespace _1.Scripts.Static
{
    public static class LayerConstants
    {
        public static readonly int Ally = LayerMask.NameToLayer("Ally");
        public static readonly int Enemy = LayerMask.NameToLayer("Enemy");
        public static readonly int Chest = LayerMask.NameToLayer("Chest_P");
        public static readonly int StencilAlly = LayerMask.NameToLayer("Stencil_Ally");
        public static readonly int StencilEnemy = LayerMask.NameToLayer("Stencil_E");
    }
}

