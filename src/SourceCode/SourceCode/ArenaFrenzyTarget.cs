using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ArenaFrenzyTarget : MonoBehaviour
{
	public enum ArenaFrenzyTargetType
	{
		TARGET_TEAM_1 = 1,
		TARGET_TEAM_2,
		TARGET_NEUTRAL
	}

	public int[] _TeamScores;

	public float _RespawnTime = 2f;

	public ArenaFrenzyTargetType _TargetType = ArenaFrenzyTargetType.TARGET_NEUTRAL;

	protected ArenaFrenzyMapElement mParentMapElement;

	private int mTargetID = -1;

	public ArenaFrenzyMapElement pParentMapElement => mParentMapElement;

	public int pTargetID
	{
		get
		{
			return mTargetID;
		}
		set
		{
			mTargetID = value;
		}
	}

	public void OnTriggerEnter(Collider other)
	{
		ProcessTargetHit(other);
	}

	public void SetParentMapElement(ArenaFrenzyMapElement inParent)
	{
		mParentMapElement = inParent;
	}

	protected virtual bool ProcessTargetHit(Collider inOther)
	{
		return true;
	}

	protected virtual void PlayHitAnim()
	{
	}
}
