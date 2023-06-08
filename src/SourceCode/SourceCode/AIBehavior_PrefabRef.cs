using UnityEngine;

public class AIBehavior_PrefabRef : AIBehavior
{
	public string _PrefabName = "AI_Root_Pet";

	private AIBehavior mChild;

	private bool mLoaded;

	public override void OnStart(AIActor Actor)
	{
		base.OnStart(Actor);
		LoadPrefab();
	}

	public override AIBehaviorState Think(AIActor Actor)
	{
		LoadPrefab();
		if (mChild != null)
		{
			return SetState(mChild.Think(Actor));
		}
		return SetState(AIBehaviorState.ACTIVE);
	}

	private void LoadPrefab()
	{
		if (!mLoaded)
		{
			GameObject gameObject = Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources(_PrefabName));
			gameObject.transform.parent = base.transform;
			mChild = gameObject.GetComponentInChildren<AIBehavior>();
			mLoaded = true;
		}
	}
}
