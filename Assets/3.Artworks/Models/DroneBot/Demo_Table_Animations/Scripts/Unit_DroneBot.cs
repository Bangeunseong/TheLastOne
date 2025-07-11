using System;
using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.NPC.AIBehaviors.BehaviorDesigner.SharedVariables;
using _1.Scripts.Entity.Scripts.NPC.AIControllers;
using _1.Scripts.Entity.Scripts.Npc.StatControllers.Base;
using _1.Scripts.Interfaces.NPC;
using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Subs;
using _1.Scripts.Static;
using _1.Scripts.Weapon.Scripts.Guns;
using BehaviorDesigner.Runtime;
using UnityEngine;
using Random = UnityEngine.Random;

public class Unit_DroneBot : MonoBehaviour
{
	public Transform Shell;
	public Transform Gun_EndR;
	public Transform Gun_EndL;

	public ParticleSystem[] p_jet;
	private bool restartRes = true;
	private float shellSpeed = 70f;
	private Transform pos_side;

	public ParticleSystem p_hit, p_dead, p_smoke, p_fireL, p_fireSmokeL, p_fireR, p_fireSmokeR; //Particle effect  
	private AudioSource m_AudioSource;
	public LayerMask hittableLayer;

	private Coroutine gotDamagedCoroutine;
	private float gotDamagedParticleDuration = 0.5f;

	private BehaviorTree behaviorTree;
	private BaseNpcStatController statController;
	
	// Use this for initialization
	void Start()
	{
		m_AudioSource = GetComponent<AudioSource>();
	}

	private void Awake()
	{
		behaviorTree = GetComponent<BehaviorTree>();
		statController = GetComponent<BaseNpcStatController>();
	}

	void f_hit() //hit
	{
		// 0번 : 공격, 1번 : 삐빅 시그널. 2번 : 사망, 3번 : 맞았을때
		CoreManager.Instance.soundManager.PlaySFX(SfxType.Drone, transform.position, index: 3);
		if (gotDamagedCoroutine != null)
		{
			StopCoroutine(gotDamagedCoroutine);
		}

		var statController = behaviorTree.GetVariable("statController") as SharedBaseNpcStatController;

		if (statController != null && statController.Value is IStunnable stunnable)
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
	
	void f_afterFire()
	{
		p_fireSmokeL.Play();
		p_fireSmokeR.Play();
	}

	void f_start()
	{
		if (!restartRes)
		{
			restartRes = true;
			m_AudioSource.loop = true;
			m_AudioSource.Play();

			for (int i = 0; i < p_jet.Length; i++)
			{
				p_jet[i].Play();
			}
		}
	}

	void f_prevDead()
	{
		CoreManager.Instance.soundManager.PlaySFX(SfxType.Drone, transform.position, index:1);
	}

	void f_dead() //dead
	{
		for (int i = 0; i < p_jet.Length; i++)
		{
			p_jet[i].Stop();
		}

		p_dead.Play();
		p_smoke.Play();
		m_AudioSource.Stop();
		CoreManager.Instance.soundManager.PlaySFX(SfxType.Drone, transform.position, index:2);
		m_AudioSource.loop = false;
		restartRes = false;
	}

	void f_fire(int side) //shot 
	{
		var targetTransform = behaviorTree.GetVariable("target_Transform") as SharedTransform;
		var targetPos = behaviorTree.GetVariable("target_Pos") as SharedVector3;
		bool isAlly = statController.RuntimeStatData.IsAlly;

		if (targetTransform == null || targetTransform.Value == null) return;
		
		if (side == 1)
		{
			p_fireR.Play();
			pos_side = Gun_EndR.transform;
		}
		else
		{
			p_fireL.Play();
			pos_side = Gun_EndL.transform;
		}
		
		Vector3 directionToTarget = (targetPos.Value - pos_side.position).normalized;
		
		// 총알 생성
		var shell = CoreManager.Instance.objectPoolManager.Get("Bullet");
		if (shell.TryGetComponent(out Bullet bullet))
		{
			// 레이어 마스크 설정
			int allyMask = 1 << LayerConstants.Ally;
			int enemyMask = 1 << LayerConstants.Enemy;
			int finalLayerMask = hittableLayer;

			if (isAlly)
			{
				finalLayerMask &= ~allyMask; // Ally 제거
				finalLayerMask |= enemyMask; // Enemy 추가
			}
			else
			{
				finalLayerMask &= ~enemyMask; // Enemy 제거
				finalLayerMask |= allyMask;   // Ally 추가
			}
			
			bullet.Initialize(pos_side.position, directionToTarget, 
				150, shellSpeed + Random.Range(-shellSpeed * 0.2f, shellSpeed * 0.2f), 
				statController.RuntimeStatData.BaseDamage, finalLayerMask);
		}

		CoreManager.Instance.soundManager.PlaySFX(SfxType.Drone, transform.position, -1,0);
	}
}
 