using System.Collections;
using _1.Scripts.Entity.Scripts.Npc.StatControllers.Base;
using _1.Scripts.Interfaces.Common;
using _1.Scripts.Interfaces.NPC;
using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Subs;
using _1.Scripts.Static;
using BehaviorDesigner.Runtime;
using UnityEngine;

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

	private Coroutine gotDamagedCoroutine;
	private float gotDamagedParticleDuration = 0.5f;

	private BehaviorTree behaviorTree;
	private BaseNpcStatController statController;
	[SerializeField] private LayerMask hittableLayer = 0;
	
	private void Awake()
	{
		m_AudioSource = GetComponent<AudioSource>();
		behaviorTree = GetComponent<BehaviorTree>();
		statController = GetComponent<BaseNpcStatController>();

		hittableLayer = LayerConstants.ToLayerMask(LayerConstants.DefaultHittableLayers);
	}

	void f_hit() //hit
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

		int targetMask = isAlly
			? LayerConstants.ToLayerMask(LayerConstants.EnemyLayers)
			: LayerConstants.ToLayerMask(LayerConstants.AllyLayers);
		int finalMask = hittableLayer | targetMask;
		
		if (Physics.Raycast(pos_side.position, directionToTarget, out RaycastHit hit, 100f, finalMask))
		{
			bool targetDetected = isAlly
				? LayerConstants.EnemyLayers.Contains(hit.collider.gameObject.layer) 
				: LayerConstants.AllyLayers.Contains(hit.collider.gameObject.layer);

			if (targetDetected && hit.collider.TryGetComponent(out IDamagable damagable))
			{
				damagable.OnTakeDamage(statController.RuntimeStatData.BaseDamage);
			}
		}
		
		CoreManager.Instance.soundManager.PlaySFX(SfxType.Drone, transform.position, -1,0);
	}
}
 