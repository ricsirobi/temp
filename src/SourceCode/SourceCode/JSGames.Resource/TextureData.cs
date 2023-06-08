using UnityEngine;

namespace JSGames.Resource;

public class TextureData : AssetData
{
	public Texture _Texture;

	public override void Init(string url)
	{
		_Texture = null;
		base.Init(url);
	}

	protected override void LoadContent(object obj)
	{
		if (obj != null)
		{
			_Texture = obj as Texture;
			if (_Texture != null)
			{
				_Texture.name = _ResName;
			}
			else
			{
				UtDebug.LogError("Obj is not convertible to Texture");
			}
		}
		else
		{
			UtDebug.LogError("Loaded object is null");
		}
	}
}
