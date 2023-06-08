using UnityEngine;

namespace SquadTactics;

public class Settings : MonoBehaviour
{
	public StStatInfo[] _StatInfo;

	public EffectData[] _Effects;

	public ElementInfo[] _ElementInfo;

	public ElementCounterData[] _ElementCounterData;

	private string[] mEffectNames;

	private int mCachedLength;

	private static Settings mInstance;

	public static Settings pInstance
	{
		get
		{
			if (mInstance == null)
			{
				mInstance = (RsResourceManager.LoadAssetFromResources("PfSTSettings") as GameObject).GetComponent<Settings>();
			}
			return mInstance;
		}
	}

	public bool GetAttributeIsRounded(Stat stat)
	{
		StStatInfo[] statInfo = _StatInfo;
		foreach (StStatInfo stStatInfo in statInfo)
		{
			if (stStatInfo._Stat == stat)
			{
				return stStatInfo._UseRoundedValues;
			}
		}
		return false;
	}

	public string GetStatEffectName(Stat stat)
	{
		StStatInfo[] statInfo = _StatInfo;
		foreach (StStatInfo stStatInfo in statInfo)
		{
			if (stStatInfo._Stat == stat && !string.IsNullOrEmpty(stStatInfo._DisplayText.GetLocalizedString()))
			{
				return stStatInfo._DisplayText.GetLocalizedString();
			}
		}
		return stat.ToString();
	}

	public string GetStatEffectName(int statID)
	{
		StStatInfo[] statInfo = _StatInfo;
		foreach (StStatInfo stStatInfo in statInfo)
		{
			if (stStatInfo._StatID == statID && !string.IsNullOrEmpty(stStatInfo._DisplayText.GetLocalizedString()))
			{
				return stStatInfo._DisplayText.GetLocalizedString();
			}
		}
		UtDebug.LogError("No Stat Found!");
		return null;
	}

	public string GetStatAbbreviation(int statID)
	{
		StStatInfo[] statInfo = _StatInfo;
		foreach (StStatInfo stStatInfo in statInfo)
		{
			if (stStatInfo._StatID == statID && !string.IsNullOrEmpty(stStatInfo._AbbreviationText.GetLocalizedString()))
			{
				return stStatInfo._AbbreviationText.GetLocalizedString();
			}
		}
		UtDebug.LogError("No Stat Found!");
		return null;
	}

	public string GetStatEffectIcon(Stat stat)
	{
		StStatInfo[] statInfo = _StatInfo;
		foreach (StStatInfo stStatInfo in statInfo)
		{
			if (stStatInfo._Stat == stat && !string.IsNullOrEmpty(stStatInfo._Icon))
			{
				return stStatInfo._Icon;
			}
		}
		return stat.ToString();
	}

	public string GetEffectName(EffectName effect)
	{
		EffectData[] effects = _Effects;
		foreach (EffectData effectData in effects)
		{
			if (effect._Name == effectData._Name && !string.IsNullOrEmpty(effectData._DisplayText.GetLocalizedString()))
			{
				return effectData._DisplayText.GetLocalizedString();
			}
		}
		return effect._Name;
	}

	public string GetEffectIcon(EffectName effect)
	{
		EffectData[] effects = _Effects;
		foreach (EffectData effectData in effects)
		{
			if (effect._Name == effectData._Name && !string.IsNullOrEmpty(effectData._Icon))
			{
				return effectData._Icon;
			}
		}
		return effect._Name;
	}

	public string[] GetEffectNames()
	{
		if ((_Effects != null && mEffectNames == null) || mCachedLength != _Effects.Length)
		{
			mEffectNames = new string[_Effects.Length];
			for (int i = 0; i < _Effects.Length; i++)
			{
				mEffectNames[i] = _Effects[i]._Name;
			}
		}
		return mEffectNames;
	}

	public int FindEffectIndex(string effectName)
	{
		if (_Effects != null)
		{
			for (int i = 0; i < _Effects.Length; i++)
			{
				if (_Effects[i]._Name == effectName)
				{
					return i;
				}
			}
		}
		return 0;
	}

	public StStatInfo GetStatInfoByName(Stat stat)
	{
		StStatInfo[] statInfo = _StatInfo;
		foreach (StStatInfo stStatInfo in statInfo)
		{
			if (stStatInfo._Stat == stat)
			{
				return stStatInfo;
			}
		}
		return null;
	}

	public StStatInfo GetStatInfoByID(int statID)
	{
		StStatInfo[] statInfo = _StatInfo;
		foreach (StStatInfo stStatInfo in statInfo)
		{
			if (stStatInfo._StatID == statID)
			{
				return stStatInfo;
			}
		}
		return null;
	}

	public EffectData GetEffectDataByName(string name)
	{
		EffectData[] effects = _Effects;
		foreach (EffectData effectData in effects)
		{
			if (name == effectData._Name)
			{
				return effectData;
			}
		}
		return null;
	}

	public EffectData GetEffectDataByEffect(EffectName effect)
	{
		return GetEffectDataByName(effect._Name);
	}

	public StEffectFxInfo GetStatFxData(Stat stat)
	{
		return GetStatInfoByName(stat)._Fx;
	}

	public StEffectFxInfo GetEffectFxData(EffectName effect)
	{
		if (string.IsNullOrEmpty(effect._Name))
		{
			return null;
		}
		return GetEffectDataByEffect(effect)._FX;
	}

	public ElementInfo GetElementInfo(ElementType element)
	{
		ElementInfo[] elementInfo = _ElementInfo;
		foreach (ElementInfo elementInfo2 in elementInfo)
		{
			if (elementInfo2._Element == element)
			{
				return elementInfo2;
			}
		}
		return null;
	}

	public ElementCounter[] GetElementCounterData(string element)
	{
		if (_ElementCounterData != null)
		{
			for (int i = 0; i < _ElementCounterData.Length; i++)
			{
				for (int j = 0; j < _ElementCounterData[i]._Counters.Length; j++)
				{
					if (_ElementCounterData[i]._Counters[j]._Element.ToString() == element)
					{
						return _ElementCounterData[i]._Counters;
					}
				}
			}
		}
		return null;
	}

	public ElementType GetCounteredElement(ElementType element)
	{
		if (_ElementCounterData != null)
		{
			for (int i = 0; i < _ElementCounterData.Length; i++)
			{
				for (int j = 0; j < _ElementCounterData[i]._Counters.Length; j++)
				{
					if (_ElementCounterData[i]._Counters[j]._Element == element)
					{
						return _ElementCounterData[i]._Counters[j]._CounteredElement;
					}
				}
			}
		}
		return ElementType.NONE;
	}
}
