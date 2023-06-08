using System;
using UnityEngine;

[Serializable]
public class AITarget
{
	public TargetTypes TargetType;

	public Transform _Target;

	public Vector3 _Offset = Vector3.zero;

	public bool _ApplyRotationToOffset;

	[HideInInspector]
	public AIActor Actor;

	public AITarget()
	{
	}

	public AITarget(Transform Obj)
	{
		_Target = Obj;
	}

	public AITarget(Vector3 Pos)
	{
		_Target = null;
		_Offset = Pos;
	}

	public Vector3 GetLocation()
	{
		Quaternion Rot;
		return GetLocation(out Rot);
	}

	public Vector3 GetLocation(out Quaternion Rot)
	{
		Vector3 vector = OnGetLocation(out Rot);
		if (!_ApplyRotationToOffset)
		{
			return vector + _Offset;
		}
		return vector + Rot * _Offset;
	}

	protected virtual Vector3 OnGetLocation(out Quaternion Rot)
	{
		switch (TargetType)
		{
		case TargetTypes.TARGET:
			return GetLocation(_Target, out Rot);
		case TargetTypes.TARGET_PARENT:
			if (_Target != null && _Target.parent != null)
			{
				return GetLocation(_Target.parent, out Rot);
			}
			return GetLocation(_Target, out Rot);
		case TargetTypes.TARGET_LOCAL_POSITION:
			if (_Target != null)
			{
				Rot = _Target.localRotation;
				return _Target.localPosition;
			}
			return GetLocation(_Target, out Rot);
		case TargetTypes.AVATAR:
			if (Actor != null)
			{
				return GetLocation(Actor.GetAvatar(), out Rot);
			}
			return GetLocation(_Target, out Rot);
		case TargetTypes.AVATAR_TARGET:
			if (Actor != null)
			{
				Transform avatarTarget = Actor.GetAvatarTarget();
				if (avatarTarget != null)
				{
					return GetLocation(avatarTarget, out Rot);
				}
				avatarTarget = Actor.GetAvatar();
				if (avatarTarget != null)
				{
					Rot = avatarTarget.rotation;
					return avatarTarget.position - avatarTarget.forward * 3f;
				}
			}
			return GetLocation(_Target, out Rot);
		case TargetTypes.PET_PLAY_HOME:
			if (KAUIPetPlaySelect.Instance == null || KAUIPetPlaySelect.Instance.GetHomeObject() == null)
			{
				return GetLocation(_Target, out Rot);
			}
			return GetLocation(KAUIPetPlaySelect.Instance.GetHomeObject(), out Rot);
		default:
			return GetLocation(_Target, out Rot);
		}
	}

	protected Vector3 GetLocation(Transform target, out Quaternion rotation)
	{
		if (target != null)
		{
			rotation = target.rotation;
			return target.position;
		}
		rotation = Quaternion.identity;
		return Vector3.zero;
	}
}
