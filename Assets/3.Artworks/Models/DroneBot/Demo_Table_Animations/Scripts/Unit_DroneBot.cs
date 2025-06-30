using System;
using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.NPC.AIControllers;
using _1.Scripts.Manager.Core;
using _1.Scripts.Weapon.Scripts.Guns;
using UnityEngine;
using Random = UnityEngine.Random;

public class Unit_DroneBot : MonoBehaviour
{
	public Transform Shell;
	public Transform Gun_EndR;
	public Transform Gun_EndL;

	public ParticleSystem[] p_jet;
	private bool restartRes = true;
	private float shellSpeed = 5;
	private Transform pos_side;

	public ParticleSystem p_hit, p_dead, p_smoke, p_fireL, p_fireSmokeL, p_fireR, p_fireSmokeR; //Particle effect  
	private AudioSource m_AudioSource;

	public AudioClip s_Fire, s_hit, s_dead, s_signal; //Sound effect 
	public LayerMask hittableLayer;

	public BaseNpcAI controller;
	
	// Use this for initialization
	void Start()
	{
		m_AudioSource = GetComponent<AudioSource>();
	}

	private void Awake()
	{
		controller = GetComponent<BaseNpcAI>();
	}

	void f_hit() //hit
	{
		p_hit.Play();
		m_AudioSource.PlayOneShot(s_hit);

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

		m_AudioSource.PlayOneShot(s_signal);
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
		m_AudioSource.PlayOneShot(s_dead);
		m_AudioSource.loop = false;
		restartRes = false;
	}

	void f_fire(int side) //shot 
	{
		if (controller.targetTransform == null) return;
		
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
		
		// 목표 방향 계산: 플레이어 위치 기준
		Transform player = controller.targetTransform;
		Vector3 directionToPlayer = (player.position - pos_side.position).normalized;
		
		// 총알 생성
		var shell = CoreManager.Instance.objectPoolManager.Get("Bullet");
		if (shell.TryGetComponent(out Bullet bullet))
		{
			bullet.Initialize(pos_side.position, directionToPlayer, 
				150, shellSpeed + Random.Range(-shellSpeed * 0.2f, shellSpeed * 0.2f), 
				10, hittableLayer);
		}

		m_AudioSource.PlayOneShot(s_Fire);
	}
}
 