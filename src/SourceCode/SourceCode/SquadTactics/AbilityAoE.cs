using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SquadTactics;

public class AbilityAoE : Ability
{
	public enum AoESource
	{
		OWNER,
		TARGET
	}

	[Header("AoE Settings")]
	public AoESource _Source;

	public float _AoERange = 1.5f;

	[Tooltip("Cleave AoE abilities require range to be valid from both owner and target!")]
	public bool _IsCleave;

	public override IEnumerator Activate(Character owner, Character target)
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
			num = Random.Range(0, 100);
			bool flag = (((float)num < owner.pCharacterData._Stats._CriticalChance.pCurrentValue) ? true : false);
			num2 = (flag ? (num2 * owner.pCharacterData._Stats._CriticalDamageMultiplier.pCurrentValue) : num2);
			num2 = ((owner.pCharacterData._Team == target.pCharacterData._Team) ? num2 : (num2 * -1f));
			List<Character> validAoETargets = GetValidAoETargets(owner, target);
			num = Random.Range(0, 100);
			for (int num3 = validAoETargets.Count - 1; num3 >= 0; num3--)
			{
				bool isCounter = false;
				float num4 = num2;
				if (owner.pCharacterData._Team != target.pCharacterData._Team && GameManager.pInstance.GetElementCounterResult(owner.pCharacterData._WeaponData._ElementType, validAoETargets[num3].pCharacterData._WeaponData._ElementType) == ElementCounterResult.POSITIVE)
				{
					num4 *= GameManager.pInstance._PositiveCounterMultiplier;
					isCounter = true;
				}
				validAoETargets[num3].TakeStatChange(Stat.HEALTH, num4, null, flag, isCounter);
				if (validAoETargets[num3] != null && !validAoETargets[num3].pIsDead && !validAoETargets[num3].pIsIncapacitated)
				{
					Effect[] effects = _Effects;
					foreach (Effect effect in effects)
					{
						if (num < effect._HitChance)
						{
							Effect effect2 = effect.Create(owner, validAoETargets[num3]);
							target.pUiEffects.AddAppliedEffect(effect2);
						}
					}
					validAoETargets[num3].pUiEffects.CheckEffectsStatus();
				}
			}
			if (num2 != 0f)
			{
				yield return new WaitForSeconds(GameManager.pInstance._FloatingTextDuration);
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

	public List<Character> GetValidAoETargets(Character owner, Character target, bool useFinalPathNode = false)
	{
		List<Character> list = new List<Character>(GameManager.pInstance.GetTeamCharacters(target.pCharacterData._Team));
		for (int num = list.Count - 1; num >= 0; num--)
		{
			Node node = null;
			if (!_IsCleave)
			{
				node = ((!useFinalPathNode || _Source != 0) ? ((_Source == AoESource.OWNER) ? owner._CurrentNode : target._CurrentNode) : owner.GetFinalNode());
				if (node == null)
				{
					break;
				}
				if (GameManager.pInstance._Grid.GetDistanceBetweenNodes(node, list[num]._CurrentNode) > _AoERange)
				{
					list.RemoveAt(num);
				}
				else if (!GameManager.pInstance._Grid.HasLineOfSight(node, list[num]._CurrentNode))
				{
					list.RemoveAt(num);
				}
			}
			else
			{
				node = ((!useFinalPathNode) ? owner._CurrentNode : (node = owner.GetFinalNode()));
				if (node == null)
				{
					break;
				}
				if (GameManager.pInstance._Grid.GetDistanceBetweenNodes(node, list[num]._CurrentNode) > _AoERange || GameManager.pInstance._Grid.GetDistanceBetweenNodes(target._CurrentNode, list[num]._CurrentNode) > _AoERange)
				{
					list.RemoveAt(num);
				}
				else if (!GameManager.pInstance._Grid.HasLineOfSight(node, list[num]._CurrentNode) || !GameManager.pInstance._Grid.HasLineOfSight(target._CurrentNode, list[num]._CurrentNode))
				{
					list.RemoveAt(num);
				}
			}
		}
		return list;
	}
}
