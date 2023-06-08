using UnityEngine;

public class CustomInfo
{
	public Material _Material;

	public string _Part = string.Empty;

	public string _Property = string.Empty;

	public CustomInfo(string inPart, Material inMat, string inProperty)
	{
		_Part = inPart;
		_Material = inMat;
		_Property = inProperty;
	}

	public void OnTextureLoaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inFile, object inUserData)
	{
		if (inEvent == RsResourceLoadEvent.COMPLETE && !(_Material == null) && inFile != null)
		{
			Texture2D texture2D = (Texture2D)inFile;
			texture2D.wrapMode = TextureWrapMode.Clamp;
			if (_Material.HasProperty(_Property))
			{
				_Material.SetTexture(_Property, texture2D);
			}
			else if (_Material.HasProperty("_MainTex"))
			{
				_Material.SetTexture("_MainTex", texture2D);
			}
		}
	}
}
