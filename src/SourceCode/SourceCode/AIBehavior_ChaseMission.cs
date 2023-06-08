public class AIBehavior_ChaseMission : AIBehavior_Mission
{
	private bool mStartedChase;

	public override void OnStart(AIActor Actor)
	{
		base.OnStart(Actor);
		Invoke("StartChase", mMissionData.GraceTime);
	}

	public void StartChase()
	{
		mStartedChase = true;
	}

	public override void ProcessMove(AIActor Actor)
	{
		if (mStartedChase)
		{
			base.ProcessMove(Actor);
		}
	}

	protected override bool CanMove(AIActor Actor)
	{
		return mStartedChase;
	}

	public override void InProximity(AIActor Actor)
	{
	}

	public override void OutOfProximity(AIActor Actor)
	{
	}

	public override void EndBehavior()
	{
		TeleportToStartpoint();
		mStartedChase = false;
		base.EndBehavior();
	}
}
