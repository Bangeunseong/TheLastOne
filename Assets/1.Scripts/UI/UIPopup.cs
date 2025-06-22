using UnityEngine;
using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Subs;

namespace _1.Scripts.UI
{
    public abstract class UIPopup : MonoBehaviour
    {
        protected UIManager uiManager;

        public virtual void Init(UIManager manager)
        {
            uiManager = manager;
        }

        public virtual void ClosePopup()
        {
            uiManager.ClosePopup(this);
        }
    }
}