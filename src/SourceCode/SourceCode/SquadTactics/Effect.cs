using UnityEngine;

namespace SquadTactics;

public abstract class Effect : MonoBehaviour
{
	public enum FxPlayType
	{
		ALL,
		NONE,
		IN,
		LOOP,
		OUT
	}

	public enum TickPhase
	{
		BEGIN,
		END
	}

	public int _Duration = 3;

	public int _HitChance = 50;

	public LocaleString _InfoText = new LocaleString("Effect details goes here.");

	public TickPhase _TickPhase;

	protected Character mCreator;

	protected Character mOwner;

	protected StEffectFxInfo mFxInfo;

	public StEffectFxInfo pFxInfo => mFxInfo;

	public virtual Effect Create(Character creator, Character owner)
	{
		mCreator = creator;
		mOwner = owner;
		Effect component = Object.Instantiate(base.gameObject, owner.transform).GetComponent<Effect>();
		component.Initialize(creator, owner);
		component.SetFxData();
		component.Activate();
		return component;
	}

	private void Initialize(Character creator, Character owner)
	{
		mCreator = creator;
		mOwner = owner;
	}

	public abstract void Activate();

	public abstract void SetFxData();

	public virtual void TickChange(TickPhase tickPhase)
	{
		if (tickPhase == _TickPhase)
		{
			_Duration--;
		}
	}

	public virtual void Remove()
	{
		Object.Destroy(base.gameObject, mFxInfo._OutFX.pDuration);
	}

	public bool IsPositive()
	{
		return mCreator.pCharacterData._Team == mOwner.pCharacterData._Team;
	}

	public void PlayFx(FxPlayType playType)
	{
		if (mFxInfo != null)
		{
			FxData fxData = null;
			switch (playType)
			{
			case FxPlayType.IN:
				fxData = mFxInfo._InFX;
				break;
			case FxPlayType.LOOP:
				fxData = mFxInfo._LoopFX;
				break;
			case FxPlayType.OUT:
				fxData = mFxInfo._OutFX;
				break;
			}
			fxData?.PlayFx(mOwner.transform);
		}
	}

	public void StopFx(FxPlayType playType)
	{
		if (mFxInfo == null)
		{
			return;
		}
		if (playType == FxPlayType.ALL)
		{
			mFxInfo._InFX.ResetFX(mOwner.transform);
			mFxInfo._LoopFX.ResetFX(mOwner.transform);
			mFxInfo._OutFX.ResetFX(mOwner.transform);
			return;
		}
		FxData fxData = null;
		switch (playType)
		{
		case FxPlayType.IN:
			fxData = mFxInfo._InFX;
			break;
		case FxPlayType.LOOP:
			fxData = mFxInfo._LoopFX;
			break;
		case FxPlayType.OUT:
			fxData = mFxInfo._OutFX;
			break;
		}
		fxData?.ResetFX(mOwner.transform);
	}
}
