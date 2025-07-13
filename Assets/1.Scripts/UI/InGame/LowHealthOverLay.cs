using System.Collections;
using _1.Scripts.Entity.Scripts.Player.Core;
using UnityEngine;
using UnityEngine.UI;

namespace _1.Scripts.UI.InGame
{
    public class LowHealthOverLay : MonoBehaviour
    {
        [SerializeField] private Image topGradient;
        [SerializeField] private Image bottomGradient;
        [SerializeField] private Image leftGradient;
        [SerializeField] private Image rightGradient;
        
        private PlayerCondition playerCondition;
        
        [Range(0,1)] public float threshold1 = 0.75f;
        [Range(0,1)] public float threshold2 = 0.5f;
        [Range(0,1)] public float threshold3 = 0.25f;
        
        public Color color0 = new Color(1f, 0.5f, 0.5f, 0f);
        public Color color1 = new Color(1f, 0.4f, 0.4f, 0.02f);
        public Color color2 = new Color(0.9f, 0.4f, 0.4f, 0.1f);
        public Color color3 = new Color(0.5f, 0.1f, 0.1f, 0.4f);

        private Color lastColor = Color.clear;

        [SerializeField] private float flashStrength = 0.05f;
        public float flashDuration = 0.5f;
        private float flashAlpha;
        private Coroutine flashCoroutine;

        private void Start()
        {
            if (playerCondition == null)
            {
                playerCondition = FindObjectOfType<PlayerCondition>();
            }

            playerCondition.OnDamage += HandleHit;
            
            SetOverlayColer(Color.clear);
        }
        void OnDestroy()
        {
            if (playerCondition != null) playerCondition.OnDamage -= HandleHit;
        }
        private void Update()
        {
            float ratio = playerCondition.CurrentHealth / (float)playerCondition.MaxHealth;
            Color tint;
            
            if (ratio <= threshold3) tint = color3;
            else if (ratio <= threshold2) tint = color2;
            else if (ratio <= threshold1) tint = color1;
            else tint = color0;
            
            float finalAlpha = Mathf.Clamp01(tint.a + flashAlpha);
            tint.a = finalAlpha;

            if (tint != lastColor)
            {
                SetOverlayColer(tint);
                lastColor = tint;
            }
        }

        private void SetOverlayColer(Color color)
        {
            topGradient.color = color;
            bottomGradient.color = color;
            leftGradient.color = color;
            rightGradient.color = color;
        }
        
        private void HandleHit()
        {
            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(FlashCoroutine());
        }
        private IEnumerator FlashCoroutine()
        {
            float elapsed = 0f;
            flashAlpha = flashStrength;

            while (elapsed < flashDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                flashAlpha = Mathf.Lerp(flashStrength, 0f, elapsed / flashDuration);
                yield return null;
            }

            flashAlpha = 0f;
            flashCoroutine = null;
        }
    }
}