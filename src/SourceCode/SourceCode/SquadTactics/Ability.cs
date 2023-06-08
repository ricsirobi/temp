using System.Collections;
using UnityEngine;

namespace SquadTactics;

public class Ability : MonoBehaviour
{
	public enum Team
	{
		SAME,
		OPPOSITE
	}

	public delegate void AbilityUsedEvent();

	public LocaleString _Name = new LocaleString("Ability name goes here");

	public LocaleString _InfoText = new LocaleString("Ability details goes here.");

	public Material _Unselected;

	public Material _Selected;

	public Stat _InfluencingStat;

	public int _BaseAmount = 50;

	public float _InfluencingStatMultiplier;

	public Effect[] _Effects;

	public float _Range = 1f;

	public int _Cooldown = 1;

	public int _Priority;

	public FxAbilityData[] _FX;

	public Team _TargetType;

	protected int mCurrentCooldown;

	public int pCurrentCooldown
	{
		get
		{
			return mCurrentCooldown;
		}
		set
		{
			mCurrentCooldown = value;
		}
	}

	public event AbilityUsedEvent OnAbilityUsed;

	protected void FireAbilityEvent()
	{
		if (this.OnAbilityUsed != null)
		{
			this.OnAbilityUsed();
		}
	}

	public virtual IEnumerator Activate(Character owner, Character target)
	{
		if (mCurrentCooldown > 0)
		{
			yield break;
		}
		mCurrentCooldown = _Cooldown;
		FireAbilityEvent();
		if (target.pCharacterData._Team == Character.Team.ENEMY && !target.pAlerted)
		{
			if (GameManager.pInstance._FloatingTextPrefab != null)
			{
				UiFloatingTip component = Object.Instantiate(GameManager.pInstance._FloatingTextPrefab, target.transform.position + new Vector3(0f, 2.5f, 0f), Quaternion.identity).GetComponent<UiFloatingTip>();
				if (component != null)
				{
					component.Initialize(GameManager.pInstance._AlertedText.GetLocalizedString(), Color.red);
				}
			}
			if (target.pUiGridInfo != null)
			{
				target.pUiGridInfo.ShowAlertWidget(active: true);
			}
			target.pAlerted = true;
		}
		int num = ((target.pCharacterData._Team != Character.Team.INANIMATE) ? Random.Range(0, 100) : 0);
		if (owner.pCharacterData._Team == target.pCharacterData._Team || ((bool)owner.pActiveStatusEffects.Find((Effect p) => p is Hide) && target.pCharacterData._Stats._DodgeChance._BaseValue != 100f) || target.pIsStunned || target.pCharacterData._Stats._DodgeChance.pCurrentValue <= (float)num)
		{
			float num2 = owner.pCharacterData._Stats.GetStatValue(_InfluencingStat) * _InfluencingStatMultiplier + (float)_BaseAmount;
			bool isCounter = false;
			if (owner.pCharacterData._Team != target.pCharacterData._Team && GameManager.pInstance.GetElementCounterResult(owner.pCharacterData._WeaponData._ElementType, target.pCharacterData._WeaponData._ElementType) == ElementCounterResult.POSITIVE)
			{
				num2 *= GameManager.pInstance._PositiveCounterMultiplier;
				isCounter = true;
			}
			num = Random.Range(0, 100);
			bool flag = (((float)num < owner.pCharacterData._Stats._CriticalChance.pCurrentValue) ? true : false);
			num2 = (flag ? (num2 * owner.pCharacterData._Stats._CriticalDamageMultiplier.pCurrentValue) : num2);
			num2 = ((owner.pCharacterData._Team == target.pCharacterData._Team) ? num2 : (num2 * -1f));
			target.TakeStatChange(Stat.HEALTH, num2, null, flag, isCounter);
			if (num2 != 0f)
			{
				yield return new WaitForSeconds(GameManager.pInstance._FloatingTextDuration);
			}
			if (target != null && !target.pIsDead && !target.pIsIncapacitated && target.pCharacterData._Team != Character.Team.INANIMATE)
			{
				num = Random.Range(0, 100);
				Effect[] effects = _Effects;
				foreach (Effect effect in effects)
				{
					if (num < effect._HitChance)
					{
						Effect effect2 = effect.Create(owner, target);
						target.pUiEffects.AddAppliedEffect(effect2);
					}
				}
				target.pUiEffects.CheckEffectsStatus();
			}
			if (owner.pCharacterData._Team != target.pCharacterData._Team && (bool)owner.pActiveStatusEffects.Find((Effect p) => p is Hide))
			{
				owner.RemoveStatusEffect(owner.pActiveStatusEffects.Find((Effect p) => p is Hide));
				GameManager.pInstance.CheckEnemyAlertStatus();
			}
		}
		else
		{
			StartCoroutine(target.DodgeEffect());
		}
	}
}
