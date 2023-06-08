using System;
using UnityEngine;

[Serializable]
public class PrtAttachment
{
	public string _NodeName;

	public GameObject _PrtPrefab;

	public bool _DisableEmitInsteadOfDestroy;

	private Transform mNodeTransform;

	private GameObject mParticleGO;

	public Transform pNodeTransform
	{
		get
		{
			return mNodeTransform;
		}
		set
		{
			mNodeTransform = value;
		}
	}

	public GameObject pParticleGO
	{
		get
		{
			return mParticleGO;
		}
		set
		{
			mParticleGO = value;
		}
	}
}
