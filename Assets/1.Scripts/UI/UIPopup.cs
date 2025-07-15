using UnityEngine;
using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Subs;

namespace _1.Scripts.UI
{
    public abstract class UIPopup : UIBase
    {

        public virtual void ClosePopup()
        {
            uiManager.ClosePopup();
        }
        
        public override void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }
    }
}