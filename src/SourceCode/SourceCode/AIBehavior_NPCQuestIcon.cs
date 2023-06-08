using UnityEngine;

public class AIBehavior_NPCQuestIcon : AIBehavior
{
	public Texture _Icon;

	protected Texture mPreviousIcon;

	protected bool mActiveBefore;

	protected GameObject mQuestIcon;

	public override void OnStart(AIActor Actor)
	{
		Transform transform = Actor.transform.Find("QuestIcon");
		mQuestIcon = ((transform != null) ? transform.gameObject : null);
		if (mQuestIcon != null)
		{
			Renderer component = mQuestIcon.GetComponent<Renderer>();
			mPreviousIcon = component.material.mainTexture;
			mActiveBefore = mQuestIcon.activeSelf;
			component.material.mainTexture = _Icon;
			mQuestIcon.SetActive(value: true);
		}
		base.OnStart(Actor);
	}

	public override AIBehaviorState Think(AIActor Actor)
	{
		return SetState(AIBehaviorState.ACTIVE);
	}

	public override void OnTerminate(AIActor Actor)
	{
		if (mQuestIcon != null)
		{
			mQuestIcon.GetComponent<Renderer>().material.mainTexture = mPreviousIcon;
			mQuestIcon.SetActive(mActiveBefore);
		}
		base.OnTerminate(Actor);
	}
}
