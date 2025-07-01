using System;
using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.Player.Core;
using UnityEngine;
using UnityEngine.UI;
using _1.Scripts.Manager.Subs;

namespace _1.Scripts.UI.InGame
{
    public class CrosshairController : MonoBehaviour
    {
        [SerializeField] private Image crosshairImage;
        
        [SerializeField] private float crosshairSize = 1.2f;
        [SerializeField] private float sizeModifyDuration = 0.1f;

        private PlayerInput playerInput;
        private Vector3 originalScale;

        private void Awake()
        {
            if (crosshairImage == null) crosshairImage = GetComponent<Image>();
            originalScale = crosshairImage.rectTransform.localScale;
        }

        void Start()
        {
            playerInput = FindObjectOfType<PlayerInput>();
            if (playerInput == null)
            {
                Debug.LogError("PlayerInput is null");
                enabled = false;
                return;
            }
            var actions = playerInput.PlayerActions;
            actions.Enable();

            actions.Aim.performed += _ => crosshairImage.enabled = false;
            actions.Aim.canceled += _ => crosshairImage.enabled = true;

            actions.Fire.performed += _ =>
            {
                StopAllCoroutines();
                StartCoroutine(ModifyCrosshairSize());
            };
        }
        
        private IEnumerator ModifyCrosshairSize()
        {
            var rt = crosshairImage.rectTransform;
            float half = sizeModifyDuration / 2;
            Vector3 target = originalScale * crosshairSize;
            float t = 0;

            while (t < half)
            {
                t += Time.unscaledDeltaTime;
                rt.localScale = Vector3.Lerp(originalScale, target, t / half);
                yield return null;
            }

            t = 0f;
            while (t < half)
            {
                t += Time.unscaledDeltaTime;
                rt.localScale = Vector3.Lerp(target, originalScale, t / half);
                yield return null;
            }
            rt.localScale = originalScale;
        }
        private void OnDestroy()
        {
            var actions = playerInput.PlayerActions;
            actions.Aim.performed -= _ => crosshairImage.enabled = false;
            actions.Aim.canceled  -= _ => crosshairImage.enabled = true;
            actions.Fire.performed -= _ =>
            {
                StopAllCoroutines();
                StartCoroutine(ModifyCrosshairSize());
            };
        }
    }
}