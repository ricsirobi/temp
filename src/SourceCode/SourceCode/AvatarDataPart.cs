using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

[Serializable]
[XmlRoot(ElementName = "AvatarDataPart", Namespace = "")]
public class AvatarDataPart
{
	public string PartType;

	[XmlArrayItem("Offset")]
	public AvatarDataPartOffset[] Offsets;

	[XmlArrayItem("Geometry")]
	public string[] Geometries;

	[XmlArrayItem("Texture")]
	public string[] Textures;

	[XmlArrayItem("Attribute")]
	public AvatarPartAttribute[] Attributes;

	[XmlElement(ElementName = "Uiid", IsNullable = true)]
	public int? UserInventoryId;

	public const string SAVED_DEFAULT_PREFIX = "DEFAULT_";

	public const string PLACEHOLDER = "PLACEHOLDER";

	public static bool IsResourceValid(string resName)
	{
		if (!string.IsNullOrEmpty(resName) && resName != "__EMPTY__" && resName != "PLACEHOLDER" && !resName.StartsWith("http"))
		{
			return !resName.Equals("NULL", StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}

	public AvatarDataPart()
	{
	}

	public AvatarDataPart(string inPartType)
	{
		PartType = inPartType;
		if (inPartType == "Version")
		{
			Offsets = new AvatarDataPartOffset[3];
			return;
		}
		Geometries = new string[1];
		Geometries[0] = "__EMPTY__";
		Textures = new string[1];
		Textures[0] = "__EMPTY__";
	}

	public bool IsDefault()
	{
		return PartType.Contains("DEFAULT_");
	}

	public bool IsPlaceHolder()
	{
		if (Geometries != null && Geometries.Length != 0)
		{
			return Geometries[0] == "PLACEHOLDER";
		}
		return false;
	}

	public void SetVersion(Vector3 inVersion)
	{
		Offsets = new AvatarDataPartOffset[1];
		Offsets[0] = new AvatarDataPartOffset();
		Offsets[0].X = (int)inVersion.x;
		Offsets[0].Y = (int)inVersion.y;
		Offsets[0].Z = (int)inVersion.z;
	}

	public Vector3 GetVersion()
	{
		if (Offsets != null && Offsets.Length != 0)
		{
			return new Vector3(Offsets[0].X, Offsets[0].Y, Offsets[0].Z);
		}
		return Vector3.zero;
	}

	public int GetPartScaleIdx()
	{
		if (!(PartType == AvatarData.pPartSettings.AVATAR_PART_HEAD))
		{
			return 0;
		}
		return 2;
	}

	public void AddBundles(List<string> assetsToDownload)
	{
		if (PartType == "Version")
		{
			return;
		}
		if (Geometries != null)
		{
			for (int i = 0; i < Geometries.Length; i++)
			{
				string text = Geometries[i];
				if (IsResourceValid(text) && !assetsToDownload.Contains(text))
				{
					assetsToDownload.Add(text);
				}
			}
		}
		if (Textures == null)
		{
			return;
		}
		for (int j = 0; j < Textures.Length; j++)
		{
			string text2 = Textures[j];
			if (IsResourceValid(text2) && !assetsToDownload.Contains(text2))
			{
				assetsToDownload.Add(text2);
			}
		}
	}
}
