using _1.Scripts.Manager.Data;
using UnityEngine;

namespace _1.Scripts.Interfaces.Item
{
    public interface IItem
    {
        void Initialize(DataTransferObject dto = null);
        void OnUse(GameObject interactor);
    }
}