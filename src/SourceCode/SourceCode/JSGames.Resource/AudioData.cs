using UnityEngine;

namespace JSGames.Resource;

public class AudioData : AssetData
{
	public AudioClip _Clip;

	public override void Init(string resName)
	{
		_Clip = null;
		base.Init(resName);
	}

	protected override void LoadContent(object obj)
	{
		if (obj != null)
		{
			_Clip = obj as AudioClip;
			if (_Clip != null)
			{
				_Clip.name = _ResName;
			}
			else
			{
				UtDebug.LogError("Obj is not convertible to AudioClip");
			}
		}
		else
		{
			UtDebug.LogError("Loaded object is null");
		}
	}
}
