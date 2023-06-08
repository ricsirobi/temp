using System.Collections.Generic;
using UnityEngine;

public class LeadingParticleManager : MonoBehaviour
{
	private Dictionary<string, LeadingParticle> mParticles = new Dictionary<string, LeadingParticle>();

	private static LeadingParticleManager mInstance;

	public static void Start(string inName, Transform inStartObject, LeadingParticleData inData)
	{
		if (inData._Target == null && !string.IsNullOrEmpty(inData._TargetName))
		{
			GameObject gameObject = GameObject.Find(inData._TargetName);
			if (gameObject != null)
			{
				inData._Target = gameObject.transform;
			}
		}
		if (!string.IsNullOrEmpty(inName) && (!(inData._Particle == null) || !string.IsNullOrEmpty(inData._ParticleAssetName)) && !(inStartObject == null) && !(inData._Target == null))
		{
			if (mInstance == null)
			{
				mInstance = new GameObject("LeadingParticleManager").AddComponent<LeadingParticleManager>();
			}
			else if (mInstance.mParticles.ContainsKey(inName))
			{
				UtDebug.LogError("The leading particle already exists.");
				return;
			}
			LeadingParticle leadingParticle = new LeadingParticle();
			leadingParticle._Particle = inData._Particle;
			leadingParticle._StartObject = inStartObject;
			leadingParticle._TargetObject = inData._Target;
			leadingParticle._Interval = inData._Interval;
			leadingParticle._StartDistance = inData._StartDistance;
			if (inData._Particle == null)
			{
				leadingParticle.Download(inData._ParticleAssetName);
			}
			mInstance.mParticles.Add(inName, leadingParticle);
		}
	}

	public static void Stop(string inName)
	{
		if (mInstance != null)
		{
			mInstance.mParticles.Remove(inName);
		}
	}

	private void Update()
	{
		foreach (KeyValuePair<string, LeadingParticle> mParticle in mParticles)
		{
			if (mParticle.Value != null)
			{
				mParticle.Value.DoUpdate();
			}
		}
	}
}
