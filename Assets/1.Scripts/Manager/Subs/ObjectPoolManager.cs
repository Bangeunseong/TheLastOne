using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

namespace _1.Scripts.Manager.Subs
{
    [Serializable] 
    public class ObjectPoolManager
    {
        private Dictionary<string, ObjectPool<GameObject>> pools = new();
        private Transform poolRoot;   
        
        private int defaultCapacity = 50;
        private int maxCapacity = 500;

        public ObjectPoolManager()
        {
            poolRoot = new GameObject("ObjectPoolManager").transform;
            UnityEngine.Object.DontDestroyOnLoad(poolRoot.gameObject);
        }
        
        /// <summary>
        /// 오브젝트풀 생성
        /// 최소/최대 용량지정은 해도되고 안해도됨
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="capacity"></param>
        /// <param name="maxSize"></param>
        public void CreatePool(GameObject prefab, int capacity = -1, int maxSize = -1)
        {
            if (pools.ContainsKey(prefab.name))
            {
                Debug.LogWarning($"Pool for '{prefab.name}' already exists.");
                return;
            }
    
            int poolCapacity = capacity > 0 ? capacity : defaultCapacity;
            int poolMaxSize = maxSize > 0 ? maxSize : maxCapacity;

            var pool = new ObjectPool<GameObject>(
                createFunc: () => UnityEngine.Object.Instantiate(prefab, poolRoot),
                actionOnGet: item => item.gameObject.SetActive(true),
                actionOnRelease: item =>
                {
                    item.gameObject.SetActive(false);
                    item.transform.SetParent(poolRoot);
                },
                actionOnDestroy: item => UnityEngine.Object.Destroy(item.gameObject),
                collectionCheck: false,
                defaultCapacity: poolCapacity,
                maxSize: poolMaxSize);

            List<GameObject> temp = new List<GameObject>();

            for (int i = 0; i < poolCapacity; ++i)
            {
                GameObject obj = pool.Get();
                temp.Add(obj);
            }

            foreach (GameObject obj in temp)
            {
                pool.Release(obj);
            }

            pools[prefab.name] = pool;
        }

        public async Task CreatePoolsFromResourceAsync()
        {
            
        }
        
        
    }
}