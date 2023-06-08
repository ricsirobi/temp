using System.Collections.Generic;
using UnityEngine;

public class CompactUI : MonoBehaviour
{
	public float _Radius = 4f;

	public Transform _TargetRef;

	public bool _EnableAutoRotTowardsTarget;

	public bool _DefaultClickEnabled = true;

	public bool _UseSavedState = true;

	public KAButton _MaximizeButton;

	public KAButton _MinimizeButton;

	public bool _AutoDistributeChild = true;

	public float _RotationAngle = 25f;

	public float _InitialRotation;

	public string _SaveKey = string.Empty;

	public UiAnimStyle.AnimStyle _AnimStyle;

	public KAWidget[] _ChildList;

	private List<Vector3> mChildInitialPos = new List<Vector3>();

	private List<Vector3> mChildTargetDirectionList = new List<Vector3>();

	private bool mExpanded = true;

	private bool mIsInTransition;

	private bool mClickEnabled;

	private UiAnimStyle mUiAnimStyle;

	public bool pIsInTransition => mIsInTransition;

	public bool pExpand
	{
		set
		{
			if (!mIsInTransition)
			{
				mIsInTransition = true;
				if (mUiAnimStyle != null)
				{
					mUiAnimStyle.StartEffect();
				}
			}
		}
	}

	public KAWidget pButton
	{
		get
		{
			if (mExpanded)
			{
				return _MinimizeButton;
			}
			return _MaximizeButton;
		}
	}

	public List<Vector3> pChildInitialPos => mChildInitialPos;

	public List<Vector3> pChildTargetDirectionList => mChildTargetDirectionList;

	public string pKey
	{
		get
		{
			if (UserInfo.pInstance != null)
			{
				return _SaveKey + "-" + UserInfo.pInstance.UserID;
			}
			return _SaveKey;
		}
	}

	public bool pExpanded
	{
		get
		{
			return mExpanded;
		}
		set
		{
			if (mExpanded != value)
			{
				mExpanded = value;
				base.gameObject.BroadcastMessage("OnCompactUIStateChanged", mExpanded, SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	private void Start()
	{
		if (_UseSavedState)
		{
			if (string.IsNullOrEmpty(_SaveKey))
			{
				Debug.LogError("No save key provided. UI state will not be saved/loaded. Make sure this key is unique");
			}
			if (PlayerPrefs.HasKey(pKey))
			{
				_DefaultClickEnabled = PlayerPrefs.GetInt(pKey) == 1;
			}
			else
			{
				_DefaultClickEnabled = true;
			}
		}
		mUiAnimStyle = base.gameObject.GetComponent<UiAnimStyle>();
		if (mUiAnimStyle == null)
		{
			Debug.LogError("No UiAnimStyle script attached to " + base.name);
		}
		Init();
	}

	public void Init()
	{
		if (!(null != _MinimizeButton) || !(null != _MaximizeButton) || _ChildList == null || _ChildList.Length == 0)
		{
			return;
		}
		_MaximizeButton.SetVisibility(!_DefaultClickEnabled);
		_MinimizeButton.SetVisibility(_DefaultClickEnabled);
		mClickEnabled = _DefaultClickEnabled;
		_MaximizeButton.pUI.pEvents.OnClick += OnClick;
		_MinimizeButton.pUI.pEvents.OnClick += OnClick;
		mChildInitialPos.Clear();
		for (int i = 0; i < _ChildList.Length; i++)
		{
			KAWidget kAWidget = _ChildList[i];
			mChildInitialPos.Add(kAWidget.transform.localPosition);
			Vector3 vector;
			if (_TargetRef != null)
			{
				vector = _TargetRef.localPosition - kAWidget.transform.localPosition;
				vector.Normalize();
				if (!_DefaultClickEnabled)
				{
					kAWidget.transform.localPosition = kAWidget.transform.localPosition + vector * _Radius;
					pExpanded = false;
				}
				if (_EnableAutoRotTowardsTarget)
				{
					float num = Vector3.Angle(kAWidget.transform.up, vector);
					if (vector.x > 0f)
					{
						num = 360f - num;
					}
					kAWidget.transform.eulerAngles = new Vector3(kAWidget.transform.eulerAngles.x, kAWidget.transform.eulerAngles.y, num);
				}
			}
			else
			{
				int num2 = _ChildList.Length - 1;
				if (_ChildList.Length < 2)
				{
					num2 = 1;
				}
				float num3 = 0f;
				num3 = ((!_AutoDistributeChild) ? _RotationAngle : (90f / (float)num2));
				kAWidget.transform.localRotation = Quaternion.Euler(0f, 0f, _InitialRotation + num3 * (float)i);
				vector = kAWidget.transform.up;
			}
			mChildTargetDirectionList.Add(vector);
		}
	}

	private void OnDestroy()
	{
		if (_UseSavedState && !string.IsNullOrEmpty(_SaveKey))
		{
			PlayerPrefs.SetInt(pKey, pExpanded ? 1 : 0);
		}
		if (null != _MaximizeButton && null != _MaximizeButton.pUI && _MaximizeButton.pUI.pEvents != null)
		{
			_MaximizeButton.pUI.pEvents.OnClick -= OnClick;
		}
		if (null != _MinimizeButton && null != _MinimizeButton.pUI && _MinimizeButton.pUI.pEvents != null)
		{
			_MinimizeButton.pUI.pEvents.OnClick -= OnClick;
		}
	}

	private void OnClick(KAWidget inWidget)
	{
		if (_MaximizeButton == inWidget)
		{
			pExpand = true;
		}
		else if (_MinimizeButton == inWidget)
		{
			pExpand = true;
		}
	}

	private void Expand()
	{
	}

	public void OnEffectDone()
	{
		mIsInTransition = false;
		pExpanded = !pExpanded;
		_MaximizeButton.SetVisibility(!pExpanded);
		_MinimizeButton.SetVisibility(pExpanded);
		mClickEnabled = !mClickEnabled;
	}

	public void SetInteractive(bool isInteractive)
	{
		_MaximizeButton.SetInteractive(isInteractive);
		_MinimizeButton.SetInteractive(isInteractive);
	}
}
