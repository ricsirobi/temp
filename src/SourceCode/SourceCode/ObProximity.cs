using UnityEngine;

public class ObProximity : KAMonoBase
{
	public bool _Draw;

	public bool _Active = true;

	public bool _UseGlobalActive = true;

	public bool _StopTutorialOnLoadLevel;

	public string _LoadLevel = "";

	public string _StartMarker = "";

	public GameObject _ActivateObject;

	public float _Range;

	public Vector3 _Offset;

	private float mLastUpdateTime;

	private bool mWithinRange;

	[SerializeField]
	private bool m_DisableOnLeave;

	public virtual void Update()
	{
		if ((_UseGlobalActive && !ObClickable.pGlobalActive) || !_Active || AvAvatar.pObject == null || _Range == 0f)
		{
			return;
		}
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		if (realtimeSinceStartup - mLastUpdateTime < 0.5f)
		{
			return;
		}
		mLastUpdateTime = realtimeSinceStartup + Random.value * 0.1f;
		if (!IsInProximity(AvAvatar.position))
		{
			mWithinRange = false;
			if (m_DisableOnLeave)
			{
				_ActivateObject.SetActive(value: false);
			}
		}
		else
		{
			if (mWithinRange)
			{
				return;
			}
			mWithinRange = true;
			if (!TryLoadLevel(_LoadLevel))
			{
				if (_ActivateObject != null)
				{
					_ActivateObject.SetActive(value: true);
				}
				SendMessage("OnProximity", null, SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	public bool TryLoadLevel(string inLevel)
	{
		if (!string.IsNullOrEmpty(inLevel) && UnlockManager.IsSceneUnlocked(inLevel))
		{
			AvAvatar.SetActive(inActive: false);
			if (_StartMarker != "")
			{
				AvAvatar.pStartLocation = _StartMarker;
			}
			if (_StopTutorialOnLoadLevel)
			{
				TutorialManager.StopTutorials();
			}
			LoadLevel(inLevel);
			return true;
		}
		return false;
	}

	public virtual void LoadLevel(string level)
	{
		RsResourceManager.LoadLevel(_LoadLevel);
	}

	private void OnDrawGizmos()
	{
		if (_Draw && _Range != 0f)
		{
			Gizmos.color = Color.white;
			Gizmos.DrawWireSphere(base.transform.position + base.transform.TransformDirection(_Offset), _Range);
		}
	}

	public bool IsInProximity(Vector3 position)
	{
		return (position - (base.transform.position + base.transform.TransformDirection(_Offset))).magnitude < _Range;
	}
}
