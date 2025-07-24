using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using PlayerInput = _1.Scripts.Entity.Scripts.Player.Core.PlayerInput;

namespace _1.Scripts.UI.InGame.HUD
{
    public class CrosshairController : MonoBehaviour
    {
        [SerializeField] private GameObject crosshair;
        [SerializeField] private Image[] crosshairImage;
        [SerializeField] private RectTransform crosshairRectTransform;
        [SerializeField] private float crosshairSize = 1.2f;
        [SerializeField] private float sizeModifyDuration = 0.1f;

        private InputAction aimAction;
        private InputAction fireAction;
        private PlayerInput playerInput;
        private Vector3 originalScale;
        
        private Coroutine modifyCoroutine;
        private Coroutine shrinkCoroutine;
        
        private void OnEnable()
        {
            if (playerInput == null)
            {
                playerInput = FindObjectOfType<PlayerInput>();
                
                aimAction = playerInput.PlayerActions.Aim;
                fireAction = playerInput.PlayerActions.Fire;
            }
            aimAction.started += OnAimStarted;
            aimAction.canceled  += OnAimCanceled;
            fireAction.started += OnFirePerformed;
            fireAction.canceled += OnFireCanceled;
        }
        private void Start()
        {
            originalScale = crosshairRectTransform.localScale;
        }
        
        private void OnDisable()
        {
            if (playerInput == null) return;
            
            aimAction.started -= OnAimStarted;
            aimAction.canceled  -= OnAimCanceled;
            fireAction.started -= OnFirePerformed;
            fireAction.canceled -= OnFireCanceled;
        }

        private void OnAimStarted(InputAction.CallbackContext context)
        {
            for (int i = 0; i < crosshairImage.Length; i++)
            {
                crosshairImage[i].enabled = false;
            }
        }
        
        private void OnAimCanceled(InputAction.CallbackContext context)
        {
            for (int i = 0; i < crosshairImage.Length; i++)
            {
                crosshairImage[i].enabled = true;
            }
        }

        private void OnFirePerformed(InputAction.CallbackContext context)
        {
            if (shrinkCoroutine != null)
            {
                StopCoroutine(shrinkCoroutine);
                shrinkCoroutine = null;
            }
            if (modifyCoroutine == null)
            {
                modifyCoroutine = StartCoroutine(ModifyCrosshairSize());
            }
        }
        private void OnFireCanceled(InputAction.CallbackContext context)
        {
            if (modifyCoroutine != null)
            {
                StopCoroutine(modifyCoroutine);
                modifyCoroutine = null;
            }
            if (shrinkCoroutine == null)
            {
                shrinkCoroutine = StartCoroutine(ShrinkCrosshairSize());
            }
        }

        private IEnumerator ModifyCrosshairSize()
        {
            var rectTransform = crosshairRectTransform;
            Vector3 target = originalScale * crosshairSize;
            float t = 0;

            while (t < sizeModifyDuration)
            {
                t += Time.unscaledDeltaTime;
                rectTransform.localScale = Vector3.Lerp(originalScale, target, t / sizeModifyDuration);
                yield return null;
            }

            modifyCoroutine = null;
        }

        private IEnumerator ShrinkCrosshairSize()
        {
            var rectTransform = crosshairRectTransform;
            Vector3 startSize = rectTransform.localScale;
            float t = 0;

            while (t < sizeModifyDuration)
            {
                t += Time.unscaledDeltaTime;
                rectTransform.localScale = Vector3.Lerp(startSize, originalScale, t / sizeModifyDuration);
                yield return null;
            }
            rectTransform.localScale = originalScale;
            shrinkCoroutine = null;
        }
    }
}