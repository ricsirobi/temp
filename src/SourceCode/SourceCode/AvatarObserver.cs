using UnityEngine;

public class AvatarObserver : MonoBehaviour
{
	private GameObject mSpy;

	public GameObject pSpy
	{
		get
		{
			return mSpy;
		}
		set
		{
			mSpy = value;
		}
	}

	public void OnSpringBoardUse(ObSpringBoard inSB)
	{
		BroadcastMessageToSpy("OnAvatarSpringBoardUse", inSB, SendMessageOptions.DontRequireReceiver);
	}

	public void OnSpringBoardStateEnded()
	{
		BroadcastMessageToSpy("OnAvatarSpringBoardStateEnded", null, SendMessageOptions.DontRequireReceiver);
	}

	public void OnSetPositionDone(Vector3 targetPos)
	{
		BroadcastMessageToSpy("OnAvatarSetPositionDone", targetPos, SendMessageOptions.DontRequireReceiver);
	}

	public void OnGlideModeEnded()
	{
		BroadcastMessageToSpy("OnAvatarGlideModeEnded", null, SendMessageOptions.DontRequireReceiver);
	}

	public void OnLaunchModeStarted()
	{
		BroadcastMessageToSpy("OnAvatarLaunchModeStarted", null, SendMessageOptions.DontRequireReceiver);
	}

	public void OnDetachPets()
	{
		BroadcastMessageToSpy("OnAvatarDetachPets", null, SendMessageOptions.DontRequireReceiver);
	}

	public void OnSlidingStateEnded(Vector3 avatarPos)
	{
		BroadcastMessageToSpy("OnAvatarSlidingStateEnded", avatarPos, SendMessageOptions.DontRequireReceiver);
	}

	public void BroadcastMessageToSpy(string message, object data, SendMessageOptions opt)
	{
		if (mSpy != null)
		{
			mSpy.BroadcastMessage(message, data, opt);
		}
	}
}
