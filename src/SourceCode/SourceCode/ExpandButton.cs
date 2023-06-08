using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(KAButton))]
public class ExpandButton : MonoBehaviour
{
	public float _Duration = 5f;

	public float _Radius = 4f;

	public Transform _TargetRef;

	public bool _EnableAutoRotTowardsTarget;

	public bool _DefaultClickEnabled = true;

	public bool _AutoDistributeChild = true;

	public float _RotationAngle = 25f;

	public float _InitialRotation;

	private List<Vector3> mChildInitialPos = new List<Vector3>();

	private List<KAWidget> mChildList = new List<KAWidget>();

	private List<Vector3> mChildTargetDirectionList = new List<Vector3>();

	private bool mExpand;

	private bool mExpanded = true;

	private bool mClickEnabled;

	private int mChildCount;

	private float mTimer;

	private KAUIEvents mEvents;

	private KAButton mButton;

	public bool pExpand
	{
		set
		{
			mExpand = value;
		}
	}

	public KAWidget pButton => mButton;

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
				base.gameObject.BroadcastMessage("OnExpandButtonStateChanged", mExpanded, SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	private void Start()
	{
		Init();
	}

	public void Init()
	{
		if (null == mButton)
		{
			mButton = GetComponent<KAButton>();
		}
		if (!(null != mButton))
		{
			return;
		}
		mClickEnabled = _DefaultClickEnabled;
		mButton.pUI.pEvents.OnClick += OnClick;
		mChildCount = mButton.GetNumChildren();
		mChildList.Clear();
		mChildInitialPos.Clear();
		for (int i = 0; i < mChildCount; i++)
		{
			KAWidget kAWidget = mButton.FindChildItemAt(i);
			mChildList.Add(kAWidget);
			mChildInitialPos.Add(kAWidget.transform.localPosition);
			kAWidget.transform.GetComponent<Collider>().enabled = mClickEnabled;
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
				int num2 = mChildCount - 1;
				if (mChildCount < 2)
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
		if (null != mButton && null != mButton.pUI)
		{
			mButton.pUI.pEvents.OnClick -= OnClick;
		}
	}

	private void OnClick(KAWidget inWidget)
	{
		if (mButton.name == inWidget.name)
		{
			pExpand = true;
		}
	}

	private void Update()
	{
		if (mExpand)
		{
			Expand();
		}
	}

	private void Expand()
	{
		mTimer += Time.deltaTime;
		if (mTimer >= _Duration)
		{
			mExpand = false;
			mTimer = 0f;
			pExpanded = !pExpanded;
			mClickEnabled = !mClickEnabled;
			for (int i = 0; i < mChildCount; i++)
			{
				mChildList[i].transform.GetComponent<Collider>().enabled = mClickEnabled;
			}
			return;
		}
		for (int j = 0; j < mChildCount; j++)
		{
			KAWidget kAWidget = mChildList[j];
			if (pExpanded)
			{
				kAWidget.MoveTo(mChildInitialPos[j] + mChildTargetDirectionList[j] * _Radius, _Duration);
			}
			else
			{
				kAWidget.MoveTo(mChildInitialPos[j], _Duration);
			}
		}
	}
}
