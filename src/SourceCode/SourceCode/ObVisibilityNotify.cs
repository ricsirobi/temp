using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class ObVisibilityNotify : MonoBehaviour
{
	public enum NotifyOption
	{
		UPWARDS,
		BROADCAST
	}

	public string _OnVisibleMessage = "OnVisible";

	public string _OnInvisibleMessage = "OnInvisible";

	public NotifyOption _NotifyOption;

	private void Start()
	{
	}

	private void OnBecameVisible()
	{
		if (_NotifyOption == NotifyOption.UPWARDS)
		{
			base.gameObject.SendMessageUpwards(_OnVisibleMessage, SendMessageOptions.DontRequireReceiver);
		}
		else
		{
			base.gameObject.BroadcastMessage(_OnVisibleMessage, SendMessageOptions.DontRequireReceiver);
		}
	}

	private void OnBecameInvisible()
	{
		if (_NotifyOption == NotifyOption.UPWARDS)
		{
			base.gameObject.SendMessageUpwards(_OnInvisibleMessage, SendMessageOptions.DontRequireReceiver);
		}
		else
		{
			base.gameObject.BroadcastMessage(_OnInvisibleMessage, SendMessageOptions.DontRequireReceiver);
		}
	}
}
