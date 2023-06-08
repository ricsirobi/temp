using UnityEngine;

public class ItemAudioResData : ItemResNameData
{
	public AudioClip _Clip;

	public override void Init(string resName)
	{
		_Clip = null;
		base.Init(resName);
	}

	public override Object LoadRes(AssetBundle bd)
	{
		_Clip = CoBundleLoader.LoadAudioClip(bd, _ResName);
		return _Clip;
	}
}
