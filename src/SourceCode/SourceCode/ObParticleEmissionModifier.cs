using System;
using System.Collections.Generic;
using UnityEngine;

public class ObParticleEmissionModifier : MonoBehaviour
{
	[Serializable]
	public class ParticleModifierInfo
	{
		public List<ParticleSystem> _ParticleSystem;

		public float _MinDist = 50f;

		public float _MaxDist = 250f;

		public float _MinEmissionRate = 0.01f;

		public float _MaxEmissionRate = 3f;

		public string _ColorValueName = "_TintColor";
	}

	[Serializable]
	public class ColorModifierInfo
	{
		public List<Renderer> _Renderers;

		public float _MinDist = 50f;

		public float _MaxDist = 250f;

		public string _ColorValueName = "_TintColor";

		public Color _ColorValFar = new Color(0.5372f, 0.5372f, 0.5372f, 0.5f);

		public Color _ColorValNear = new Color(0.5372f, 0.5372f, 0.5372f, 0.01f);
	}

	public List<ParticleModifierInfo> _ModifierInfo;

	public List<ColorModifierInfo> _ColorModifierInfo;

	private void Start()
	{
	}

	private void Update()
	{
		if (!(AvAvatar.pObject != null) || _ModifierInfo == null)
		{
			return;
		}
		foreach (ParticleModifierInfo item in _ModifierInfo)
		{
			foreach (ParticleSystem item2 in item._ParticleSystem)
			{
				if (item2 != null)
				{
					Mathf.Clamp(Vector3.Distance(item2.transform.position, AvAvatar.mTransform.position), item._MinDist, item._MaxDist);
					ParticleSystem.EmissionModule emission = item2.emission;
					emission.rateOverTime = new ParticleSystem.MinMaxCurve(item._MinEmissionRate, item._MaxEmissionRate);
				}
			}
		}
		foreach (ColorModifierInfo item3 in _ColorModifierInfo)
		{
			foreach (Renderer renderer in item3._Renderers)
			{
				if (renderer != null)
				{
					float t = (Mathf.Clamp(Vector3.Distance(renderer.transform.position, AvAvatar.mTransform.position), item3._MinDist, item3._MaxDist) - item3._MinDist) / (item3._MaxDist - item3._MinDist);
					renderer.material.SetColor(item3._ColorValueName, Color.Lerp(item3._ColorValNear, item3._ColorValFar, t));
				}
			}
		}
	}
}
