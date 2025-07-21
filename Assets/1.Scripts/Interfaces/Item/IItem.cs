using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Data;
using UnityEngine;

namespace _1.Scripts.Interfaces.Item
{
    public interface IItem
    {
        void Initialize(CoreManager coreManager, DataTransferObject dto = null);
        bool OnUse(GameObject interactor);
        bool OnRefill(int value = 1);
    }
}