using System;
using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Manager.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _1.Scripts.UI.InGame
{
    public class HackingProgressUI : MonoBehaviour
    {
        [SerializeField] private Slider progressSlider;
        [SerializeField] private Animator animator;
        [SerializeField] private float offsetY;

        private Transform target;
        private Transform camera;

        private void Awake()
        {
            camera = Camera.main.transform;
            gameObject.SetActive(false);
        }

        private void LateUpdate()
        {
            if (camera == null) camera = Camera.main?.transform;
            if (target == null || camera == null) return;
            transform.position = target.position + Vector3.up * offsetY;
            transform.LookAt(camera);
        }

        public void SetTarget(Transform target)
        {
            this.target = target;
            gameObject.SetActive(true);
            animator.Play("Show");
        }
        
        public void SetProgress(float progress)
        {
            progressSlider.value = progress;
        }
        
        public void OnSuccess()
        {
            animator.Play("Success");
            StartCoroutine(DisappearCoroutine(1f));
        }
        
        public void OnFail()
        {
            animator.Play("Fail");
            StartCoroutine(DisappearCoroutine(1f));
        }

        private IEnumerator DisappearCoroutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            CoreManager.Instance.objectPoolManager.Release(gameObject);
        }
    }
}