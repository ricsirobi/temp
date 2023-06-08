using UnityEngine;

public class ItemTextureResData : ItemResNameData
{
	public Texture _Texture;

	public override void Init(string resName)
	{
		_Texture = null;
		base.Init(resName);
	}

	public override Object LoadRes(AssetBundle bd)
	{
		_Texture = CoBundleLoader.LoadTexture(bd, _ResName);
		if (_Texture != null)
		{
			_Texture.name = _ResBundleName + "/" + _ResName;
		}
		return _Texture;
	}
}
