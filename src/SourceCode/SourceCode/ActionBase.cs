using UnityEngine;

public abstract class ActionBase : MonoBehaviour
{
	public string _ActionName = "Harvest";

	protected ObjectActionsCSM mCSM;

	public virtual void Start()
	{
		mCSM = GetComponent<ObjectActionsCSM>();
		if (mCSM != null)
		{
			mCSM.RegisterCSMAction(this);
		}
	}

	public virtual bool IsActionAllowed()
	{
		return AvAvatar.pState != AvAvatarState.PAUSED;
	}

	public abstract void ExecuteAction();
}
