using UnityEngine;

public class ExplodingTile : ComboTileBase
{
	[SerializeField]
	private int _BlastRadius = 1;

	[SerializeField]
	private GameObject _Effect;

	private bool mPlayedCreateEffect;

	public override int[] pExplodeRange => new int[4]
	{
		Mathf.Max(base.pRow - _BlastRadius, 0),
		Mathf.Max(base.pColumn - _BlastRadius, 0),
		base.pRow + _BlastRadius,
		base.pColumn + _BlastRadius
	};

	public override GameObject pEffect => _Effect;

	protected void Start()
	{
		_Type = TileType.EXPLODE;
		if (mState == State.NONE)
		{
			SetState(State.IDLE);
		}
	}

	public override void OnStateChange(State inNewState, State inPrvState)
	{
		if (inNewState == State.IDLE && inPrvState == State.MOVE_FALL && !mPlayedCreateEffect)
		{
			ElementEffects.pInstance.PlayCompoundCreationFX(base.transform.position);
			mPlayedCreateEffect = true;
		}
	}
}
