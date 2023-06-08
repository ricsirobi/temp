using UnityEngine;

public class SubstanceInfo
{
	public Material _Material;

	public string _Part = string.Empty;

	public string _Property = string.Empty;

	public SubstanceInfo(string inPart, Material inMat, string inProperty)
	{
		_Part = inPart;
		_Material = inMat;
		_Property = inProperty;
	}

	public void OnTextureLoaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inFile, object inUserData)
	{
		if (inEvent == RsResourceLoadEvent.COMPLETE)
		{
			Texture2D texture2D = (Texture2D)inFile;
			if (_Material != null)
			{
				texture2D.wrapMode = TextureWrapMode.Clamp;
				_Material.SetTexture(_Property, texture2D);
			}
		}
	}
}
