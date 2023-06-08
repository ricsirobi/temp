using UnityEngine;

public class ComboTileBase : ElementMatchTilePiece
{
	public LocaleString _CompoundInfo = new LocaleString(string.Empty);

	private bool mAnimateOnSpawn;

	public GameObject _SpawnEffect;

	protected Vector3 mOrgEulerAngle;

	private float mCachedSpeed = 30f;

	private GameObject mEffectGO;

	public bool pAnimateOnSpawn
	{
		get
		{
			return mAnimateOnSpawn;
		}
		set
		{
			mAnimateOnSpawn = value;
		}
	}

	protected void Init()
	{
		if (mAnimateOnSpawn && _SpawnEffect != null)
		{
			mOrgEulerAngle = base.transform.localEulerAngles;
			mEffectGO = Object.Instantiate(_SpawnEffect);
			mEffectGO.transform.position = TileMatchPuzzleGame.pInstance.GetPosition(base.pRow, base.pColumn);
			mEffectGO.SetActive(value: true);
			mCachedSpeed = mMoveSpeed;
		}
	}

	protected void DisableSpawnAnimation()
	{
		if (mEffectGO != null)
		{
			mMoveSpeed = mCachedSpeed;
			base.transform.localEulerAngles = mOrgEulerAngle;
			Object.Destroy(mEffectGO);
		}
	}
}
