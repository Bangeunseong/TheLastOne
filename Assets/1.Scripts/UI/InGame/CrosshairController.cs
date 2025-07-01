using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using _1.Scripts.Manager.Subs;
using UnityEngine.InputSystem;
using PlayerInput = _1.Scripts.Entity.Scripts.Player.Core.PlayerInput;

namespace _1.Scripts.UI.InGame
{
    public class CrosshairController : MonoBehaviour
    {
        [SerializeField] private Image crosshairImage;
        
        [SerializeField] private float crosshairSize = 1.2f;
        [SerializeField] private float sizeModifyDuration = 0.1f;

        private InputAction aimAction;
        private InputAction fireAction;
        private PlayerInput playerInput;
        private Vector3 originalScale;

        private void Awake()
        {
            if (crosshairImage == null) crosshairImage = GetComponent<Image>();
            originalScale = crosshairImage.rectTransform.localScale;
        }

        void OnEnable()
        {
            if (playerInput == null)
            {
                playerInput = FindObjectOfType<PlayerInput>();

                playerInput.PlayerActions.Enable();
                aimAction = playerInput.PlayerActions.Aim;
                fireAction = playerInput.PlayerActions.Fire;
            }
            aimAction.performed += OnAimPerformed;
            aimAction.canceled  += OnAimCanceled;
            fireAction.started += OnFirePerformed;
            fireAction.canceled += OnFireCanceled;
        }

        private void OnDisable()
        {
            if (playerInput == null) return;
            aimAction.performed -= OnAimPerformed;
            aimAction.canceled  -= OnAimCanceled;
            fireAction.started -= OnFirePerformed;
            fireAction.canceled -= OnFireCanceled;
        }

        private void OnAimPerformed(InputAction.CallbackContext context)
        {
            crosshairImage.enabled = false;
        }
        
        private void OnAimCanceled(InputAction.CallbackContext context)
        {
            crosshairImage.enabled = true;
        }

        private void OnFirePerformed(InputAction.CallbackContext context)
        {
            StopAllCoroutines();
            StartCoroutine(ModifyCrosshairSize());
        }
        private void OnFireCanceled(InputAction.CallbackContext context)
        {
            StopAllCoroutines();
            StartCoroutine(ShrinkCrosshairSize());
        }

        private IEnumerator ModifyCrosshairSize()
        {
            var rectTransform = crosshairImage.rectTransform;
            Vector3 target = originalScale * crosshairSize;
            float t = 0;

            while (t < sizeModifyDuration)
            {
                t += Time.unscaledDeltaTime;
                rectTransform.localScale = Vector3.Lerp(originalScale, target, t / sizeModifyDuration);
                yield return null;
            }
        }

        private IEnumerator ShrinkCrosshairSize()
        {
            var rectTransform = crosshairImage.rectTransform;
            Vector3 startSize = rectTransform.localScale;
            float t = 0;

            while (t < sizeModifyDuration)
            {
                t += Time.unscaledDeltaTime;
                rectTransform.localScale = Vector3.Lerp(startSize, originalScale, t / sizeModifyDuration);
                yield return null;
            }
            rectTransform.localScale = originalScale;
        }
    }
}