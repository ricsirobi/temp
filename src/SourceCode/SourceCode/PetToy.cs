public class PetToy : KAMonoBase
{
	protected KAUIPetPlaySelect mPlayUI;

	protected AIEvaluator mAIRoot;

	public virtual void Initialize(KAUIPetPlaySelect ui)
	{
		mPlayUI = ui;
		mAIRoot = GetComponentInChildren<AIEvaluator>();
		if (mAIRoot != null)
		{
			SanctuaryManager.pCurPetInstance.AIActor.PushPetPlayGoal(mAIRoot);
		}
	}

	public void DropObject()
	{
		mPlayUI.DropObject(lookatcam: true);
	}

	public void OnDropObject()
	{
		if (!(mAIRoot == null))
		{
			SanctuaryManager.pCurPetInstance.AIActor.RemovePetPlayGoal(mAIRoot);
			mAIRoot.transform.parent = base.transform;
			mAIRoot = null;
		}
	}
}
