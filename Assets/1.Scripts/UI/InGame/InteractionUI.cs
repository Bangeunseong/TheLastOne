using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _1.Scripts.UI.InGame
{
    public class InteractionUI : MonoBehaviour
    {
        [SerializeField] private Animator animator;


        public void Show()
        {
            this.gameObject.SetActive(true);
            animator.ResetTrigger("Hide");
            animator.SetTrigger("Show");
        }

        public void Hide()
        {
            animator.ResetTrigger("Show");
            animator.SetTrigger("Hide");
            this.gameObject.SetActive(false);
        }
    }
}