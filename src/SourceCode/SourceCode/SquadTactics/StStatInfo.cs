using System;
using UnityEngine;

namespace SquadTactics;

[Serializable]
public class StStatInfo
{
	public string _StatName;

	public Stat _Stat;

	public int _StatID;

	public bool _UseRoundedValues;

	public LocaleString _DisplayText;

	public string _Icon;

	public LocaleString _AbbreviationText = new LocaleString("Identity");

	public Color _Color = Color.red;

	public StEffectFxInfo _Fx;

	public bool _Display;

	[Header("Level Multiplier")]
	public string _FieldToModify;

	public float _Value;
}
