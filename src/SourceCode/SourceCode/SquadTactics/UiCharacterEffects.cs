using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SquadTactics;

public class UiCharacterEffects : MonoBehaviour
{
	public KAWidget _ArrowWidget;

	public KAWidget _EffectWidget;

	public float _RemovalFlashTimer = 0.2f;

	public Vector3 _MovementSpeed = new Vector3(0f, 1f, 0f);

	private List<Effect> mEffectsToApply = new List<Effect>();

	private List<Effect> mEffectsToRemove = new List<Effect>();

	private Character mStCharacter;

	private Vector3 mOriginalPosition;

	private IEnumerator mCycleCoroutine;

	private void Start()
	{
		mOriginalPosition = base.transform.localPosition;
		mStCharacter = base.gameObject.GetComponentInParent<Character>();
	}

	public void AddAppliedEffect(Effect effect)
	{
		mEffectsToApply.Add(effect);
	}

	public void AddRemovedEffect(Effect effect)
	{
		mEffectsToRemove.Add(effect);
	}

	private void ShowEffect(Effect effect)
	{
		SetArrowSprite(effect.IsPositive());
		SetEffectSprite(effect);
		SetVisibility(isVisible: true);
	}

	private void SetVisibility(bool isVisible)
	{
		if (_ArrowWidget != null)
		{
			_ArrowWidget.SetVisibility(isVisible);
		}
		if (_EffectWidget != null)
		{
			_EffectWidget.SetVisibility(isVisible);
		}
	}

	private void SetArrowSprite(bool isPositive)
	{
		if (!(_ArrowWidget == null))
		{
			string animName = (isPositive ? GameManager.pInstance._HUD._PositiveArrowIcon : GameManager.pInstance._HUD._NegativeArrowIcon);
			_ArrowWidget.pAnim2D.Play(animName);
		}
	}

	private void SetEffectSprite(Effect effect)
	{
		if (!(_EffectWidget == null))
		{
			string sprite = string.Empty;
			if (effect is Tick)
			{
				Tick tick = (Tick)effect;
				sprite = Settings.pInstance.GetEffectIcon(tick._AppliedEffect);
			}
			else if (effect is Buff)
			{
				Buff buff = (Buff)effect;
				sprite = Settings.pInstance.GetStatEffectIcon(buff._AffectedStat);
			}
			else if (effect is CrowdControl)
			{
				CrowdControl crowdControl = (CrowdControl)effect;
				sprite = Settings.pInstance.GetEffectIcon(crowdControl._AppliedEffect);
			}
			else if (effect is Hide)
			{
				Hide hide = (Hide)effect;
				sprite = Settings.pInstance.GetEffectIcon(hide._AppliedEffect);
			}
			else if (effect is Taunt)
			{
				Taunt taunt = (Taunt)effect;
				sprite = Settings.pInstance.GetEffectIcon(taunt._AppliedEffect);
			}
			_EffectWidget.SetSprite(sprite);
		}
	}

	private IEnumerator ShowAppliedEffect(Effect effect)
	{
		StopCycling();
		ShowEffect(effect);
		effect.PlayFx(Effect.FxPlayType.IN);
		yield return new WaitForSeconds(effect.pFxInfo._InFX.pDuration);
		effect.StopFx(Effect.FxPlayType.IN);
		mEffectsToApply.Remove(effect);
		CheckEffectsStatus();
	}

	private IEnumerator ShowRemovedEffect(Effect effect)
	{
		StopCycling();
		effect.PlayFx(Effect.FxPlayType.OUT);
		ShowEffect(effect);
		bool isVisible = true;
		float timer = effect.pFxInfo._OutFX.pDuration;
		float flashTimer = _RemovalFlashTimer;
		while (timer > 0f)
		{
			base.transform.position += _MovementSpeed * Time.deltaTime;
			timer -= Time.deltaTime;
			flashTimer -= Time.deltaTime;
			if (flashTimer <= 0f)
			{
				isVisible = !isVisible;
				SetVisibility(isVisible);
				flashTimer = _RemovalFlashTimer;
			}
			yield return null;
		}
		effect.StopFx(Effect.FxPlayType.OUT);
		SetVisibility(isVisible: false);
		base.transform.localPosition = mOriginalPosition;
		mEffectsToRemove.Remove(effect);
		CheckEffectsStatus();
	}

	private IEnumerator CycleEffects()
	{
		foreach (Effect effect in mStCharacter.pActiveStatusEffects)
		{
			effect.PlayFx(Effect.FxPlayType.LOOP);
			ShowEffect(effect);
			yield return new WaitForSeconds(effect.pFxInfo._LoopFX.pDuration);
			if (!effect.pFxInfo._LoopFX._AlwaysShow)
			{
				effect.StopFx(Effect.FxPlayType.LOOP);
			}
		}
		mCycleCoroutine = null;
		CheckEffectsStatus();
	}

	public void CheckEffectsStatus()
	{
		if (mStCharacter == null)
		{
			return;
		}
		if (mEffectsToApply.Count > 0)
		{
			StopCycling();
			StartCoroutine(ShowAppliedEffect(mEffectsToApply[0]));
		}
		else if (mEffectsToRemove.Count > 0)
		{
			StopCycling();
			StartCoroutine(ShowRemovedEffect(mEffectsToRemove[0]));
		}
		else if (mStCharacter.pActiveStatusEffects.Count <= 0)
		{
			SetVisibility(isVisible: false);
			if (mCycleCoroutine != null)
			{
				StopCoroutine(mCycleCoroutine);
			}
		}
		else
		{
			StopCycling();
			mCycleCoroutine = CycleEffects();
			StartCoroutine(mCycleCoroutine);
		}
	}

	private void StopCycling()
	{
		if (mCycleCoroutine != null)
		{
			StopCoroutine(mCycleCoroutine);
		}
	}

	public void Stop()
	{
		if (mCycleCoroutine != null)
		{
			StopCoroutine(mCycleCoroutine);
		}
		SetVisibility(isVisible: false);
	}
}
