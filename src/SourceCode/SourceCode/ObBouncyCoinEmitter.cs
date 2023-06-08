using UnityEngine;

public class ObBouncyCoinEmitter : MonoBehaviour
{
	public GameObject _Coin;

	public GameObject _CollectMessageObject;

	public ObCoinEmitterRange _CoinsToEmit;

	public bool _SubtractFromWallet;

	public float _IntervalBetweenCoins = 0.1f;

	public float _DelayBeforeFirstCoin = 0.5f;

	public Vector3 _SpawnOffset = new Vector3(0f, 2f, 0f);

	protected int mCoinsToEmit;

	protected float mCoinEmitTimer = 0.1f;

	protected float mStartDelayTimer;

	protected virtual GameObject InstantiateCoin()
	{
		return Object.Instantiate(_Coin);
	}

	public virtual void Update()
	{
		if (mCoinsToEmit <= 0)
		{
			return;
		}
		mStartDelayTimer -= Time.deltaTime;
		if (!(mStartDelayTimer <= 0f))
		{
			return;
		}
		mCoinEmitTimer -= Time.deltaTime;
		if (mCoinEmitTimer <= 0f)
		{
			GameObject obj = InstantiateCoin();
			obj.name = _Coin.name + mCoinsToEmit;
			obj.transform.position = base.transform.position + _SpawnOffset;
			float y = Random.Range(-180f, 180f);
			obj.transform.eulerAngles = base.transform.eulerAngles + new Vector3(0f, y, 0f);
			ObCollect componentInChildren = obj.GetComponentInChildren<ObCollect>();
			if (componentInChildren != null && _CollectMessageObject != null)
			{
				componentInChildren._MessageObject = _CollectMessageObject;
			}
			ObBouncyCoin component = obj.GetComponent<ObBouncyCoin>();
			if (component != null)
			{
				float x = Random.Range(-1.5f, 1.5f);
				float z = Random.Range(-1.5f, 1.5f);
				component._ExplosionPositionOffset = new Vector3(x, 0f, z);
				component.StartExplosion();
			}
			mCoinEmitTimer = _IntervalBetweenCoins;
			mCoinsToEmit--;
		}
	}

	public void GenerateCoins()
	{
		if (!(_Coin != null))
		{
			return;
		}
		mCoinsToEmit = Random.Range(_CoinsToEmit._Min, _CoinsToEmit._Max + 1);
		if (_SubtractFromWallet)
		{
			if (Money.pGameCurrency < mCoinsToEmit)
			{
				mCoinsToEmit = Money.pGameCurrency;
			}
			Money.AddMoney(-mCoinsToEmit, bForceUpdate: false);
		}
		mStartDelayTimer = _DelayBeforeFirstCoin;
		mCoinEmitTimer = 0f;
	}
}
