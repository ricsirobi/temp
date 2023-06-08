using UnityEngine;

public class ObChangeAvatarStateData : MonoBehaviour
{
	public float _Gravity = -8f;

	public float _JumpHeight = 4.5f;

	public float _MaxAirSpeed = 8f;

	public float _LowGravityTime = 30f;

	public bool _ForMembersOnly;

	public void OnTriggerEnter(Collider col)
	{
		GameObject gameObject = CheckForAvatar(col.gameObject);
		if (gameObject != null)
		{
			CreateOrResetOverride(gameObject);
		}
	}

	public void OnTriggerStay(Collider col)
	{
		GameObject gameObject = CheckForAvatar(col.gameObject);
		if (gameObject != null)
		{
			CreateOrResetOverride(gameObject);
		}
	}

	public void OnTriggerExit(Collider col)
	{
		GameObject gameObject = CheckForAvatar(col.gameObject);
		if (gameObject != null)
		{
			StartTimer(gameObject);
		}
	}

	private GameObject CheckForAvatar(GameObject checkObj)
	{
		if (checkObj == null)
		{
			return null;
		}
		AvAvatarController component = checkObj.GetComponent<AvAvatarController>();
		if (component == null)
		{
			return null;
		}
		if (!_ForMembersOnly || component.IsMember())
		{
			return checkObj;
		}
		return null;
	}

	private void CreateOrResetOverride(GameObject avatarObj)
	{
		AvStateOverride avStateOverride = AvStateOverride.FindOverride(avatarObj, AvAvatarSubState.NORMAL);
		if (avStateOverride != null)
		{
			avStateOverride.OnResetTimer();
		}
		else
		{
			AvStateOverride.SetOrCreate(avatarObj, AvAvatarSubState.NORMAL, inRemoveOnLevelLoad: true, _LowGravityTime, _Gravity, _JumpHeight, _MaxAirSpeed);
		}
	}

	public void StartTimer(GameObject avatarObj)
	{
		AvStateOverride avStateOverride = AvStateOverride.FindOverride(avatarObj, AvAvatarSubState.NORMAL);
		if (avStateOverride != null)
		{
			avStateOverride.OnStartTimer();
		}
	}
}
