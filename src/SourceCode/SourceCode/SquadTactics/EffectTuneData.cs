using UnityEngine;

namespace SquadTactics;

public class EffectTuneData : MonoBehaviour
{
	public EffectData[] _Effects;

	private string[] mEffectNames;

	private int mCachedLength;

	private static EffectTuneData mInstance;

	public static EffectTuneData pInstance
	{
		get
		{
			if (mInstance == null)
			{
				mInstance = (Resources.Load("PfSTSettings") as GameObject).GetComponent<EffectTuneData>();
			}
			return mInstance;
		}
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
}
