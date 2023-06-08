using System;
using UnityEngine;

public class ObAvatarPropInfo : MonoBehaviour
{
	[Serializable]
	public class AvatarNameInfo
	{
		public string _Name;

		public bool _UseUserId;
	}

	[Serializable]
	public class PropData
	{
		public string _ParentBone;

		public string _AnimationState;

		public Transform _Transform;

		public Vector3 _Offset = Vector3.zero;

		public Vector3 _Rotation = Vector3.zero;

		public Vector3 _Scale = Vector3.one;
	}

	[Serializable]
	public class PropInfo
	{
		public AvatarNameInfo[] _NameInfo;

		public PropData _PropData;
	}

	public string _PartType;

	public string _ApplyPropViewState;

	public string _ResetPropViewState;

	private string mAnimState;

	private Animator mAnim;

	private Transform mPrevParentBone;

	public PropData _DefaultPropData;

	public PropInfo[] _PropInfo;

	private void ApplyPropInfo(string name)
	{
		PropData propData = _DefaultPropData;
		PropInfo[] propInfo = _PropInfo;
		foreach (PropInfo propInfo2 in propInfo)
		{
			AvatarNameInfo[] nameInfo = propInfo2._NameInfo;
			foreach (AvatarNameInfo obj in nameInfo)
			{
				string text = obj._Name;
				if (obj._UseUserId)
				{
					MMOAvatar component = base.transform.root.GetComponent<MMOAvatar>();
					text += ((component == null) ? UserInfo.pInstance.UserID : component.pUserID);
				}
				if (text == name)
				{
					propData = propInfo2._PropData;
				}
			}
		}
		string n = (propData._ParentBone.Contains("/") ? propData._ParentBone : AvatarData.GetParentBone(propData._ParentBone));
		Transform transform = base.transform.root.Find(n);
		if (!(transform != null))
		{
			return;
		}
		if (!string.IsNullOrEmpty(propData._AnimationState))
		{
			mAnimState = propData._AnimationState;
			if (mAnim == null)
			{
				mAnim = base.transform.root.GetComponentInChildren<Animator>();
			}
			if (mAnim != null)
			{
				mAnim.SetBool(propData._AnimationState, value: true);
			}
		}
		else if (mAnim != null && !string.IsNullOrEmpty(mAnimState))
		{
			mAnim.SetBool(mAnimState, value: false);
		}
		Transform parent = base.transform.parent.parent;
		if (!(transform == parent))
		{
			Transform transform2 = transform.Find(_PartType);
			if (transform2 != null)
			{
				transform2.parent = null;
				UnityEngine.Object.Destroy(transform2.gameObject);
			}
			mPrevParentBone = parent;
			base.transform.parent.parent = transform;
			base.transform.parent.localPosition = Vector3.zero;
			base.transform.parent.localEulerAngles = Vector3.zero;
			Transform obj2 = ((propData._Transform != null) ? propData._Transform : base.transform);
			obj2.localPosition = propData._Offset;
			obj2.localEulerAngles = propData._Rotation;
			obj2.localScale = propData._Scale;
		}
	}

	private void ResetPropInfo(string partType)
	{
		if (!(partType != _PartType))
		{
			if (!string.IsNullOrEmpty(_ResetPropViewState))
			{
				base.transform.parent.parent.BroadcastMessage("ApplyViewInfo", _ResetPropViewState, SendMessageOptions.DontRequireReceiver);
			}
			if (mAnim != null && !string.IsNullOrEmpty(mAnimState))
			{
				mAnim.SetBool(mAnimState, value: false);
			}
			if (mPrevParentBone != null)
			{
				base.transform.parent.parent = mPrevParentBone;
			}
		}
	}

	private void OnUpdateAvatar()
	{
		if (!string.IsNullOrEmpty(_ApplyPropViewState))
		{
			base.transform.parent.parent.BroadcastMessage("ApplyViewInfo", _ApplyPropViewState, SendMessageOptions.DontRequireReceiver);
		}
	}
}
