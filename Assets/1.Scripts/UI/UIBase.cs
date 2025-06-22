using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Manager.Subs;
using UnityEngine;


namespace _1.Scripts.UI
{
    public abstract class UIBase : MonoBehaviour
    {
        protected UIManager uiManager;

        public virtual void Init(UIManager manager)
        {
            uiManager = manager;
            if (uiManager == null)
                Service.Log("UIManager가 Null");
        }

        public abstract void SetActive(bool active);
    }
}