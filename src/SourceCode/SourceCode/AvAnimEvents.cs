using System.Collections;

public class AvAnimEvents : KAMonoBase
{
	public AvAvatarAnimEvent[] _Events;

	public void Animate(AvAvatarAnimation inSettings)
	{
		if (!(base.animation[inSettings.mName] == null))
		{
			StartAnimationEventProcessing(inSettings.mName);
		}
	}

	private void StopAnimatingEventProcessing()
	{
		StopCoroutine("ProcessAnimationEvents");
	}

	protected void StartAnimationEventProcessing(string inAnimName)
	{
		AvAvatarAnimEvent[] events = _Events;
		foreach (AvAvatarAnimEvent avAvatarAnimEvent in events)
		{
			if (avAvatarAnimEvent._Target != null && avAvatarAnimEvent._Animation == inAnimName)
			{
				StopCoroutine("ProcessAnimationEvents");
				StartCoroutine("ProcessAnimationEvents", avAvatarAnimEvent);
				break;
			}
		}
	}

	public IEnumerator ProcessAnimationEvents(AvAvatarAnimEvent inEvent)
	{
		AnimData[] times = inEvent._Times;
		foreach (AnimData evTime in times)
		{
			while (evTime._Time > base.animation[inEvent._Animation].time && base.animation.IsPlaying(inEvent._Animation))
			{
				yield return 0;
			}
			if (inEvent._Target != null)
			{
				inEvent.mData = evTime;
				inEvent._Target.SendMessage(inEvent._Function, inEvent);
			}
		}
	}
}
