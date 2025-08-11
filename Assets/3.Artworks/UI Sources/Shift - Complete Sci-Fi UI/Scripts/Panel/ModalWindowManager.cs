using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Michsky.UI.Shift
{
    public class ModalWindowManager : MonoBehaviour
    {
        [Header("Resources")]
        public TextMeshProUGUI windowTitle;
        public TextMeshProUGUI windowDescription;

        [Header("Settings")]
        public bool sharpAnimations = false;
        public bool useCustomTexts = false;
        public string titleText = "Title";
        [TextArea] public string descriptionText = "Description here";

        Animator mWindowAnimator;
        bool isOn = false;

        void Awake()
        {
            mWindowAnimator = gameObject.GetComponent<Animator>();

            if (useCustomTexts == false)
            {
                windowTitle.text = titleText;
                windowDescription.text = descriptionText;
            }

            gameObject.SetActive(false);
        }

        void OnEnable()
        {
            isOn = false;
        }
        public void ModalWindowIn()
        {
            StopCoroutine("DisableWindow");
            gameObject.SetActive(true);

            if (isOn == false && mWindowAnimator != null)
            {
                if (sharpAnimations == false)
                    mWindowAnimator.CrossFade("Window In", 0.1f);
                else
                    mWindowAnimator.Play("Window In");

                isOn = true;
            }
        }

        public void ModalWindowOut()
        {
            if (isOn == true && mWindowAnimator != null)
            {
                if (sharpAnimations == false)
                    mWindowAnimator.CrossFade("Window Out", 0.1f);
                else
                    mWindowAnimator.Play("Window Out");

                isOn = false;
            }

            StartCoroutine("DisableWindow");
        }

        IEnumerator DisableWindow()
        {
            yield return new WaitForSecondsRealtime(0.5f);
            gameObject.SetActive(false);
        }
    }
}