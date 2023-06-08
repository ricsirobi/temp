using System.Collections.Generic;
using UnityEngine;

public class CogObject : KAMonoBase
{
	public CogType _CogType;

	public List<CogObject> _ConnectedMachineCogs = new List<CogObject>();

	public List<GameObject> _InContactList = new List<GameObject>();

	public List<GameObject> _InContactRatchetList = new List<GameObject>();

	public float _AngularSpeed;

	public RotateDirection _RotateDirection;

	public bool _IsRachetAttached;

	public GameObject _AttachedRachet;

	public ParticleSystem _MachineParticle;

	public Material _OriginalMaterial;

	public Material _PickedUpMaterial;

	private bool mInvalid;

	private Cog mCachedCog;

	public GFGear pGear { get; set; }

	public CogObject pParent { get; set; }

	public bool? pIsCCW { get; set; }

	public bool pIsMachineDefective { get; set; }

	public Cog pCachedCog => mCachedCog;

	public void Setup(Cog inCog)
	{
		pIsMachineDefective = false;
		if (pGear == null)
		{
			pGear = GetComponent<GFGear>();
		}
		if (inCog != null)
		{
			mCachedCog = inCog;
			_CogType = inCog.CogType;
			_RotateDirection = inCog.RotateDirection;
			_AngularSpeed = inCog.AngularSpeed;
			_IsRachetAttached = inCog.RatchetAttached;
			SetMachineParticles(inPlay: false);
			if (inCog.Transform != null && _CogType != CogType.INVENTORY_COG)
			{
				base.transform.localPosition = inCog.Transform.Position;
				base.transform.localEulerAngles = inCog.Transform.Rotation;
				base.transform.localScale = inCog.Transform.Scale;
			}
			switch (inCog.CogType)
			{
			case CogType.START_COG:
				SetUpStartCog(inCog);
				break;
			case CogType.VICTORY_COG:
				CogsGameManager.pInstance._VictoryCogs.Add(this);
				break;
			}
			CogsGameManager.pInstance._CogsContainer.Add(this);
		}
	}

	private void SetUpStartCog(Cog inCog)
	{
		_ConnectedMachineCogs.Add(this);
		GFMachine gFMachine = (GFMachine)new GameObject("Machine").AddComponent(typeof(GFMachine));
		gFMachine.transform.parent = base.transform.parent;
		gFMachine.transform.localPosition = Vector3.zero;
		base.transform.parent = gFMachine.transform;
		gFMachine.PoweredGear = pGear;
		gFMachine.speed = _AngularSpeed;
		switch (_RotateDirection)
		{
		case RotateDirection.CW:
			gFMachine.Reverse = true;
			pIsCCW = false;
			break;
		case RotateDirection.CCW:
			gFMachine.Reverse = false;
			pIsCCW = true;
			break;
		case RotateDirection.NONE:
			gFMachine.speed = 0f;
			pIsCCW = null;
			break;
		}
		CogsGameManager.pInstance._StartCogs.Add(this);
	}

	protected virtual void OnTriggerEnter(Collider inCol)
	{
		if (inCol.tag == "Cog")
		{
			if (!_InContactList.Contains(inCol.gameObject))
			{
				_InContactList.Add(inCol.gameObject);
			}
		}
		else if (inCol.tag == "Ratchet" && inCol.gameObject != _AttachedRachet && !_InContactRatchetList.Contains(inCol.gameObject))
		{
			_InContactRatchetList.Add(inCol.gameObject);
		}
	}

	protected virtual void OnTriggerStay(Collider inCol)
	{
		if (CogsGameManager.pInstance.pIsPlaying)
		{
			return;
		}
		if (inCol.tag == "Cog")
		{
			if (!_InContactList.Contains(inCol.gameObject))
			{
				_InContactList.Add(inCol.gameObject);
			}
		}
		else if (inCol.tag == "Ratchet" && inCol.gameObject != _AttachedRachet && !_InContactRatchetList.Contains(inCol.gameObject))
		{
			_InContactRatchetList.Add(inCol.gameObject);
		}
	}

	protected virtual void OnTriggerExit(Collider inCol)
	{
		if (inCol.tag == "Cog")
		{
			if (_InContactList.Contains(inCol.gameObject))
			{
				_InContactList.Remove(inCol.gameObject);
			}
		}
		else if (inCol.tag == "Ratchet" && inCol.gameObject != _AttachedRachet && _InContactRatchetList.Contains(inCol.gameObject))
		{
			_InContactRatchetList.Remove(inCol.gameObject);
		}
	}

	public void SetMachineParticles(bool inPlay)
	{
		if (_MachineParticle != null)
		{
			if (inPlay)
			{
				_MachineParticle.Play();
			}
			else
			{
				_MachineParticle.Stop();
			}
		}
	}

	public void SetInvalidColor(bool invalid)
	{
		if (mInvalid == invalid)
		{
			return;
		}
		Renderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			if (invalid)
			{
				if (_PickedUpMaterial != null)
				{
					renderer.material = _PickedUpMaterial;
				}
				Color color = renderer.material.color;
				color.a = 0.5f;
				renderer.material.color = color;
			}
			else
			{
				if (_OriginalMaterial != null)
				{
					renderer.material = _OriginalMaterial;
				}
				Color color2 = renderer.material.color;
				color2.a = 1f;
				renderer.material.color = color2;
			}
		}
		mInvalid = invalid;
	}
}
