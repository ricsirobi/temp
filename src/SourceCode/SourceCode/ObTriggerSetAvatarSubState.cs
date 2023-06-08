using UnityEngine;

public class ObTriggerSetAvatarSubState : ObTrigger
{
	public AvAvatarSubState _EnterState;

	public AvAvatarSubState _ExitState;

	public override void DoTriggerAction(GameObject other)
	{
		base.DoTriggerAction(other);
		AvAvatarController componentInChildren = AvAvatar.pObject.GetComponentInChildren<AvAvatarController>();
		if (componentInChildren != null)
		{
			componentInChildren.pSubState = _EnterState;
		}
	}

	public override void DoTriggerExit()
	{
		base.DoTriggerExit();
		AvAvatarController componentInChildren = AvAvatar.pObject.GetComponentInChildren<AvAvatarController>();
		if (componentInChildren != null)
		{
			componentInChildren.pSubState = _ExitState;
		}
	}
}
