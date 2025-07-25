    using System.Collections.Generic;
    using UnityEngine;

    namespace _1.Scripts.Static
    {
        public static class LayerConstants
        {
            public static readonly int Wall = LayerMask.NameToLayer("Wall");
            public static readonly int Ground = LayerMask.NameToLayer("Ground");
            public static readonly int Interactable = LayerMask.NameToLayer("Interactable");
            
            public static readonly int Ally = LayerMask.NameToLayer("Ally");
            public static readonly int Enemy = LayerMask.NameToLayer("Enemy");
            public static readonly int StencilAlly = LayerMask.NameToLayer("Stencil_Ally");
            public static readonly int StencilEnemy = LayerMask.NameToLayer("Stencil_E");
            
            // 부위별 데미지 목록
            public static readonly int Head_P = LayerMask.NameToLayer("Head_P");
            public static readonly int Chest_P = LayerMask.NameToLayer("Chest_P");
            public static readonly int Belly_P = LayerMask.NameToLayer("Belly_P");
            public static readonly int Arm_P = LayerMask.NameToLayer("Arm_P");
            public static readonly int Leg_P = LayerMask.NameToLayer("Leg_P");
            
            public static readonly int Head_E = LayerMask.NameToLayer("Head_E");
            public static readonly int Chest_E = LayerMask.NameToLayer("Chest_E");
            public static readonly int Belly_E = LayerMask.NameToLayer("Belly_E");
            public static readonly int Arm_E = LayerMask.NameToLayer("Arm_E");
            public static readonly int Leg_E = LayerMask.NameToLayer("Leg_E");
            
            public static readonly HashSet<int> DefaultHittableLayers = new()
            {
                Wall, Ground, Interactable
            };
            
            public static readonly HashSet<int> AllyLayers = new()
            {
                Head_P, Chest_P, Belly_P, Arm_P, Leg_P
            };

            public static readonly HashSet<int> EnemyLayers = new()
            {
                Head_E, Chest_E, Belly_E, Arm_E, Leg_E
            };
            
            public static readonly HashSet<int> IgnoreLayersForStencil = new()
            {
                Head_P, Chest_P, Belly_P, Arm_P, Leg_P,
                Head_E, Chest_E, Belly_E, Arm_E, Leg_E, 
            };

            /// <summary>
            /// 레이어 HashSet들 레이어마스크로 변환
            /// </summary>
            /// <param name="layers"></param>
            /// <returns></returns>
            public static int ToLayerMask(HashSet<int> layers)
            {
                int mask = 0;
                foreach (int layer in layers)
                {
                    mask |= (1 << layer);
                }
                return mask;
            }
        }
    }

