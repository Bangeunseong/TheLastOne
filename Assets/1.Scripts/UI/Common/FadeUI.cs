using System.Collections;
using System.Collections.Generic;
using _1.Scripts.UI;
using UnityEngine;

namespace _1.Scripts.UI.Common
{
    public class FadeUI : UIBase
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Animator animator;
        
        private Coroutine fadeInCoroutine;

        /// <summary>
        /// Fade Out
        /// </summary>
        public override void Show()
        {
            Service.Log("FadeUI: Fade In");
            panel.SetActive(true);
            animator.Play("Out");
        }
        /// <summary>
        /// Fade In
        /// </summary>
        public override void Hide()
        {
            if (!gameObject.activeInHierarchy) return;
            Service.Log("FadeUI: Fade Out");
            if (fadeInCoroutine != null) StopCoroutine(fadeInCoroutine);
            fadeInCoroutine = StartCoroutine(FadeInCoroutine());
        }

        private IEnumerator FadeInCoroutine()
        {
            if (!gameObject.activeInHierarchy) yield break;
            animator.Play("In");
            yield return new WaitForSeconds(0.5f);
            panel.SetActive(false);
            yield return null;
        }
    }
}