using System;

[Serializable]
public class AvAvatarStats
{
	public float _CurrentHealth;

	public float _MaxHealth;

	public float _HealthRegenRate = 1f;

	public float _CurrentEnergy;

	public float _MaxEnergy;

	public float _EnergyRegenRate;

	public float _AutoAttackSpeed;

	public float _AutoAttackRange;

	public int _AttackDamageMin;

	public int _AttackDamageMax;

	public int _Strength;

	public int _Endurance;

	public int _Toughness;

	public int _Dexterity;

	public int _Intelligence;

	public int _Will;

	public float _CurrentLuck;

	public float _MaxLuck;

	public float _CurrentAir;

	public float _MaxAir = 100f;

	public float _AirUseRate = -0.2f;

	public float _NoAirHealthDamageRate = -0.05f;
}
