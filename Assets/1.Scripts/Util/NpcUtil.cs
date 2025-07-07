using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Static;
using UnityEngine;

namespace _1.Scripts.Util
{
    /// <summary>
    /// AI관련 헬퍼 모음집
    /// </summary>
    public static class NpcUtil
    {
        /// <summary>
        /// NPC 입장에서 지정된 타겟이 시야에 있는지 검사
        /// </summary>
        /// <param name="npcPosition">NPC 위치</param>
        /// <param name="targetPosition">타겟 위치</param>
        /// <param name="maxViewDistance">최대 시야 거리</param>
        /// <param name="isAlly">자신이 아군이면 true, 적군이면 false</param>
        /// <returns>시야에 타겟이 있고, 올바른 레이어인지 여부</returns>
        public static bool IsTargetVisible(Vector3 npcPosition, Vector3 targetPosition, float maxViewDistance,
            bool isAlly)
        {
            Vector3 origin = npcPosition;
            Vector3 direction = (targetPosition - origin).normalized;

            if (Physics.Raycast(origin, direction, out RaycastHit hit, maxViewDistance))
            {
                int expectedLayer = isAlly ? LayerConstants.Enemy : LayerConstants.Ally;

                return hit.collider.gameObject.layer == expectedLayer;
            }

            return false;
        }

        /// <summary>
        /// 타겟 바라보게 설정하는 메소드
        /// </summary>
        /// <param name="self"></param>
        /// <param name="target"></param>
        /// <param name="turnSpeed"></param>
        public static void LookAtTarget(Transform self, Transform target, float turnSpeed = 15f)
        {
            if (target == null || self == null)
                return;

            Vector3 dir = target.position - self.position;
            dir.y = 0;

            if (dir == Vector3.zero) return;

            Quaternion rot = Quaternion.LookRotation(dir);
            self.rotation = Quaternion.Slerp(self.rotation, rot, Time.deltaTime * turnSpeed);
        }

        /// <summary>
        /// 자식들 전부 레이어 변환
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="layer"></param>
        public static void SetLayerRecursively(GameObject obj, int layer)
        {
            obj.layer = layer;
            foreach (Transform child in obj.transform)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }
    }
}
