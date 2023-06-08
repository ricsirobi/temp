using System.Collections.Generic;
using UnityEngine;

public class ObCullingHandler : MonoBehaviour
{
	public enum ImportanceLevel
	{
		HIGH,
		MEDIUM,
		LOW,
		LOW_FAR
	}

	public class Status
	{
		public float _CurrentTime;

		public bool _Handled;
	}

	public ImportanceLevel _Importance;

	public string Exclude = "";

	public string Include = "";

	public bool OptimizeObjects = true;

	public bool OptimizeEmitters = true;

	private Dictionary<Renderer, float> Renderers;

	private Dictionary<ParticleSystem, Status> Emitters;

	private float LastFPS = 1000f;

	private Vector3 LastAvatarPosition = Vector3.zero;

	private float LastOptimizeTime;

	public void Awake()
	{
	}

	public void Update()
	{
	}

	public void GetValues(out float MinFPS, out float MaxSqrDist, out float MinFPSAtDist)
	{
		switch (_Importance)
		{
		case ImportanceLevel.HIGH:
			MinFPS = 5f;
			MaxSqrDist = 22500f;
			MinFPSAtDist = 20f;
			break;
		case ImportanceLevel.MEDIUM:
			MinFPS = 15f;
			MaxSqrDist = 4900f;
			MinFPSAtDist = 25f;
			break;
		case ImportanceLevel.LOW_FAR:
			MinFPS = 20f;
			MaxSqrDist = 6400f;
			MinFPSAtDist = 30f;
			break;
		default:
			MinFPS = 20f;
			MaxSqrDist = 1600f;
			MinFPSAtDist = 30f;
			break;
		}
	}

	public bool ShouldOptimize(float MinFPS)
	{
		float pFrameRate = GrFPS.pFrameRate;
		if (Time.realtimeSinceStartup - LastOptimizeTime < 2f)
		{
			return false;
		}
		if (pFrameRate < MinFPS && LastFPS > MinFPS)
		{
			return true;
		}
		if (Mathf.Abs(pFrameRate - LastFPS) > 4f)
		{
			return true;
		}
		if (AvAvatar.pObject != null && (LastAvatarPosition - AvAvatar.position).sqrMagnitude > 25f)
		{
			return true;
		}
		return false;
	}

	public void Optimize(float MinFPS, float MaxSqrDist, float MinFPSAtDist)
	{
		LastOptimizeTime = Time.realtimeSinceStartup;
		float num = (LastFPS = GrFPS.pFrameRate);
		if (AvAvatar.pObject != null)
		{
			LastAvatarPosition = AvAvatar.position;
		}
		bool forceDisable = false;
		if (num < MinFPS)
		{
			forceDisable = true;
		}
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		string[] excludes = null;
		string[] includes = null;
		if (Emitters == null || Renderers == null)
		{
			if (!string.IsNullOrEmpty(Exclude))
			{
				excludes = Exclude.ToLower().Split(';');
			}
			if (!string.IsNullOrEmpty(Include))
			{
				includes = Include.ToLower().Split(';');
			}
		}
		List<object> list = new List<object>();
		if (OptimizeEmitters)
		{
			if (Emitters == null)
			{
				Emitters = new Dictionary<ParticleSystem, Status>();
				ParticleSystem[] componentsInChildren = GetComponentsInChildren<ParticleSystem>(includeInactive: true);
				foreach (ParticleSystem particleSystem in componentsInChildren)
				{
					if (CanHandleAsset(particleSystem.gameObject, excludes, includes))
					{
						Status value = new Status();
						Emitters.Add(particleSystem, value);
					}
				}
			}
			foreach (KeyValuePair<ParticleSystem, Status> emitter in Emitters)
			{
				if (HandleCulling_ParticleSystem(emitter, realtimeSinceStartup, num, forceDisable, MinFPS, MaxSqrDist, MinFPSAtDist))
				{
					emitter.Value._CurrentTime = realtimeSinceStartup;
				}
			}
		}
		if (!OptimizeObjects)
		{
			return;
		}
		if (Renderers == null)
		{
			Renderers = new Dictionary<Renderer, float>();
			Renderer[] componentsInChildren2 = GetComponentsInChildren<Renderer>(includeInactive: true);
			foreach (Renderer renderer in componentsInChildren2)
			{
				if (renderer != null && CanHandleAsset(renderer.gameObject, excludes, includes))
				{
					Renderers.Add(renderer, realtimeSinceStartup);
				}
			}
		}
		foreach (KeyValuePair<Renderer, float> renderer2 in Renderers)
		{
			if (HandleCulling_Renderer(renderer2.Key, renderer2.Value, realtimeSinceStartup, num, forceDisable, MinFPS, MaxSqrDist, MinFPSAtDist))
			{
				list.Add(renderer2.Key);
			}
		}
		foreach (object item in list)
		{
			Renderers[(Renderer)item] = realtimeSinceStartup;
		}
		list.Clear();
	}

	private bool CanHandleAsset(GameObject go, string[] Excludes, string[] Includes)
	{
		if (NGUITools.FindInParents<ObCullingHandler>(go).gameObject != base.gameObject)
		{
			return false;
		}
		if (Excludes != null && Excludes.Length != 0)
		{
			string[] array = Excludes;
			foreach (string value in array)
			{
				if (go.name.ToLower().Contains(value))
				{
					return false;
				}
			}
		}
		if (Includes != null && Includes.Length != 0)
		{
			string[] array = Includes;
			foreach (string value2 in array)
			{
				if (go.name.ToLower().Contains(value2))
				{
					return true;
				}
			}
			return false;
		}
		return true;
	}

	private bool HandleCulling_Renderer(Renderer pRenderer, float TimeOfLastUpdate, float CurrTime, float CurrentFPS, bool ForceDisable, float MinFPS, float MaxSqrDist, float MinFPSatDist)
	{
		if (pRenderer == null)
		{
			return false;
		}
		if (ForceDisable)
		{
			return SetRendererStats(pRenderer, Enable: false, CurrTime, TimeOfLastUpdate);
		}
		float sqrMagnitude = (LastAvatarPosition - pRenderer.transform.position).sqrMagnitude;
		if (sqrMagnitude > MaxSqrDist)
		{
			return SetRendererStats(pRenderer, Enable: false, CurrTime, TimeOfLastUpdate);
		}
		float t = sqrMagnitude / MaxSqrDist;
		float num = Mathf.Lerp(MinFPS, 30f, t);
		return SetRendererStats(pRenderer, CurrentFPS > num, CurrTime, TimeOfLastUpdate);
	}

	private bool SetRendererStats(Renderer pRenderer, bool Enable, float CurrTime, float TimeOfLastUpdate)
	{
		if (pRenderer.enabled == Enable)
		{
			return Enable;
		}
		if (CurrTime - TimeOfLastUpdate < 5f && !Enable)
		{
			return false;
		}
		pRenderer.enabled = Enable;
		return true;
	}

	private bool HandleCulling_ParticleSystem(KeyValuePair<ParticleSystem, Status> Emitter, float CurrTime, float CurrentFPS, bool ForceDisable, float MinFPS, float MaxSqrDist, float MinFPSatDist)
	{
		ParticleSystem key = Emitter.Key;
		if (key == null)
		{
			return false;
		}
		if (ForceDisable)
		{
			return SetEmitterStats(Emitter, Enable: false, CurrTime);
		}
		float sqrMagnitude = (LastAvatarPosition - key.transform.position).sqrMagnitude;
		if (sqrMagnitude > MaxSqrDist)
		{
			return SetEmitterStats(Emitter, Enable: false, CurrTime);
		}
		float t = sqrMagnitude / MaxSqrDist;
		float num = Mathf.Lerp(MinFPS, 30f, t);
		return SetEmitterStats(Emitter, CurrentFPS > num, CurrTime);
	}

	private bool SetEmitterStats(KeyValuePair<ParticleSystem, Status> Particle, bool Enable, float CurrTime)
	{
		ParticleSystem key = Particle.Key;
		Status value = Particle.Value;
		if (key.gameObject.activeSelf == Enable)
		{
			return Enable;
		}
		if (CurrTime - value._CurrentTime < 5f && !Enable)
		{
			return false;
		}
		if (Enable)
		{
			if (value._Handled)
			{
				value._Handled = false;
				if (key != null)
				{
					key.Play();
				}
				key.gameObject.SetActive(value: true);
			}
		}
		else
		{
			if (key.gameObject.activeSelf)
			{
				value._Handled = true;
			}
			if (key != null)
			{
				key.Stop();
			}
			key.Clear();
			key.gameObject.SetActive(value: false);
		}
		return true;
	}
}
