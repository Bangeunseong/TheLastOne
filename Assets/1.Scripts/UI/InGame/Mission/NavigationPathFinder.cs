using UnityEngine;
using UnityEngine.AI;

namespace _1.Scripts.UI.InGame.Mission
{
    public class NavigationPathFinder : MonoBehaviour
    {
        private NavMeshPath navPath;

        private void Awake()
        {
            navPath = new NavMeshPath();
        }
        
        public Vector3[] GetPathCorners(Vector3 startPos, Vector3 targetPos)
        {
            navPath.ClearCorners();
            NavMesh.CalculatePath(startPos, targetPos, NavMesh.AllAreas, navPath);
            return navPath.corners;
        }
    }
}