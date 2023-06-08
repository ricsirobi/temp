using UnityEngine;

public class ExplodeDirectionTile : ComboTileBase
{
	public enum ExlodeDirection
	{
		ROW,
		COLUMN
	}

	[SerializeField]
	private GameObject _Effect;

	public ExlodeDirection _Direction;

	private bool mPlayedCreateEffect;

	public override int[] pExplodeRange
	{
		get
		{
			int[] array = new int[4];
			if (_Direction == ExlodeDirection.ROW)
			{
				array[0] = base.pRow;
				array[1] = 0;
				array[2] = base.pRow;
				array[3] = int.MaxValue;
			}
			else if (ExlodeDirection.COLUMN == _Direction)
			{
				array[0] = 0;
				array[1] = base.pColumn;
				array[2] = int.MaxValue;
				array[3] = base.pColumn;
			}
			return array;
		}
	}

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
