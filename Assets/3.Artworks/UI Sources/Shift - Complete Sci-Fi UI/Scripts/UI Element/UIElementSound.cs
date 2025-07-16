using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using _1.Scripts.Manager.Subs;
using _1.Scripts.Manager.Core;

namespace Michsky.UI.Shift
{
    [ExecuteInEditMode]
    public class UIElementSound : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
    {
        public SfxType hoverSfxType = SfxType.PopupOpen;
        public int hoverIndex = 0;
        public SfxType clickSfxType = SfxType.ButtonClick;
        public int clickIndex = 0;
        
        [Header("Settings")]
        public bool enableHoverSound = true;
        public bool enableClickSound = true;
        public bool checkForInteraction = true;

        private Button sourceButton;
        private SoundManager soundManager;
        
        private void Start()
        {
            soundManager = CoreManager.Instance?.soundManager;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (checkForInteraction == true && sourceButton != null && sourceButton.interactable == false)
                return;

            if (enableHoverSound == true)
            {
                soundManager?.PlayUISFX(hoverSfxType, -1, hoverIndex);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (checkForInteraction == true && sourceButton != null && sourceButton.interactable == false)
                return;

            if (enableClickSound == true)
            {
                soundManager?.PlayUISFX(clickSfxType, -1, clickIndex);
            }
        }
    }
}