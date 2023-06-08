using UnityEngine;

public class EquipDiveHose : MonoBehaviour
{
	public GameObject _DiveHose;

	public FixedJoint _HelmetHoseConnection;

	private GameObject mCurrentHose;

	private HoseManager mHoseManager;

	private bool diveHoseEquipped;

	private AvAvatarController mController;

	public bool _TestEquipHose;

	private void Start()
	{
		if (AvAvatar.pObject != null)
		{
			mController = AvAvatar.pObject.GetComponentInChildren<AvAvatarController>();
		}
	}

	private void Update()
	{
		if (_TestEquipHose)
		{
			ActivateDiveHose();
			_TestEquipHose = false;
		}
		if (mController == null)
		{
			return;
		}
		if (mController.pSubState == AvAvatarSubState.DIVESUIT && !diveHoseEquipped)
		{
			ActivateDiveHose();
		}
		else if (mController.pSubState != AvAvatarSubState.DIVESUIT && diveHoseEquipped)
		{
			if (mCurrentHose != null)
			{
				Object.Destroy(mCurrentHose);
			}
			diveHoseEquipped = false;
		}
	}

	public void ActivateDiveHose()
	{
		if (mCurrentHose != null)
		{
			Object.Destroy(mCurrentHose);
		}
		mCurrentHose = Object.Instantiate(_DiveHose, _HelmetHoseConnection.transform.position, _HelmetHoseConnection.transform.rotation);
		if (_HelmetHoseConnection != null && mCurrentHose != null)
		{
			if (mHoseManager == null)
			{
				mHoseManager = mCurrentHose.GetComponent<HoseManager>();
			}
			if (mHoseManager != null)
			{
				_HelmetHoseConnection.connectedBody = mHoseManager._FinalHoseJoint;
				diveHoseEquipped = true;
			}
		}
	}

	private void OnDisable()
	{
		if (mCurrentHose != null)
		{
			Object.Destroy(mCurrentHose);
		}
		diveHoseEquipped = false;
	}
}
