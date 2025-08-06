using System;
using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.Npc.StatControllers.Base;
using _1.Scripts.Util;
using UnityEngine;

public class GroupController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private List<BaseNpcStatController> statControllers;
    
    [Header("Setting")]
    [SerializeField] private float delay = 6f;
    
    [Header("For Debugging")]
    [SerializeField] private int aliveCount;
    
    private void Awake()
    {
        aliveCount = statControllers.Count;
        
        foreach (var statController in statControllers)
        {
            statController.OnDeath += OnChildDeath;
        }
    }

    private void Reset()
    {
        foreach (var statController in GetComponentsInChildren<BaseNpcStatController>())
        {
            statControllers.Add(statController);
        }
    }

    private void OnChildDeath()
    {
        aliveCount--;

        if (aliveCount <= 0)
        {
            StartCoroutine(DelayedRelease(delay));
        }
    }

    private IEnumerator DelayedRelease(float delay)
    {
        yield return new WaitForSeconds(delay);
        NpcUtil.DisableNpc(this.gameObject);
    }
}
