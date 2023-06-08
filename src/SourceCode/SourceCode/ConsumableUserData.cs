using System;

public class ConsumableUserData : KAWidgetUserData
{
	public Consumable _Consumable;

	public DateTime? _AcquireTime;

	public float _CoolDown = -1f;

	public DateTime _LastUsedTime = DateTime.MinValue;

	private int mNumOfScrollIterations = -1;

	private int mCurrTextureIndex;

	private KAWidget[] mEffectScrollWidgets;

	public float mTimer;

	public float pCoolDown
	{
		get
		{
			return _CoolDown;
		}
		set
		{
			_CoolDown = value;
		}
	}

	public int pNumOfScrollIterations
	{
		get
		{
			return mNumOfScrollIterations;
		}
		set
		{
			mNumOfScrollIterations = value;
		}
	}

	public int pCurrTextureIndex
	{
		get
		{
			return mCurrTextureIndex;
		}
		set
		{
			mCurrTextureIndex = value;
		}
	}

	public KAWidget[] pEffectScrollWidgets
	{
		get
		{
			return mEffectScrollWidgets;
		}
		set
		{
			mEffectScrollWidgets = value;
		}
	}

	public DateTime pLastUsedTime
	{
		get
		{
			return _LastUsedTime;
		}
		set
		{
			_LastUsedTime = value;
		}
	}

	public ConsumableUserData(Consumable inConsumable)
	{
		_Consumable = inConsumable;
		if (_Consumable._Type.Equals("Game"))
		{
			_AcquireTime = ServerTime.pCurrentTime;
		}
	}

	public void Reset()
	{
		mCurrTextureIndex = 0;
		mNumOfScrollIterations = -1;
		mEffectScrollWidgets = null;
	}
}
