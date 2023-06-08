using UnityEngine;

public class ObClickableNPCIcon : ObClickable
{
	public override void OnActivate()
	{
		_MessageObject.SendMessage("OnActivate", null, SendMessageOptions.DontRequireReceiver);
	}
}
