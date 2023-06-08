public class FishingState
{
	public FishingZone mController;

	private int mState;

	public int pStateId => mState;

	public virtual void Initialize(FishingZone controller, int nStateId)
	{
		mController = controller;
		mState = nStateId;
	}

	protected virtual void HandleYesNo(bool yes)
	{
	}

	protected virtual void HandleOkCancel()
	{
	}

	public virtual void Enter()
	{
		mController.OnTutYesNo += HandleYesNo;
		mController.OkCancelHandler += HandleOkCancel;
		if (mController.pIsTutAvailable)
		{
			ShowTutorial();
		}
	}

	public virtual void Exit()
	{
		mController.OnTutYesNo -= HandleYesNo;
		mController.OkCancelHandler -= HandleOkCancel;
	}

	public virtual void Execute()
	{
	}

	public virtual void OnGUI()
	{
	}

	public virtual void ShowTutorial()
	{
	}
}
