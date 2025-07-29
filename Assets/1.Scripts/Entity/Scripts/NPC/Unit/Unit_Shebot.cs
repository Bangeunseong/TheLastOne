using System;
using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.Npc.StatControllers.Base;
using _1.Scripts.Interfaces.NPC;
using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Subs;
using UnityEngine;

public class Unit_Shebot : MonoBehaviour
{
    private BaseNpcStatController statController;
    
    private Coroutine gotDamagedCoroutine;
    private float gotDamagedParticleDuration = 0.5f;

    public ParticleSystem p_hit, p_dead, p_smoke;

    private void Awake()
    {
        statController = GetComponent<BaseNpcStatController>();
    }

    public void f_hit() //hit
    {
        // 0번 : 공격, 1번 : 삐빅 시그널. 2번 : 사망, 3번 : 맞았을때
        CoreManager.Instance.soundManager.PlaySFX(SfxType.Drone, transform.position, index: 3);
        if (gotDamagedCoroutine != null)
        {
            StopCoroutine(gotDamagedCoroutine);
        }

        if (statController != null && statController is IStunnable stunnable)
        {
            if (!stunnable.IsStunned)
            {
                gotDamagedCoroutine = StartCoroutine(DamagedParticleCoroutine());
            }
        }
    }

    private IEnumerator DamagedParticleCoroutine()
    {
        p_hit.Play();
        yield return new WaitForSeconds(gotDamagedParticleDuration);
        p_hit.Stop();
    }
    
    public void f_prevDead()
    {
        CoreManager.Instance.soundManager.PlaySFX(SfxType.Drone, transform.position, index:1);
    }

    public void f_Dead()
    {
        p_dead.Play();
        p_smoke.Play();
        CoreManager.Instance.soundManager.PlaySFX(SfxType.Drone, transform.position, index:2);
    }
}
