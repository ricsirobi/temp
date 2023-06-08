using System;

[Serializable]
public class CustomizationItemData
{
	public string _PartName = string.Empty;

	public string _MainTexture = string.Empty;

	public string _HighlightTexture = string.Empty;

	public string _MaskTexture = string.Empty;

	public string _BumpMapTexture = string.Empty;

	public string _EyeTexture = string.Empty;

	public string _DecalTexture = string.Empty;

	public string GetTextureName(string inTypeName)
	{
		string result = string.Empty;
		if (AvatarData.pTextureSettings.TEXTURE_TYPE_STYLE == inTypeName || AvatarData.pTextureSettings.TEXTURE_TYPE_STYLER == inTypeName)
		{
			result = _MainTexture;
		}
		else if (AvatarData.pTextureSettings.TEXTURE_TYPE_EYES_OPEN == inTypeName)
		{
			result = _EyeTexture;
		}
		else if (AvatarData.pTextureSettings.TEXTURE_TYPE_HIGHLIGHT == inTypeName)
		{
			result = _HighlightTexture;
		}
		else if (AvatarData.pTextureSettings.TEXTURE_TYPE_MASK == inTypeName)
		{
			result = _MaskTexture;
		}
		else if (AvatarData.pTextureSettings.TEXTURE_TYPE_BUMP == inTypeName)
		{
			result = _BumpMapTexture;
		}
		else if ("Decal" == inTypeName)
		{
			result = _DecalTexture;
		}
		return result;
	}
}
