using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Manager.Core;
using UnityEngine;

public class Unit_DroneBot : MonoBehaviour
{
	public Transform Shell;
	public Transform Gun_EndR;
	public Transform Gun_EndL;

	public ParticleSystem[] p_jet;
	private bool restartRes = true;
	private float shellSpeed = 500;
	private Transform pos_side;

	public ParticleSystem p_hit, p_dead, p_smoke, p_fireL, p_fireSmokeL, p_fireR, p_fireSmokeR; //Particle effect  
	private AudioSource m_AudioSource;

	public AudioClip s_Fire, s_hit, s_dead, s_signal; //Sound effect 

	// Use this for initialization
	void Start()
	{
		m_AudioSource = GetComponent<AudioSource>();
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
		m_AudioSource.PlayOneShot(s_dead);
		m_AudioSource.loop = false;
		restartRes = false;


	}

	void f_fire(int side) //shot 
	{
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

		Quaternion shellRot = Quaternion.Euler(pos_side.eulerAngles.x + Random.Range(-3f, 3f),
			pos_side.eulerAngles.y + Random.Range(-3f, 3f), pos_side.eulerAngles.z + Random.Range(-3f, 3f));
		GameObject shell = CoreManager.Instance.objectPoolManager.Get("Shell");
		shell.transform.position = pos_side.position;
		shell.transform.rotation = shellRot;

		Vector3 dir = shellRot * Vector3.right * (shellSpeed + Random.Range(-shellSpeed * 0.2f, shellSpeed * 0.2f));

		shell.GetComponent<Rigidbody>().AddForce(dir);

		m_AudioSource.PlayOneShot(s_Fire);
	}
}
 