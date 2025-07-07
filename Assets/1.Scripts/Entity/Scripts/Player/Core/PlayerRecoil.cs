using System;
using _1.Scripts.Manager.Core;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _1.Scripts.Entity.Scripts.Player.Core
{
    public class PlayerRecoil : MonoBehaviour
    {
        [Header("Recoil Pivot")] 
        [SerializeField] private float recoilReturnSpeed = 5f;
        [SerializeField] private float recoilSnappiness = 10f;
        
        // Fields
        private CoreManager coreManager;
        private Vector3 targetRotation;
        private Vector3 velocity;
        
        // Properties
        public Vector3 CurrentRotation { get; private set; }

        private void Start()
        {
            coreManager = CoreManager.Instance;
        }

        private void Update()
        {
            // 반동 점점 줄어들게
            targetRotation = Vector3.SmoothDamp(targetRotation, Vector3.zero, ref velocity, 1f / recoilReturnSpeed);
            if (!coreManager.gameManager.IsGamePaused)
                CurrentRotation = Vector3.Lerp(CurrentRotation, targetRotation, recoilSnappiness * Time.unscaledDeltaTime);
        }

        public void ApplyRecoil(float recoilX = -2f, float recoilY = 2f)
        {
            targetRotation += new Vector3(recoilX, Random.Range(-recoilY, recoilY), 0f);
        }
    }
}