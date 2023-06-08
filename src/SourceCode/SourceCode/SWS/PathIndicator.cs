using System;
using System.Collections;
using UnityEngine;

namespace SWS;

public class PathIndicator : MonoBehaviour
{
	public float modRotation;

	private ParticleSystem pSys;

	private void Start()
	{
		pSys = GetComponentInChildren<ParticleSystem>();
		StartCoroutine("EmitParticles");
	}

	private IEnumerator EmitParticles()
	{
		yield return new WaitForEndOfFrame();
		while (true)
		{
			float num = (base.transform.eulerAngles.y + modRotation) * (MathF.PI / 180f);
			ParticleSystem.MainModule main = pSys.main;
			main.startRotation = num;
			pSys.Emit(1);
			yield return new WaitForSeconds(0.2f);
		}
	}
}
