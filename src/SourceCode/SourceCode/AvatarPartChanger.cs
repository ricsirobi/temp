using System;

[Serializable]
public class AvatarPartChanger
{
	public string _PrtTypeName = "";

	public bool _SaveDefault;

	public int[] _DefaultStoreIDs;

	public void ApplyGeomWIndex(AvatarData.InstanceInfo inInstance, string resName, int idx)
	{
		string[] array = resName.Split('/');
		if (array.Length == 3)
		{
			if (_SaveDefault)
			{
				AvatarData.SetGeometrySaveDefault(_PrtTypeName, array[1] + "/" + array[2], idx);
			}
			else
			{
				AvatarData.SetGeometry(inInstance, _PrtTypeName, array[1] + "/" + array[2], idx);
			}
		}
		else if (resName.Equals("NULL", StringComparison.OrdinalIgnoreCase))
		{
			if (_SaveDefault)
			{
				AvatarData.SetGeometrySaveDefault(_PrtTypeName, "NULL", idx);
			}
			else
			{
				AvatarData.SetGeometry(inInstance, _PrtTypeName, "NULL", idx);
			}
		}
	}

	public void ApplyGeom(AvatarData.InstanceInfo inInstance, string resName, ItemData iData)
	{
		ApplyGeomWIndex(inInstance, resName, 0);
		if (iData.Geometry2 != null && iData.Geometry2.Length > 0)
		{
			ApplyGeomWIndex(inInstance, iData.Geometry2, 1);
		}
	}

	public void ApplyPreset(AvatarData.InstanceInfo inInstance, ItemData iData)
	{
		string text = null;
		text = iData.GetTextureNameNoPath(AvatarData.pTextureSettings.TEXTURE_TYPE_EYES_OPEN);
		if (text.Length > 0)
		{
			AvatarData.SetStyleTexture(inInstance, AvatarData.pPartSettings.AVATAR_PART_EYES, text, AvatarData.pGeneralSettings.OPEN_INDEX);
		}
		text = iData.GetTextureNameNoPath(AvatarData.pTextureSettings.TEXTURE_TYPE_EYES_CLOSED);
		if (text.Length > 0)
		{
			AvatarData.SetStyleTexture(inInstance, AvatarData.pPartSettings.AVATAR_PART_EYES, text, AvatarData.pGeneralSettings.CLOSED_INDEX);
		}
		text = iData.GetTextureNameNoPath(AvatarData.pTextureSettings.TEXTURE_TYPE_MOUTH_OPEN);
		if (text.Length > 0)
		{
			AvatarData.SetStyleTexture(inInstance, AvatarData.pPartSettings.AVATAR_PART_MOUTH, text, AvatarData.pGeneralSettings.OPEN_INDEX);
		}
		string textureNameNoPath = iData.GetTextureNameNoPath(AvatarData.pTextureSettings.TEXTURE_TYPE_MOUTH_CLOSED);
		if (textureNameNoPath.Length > 0)
		{
			AvatarData.SetStyleTexture(inInstance, AvatarData.pPartSettings.AVATAR_PART_MOUTH, textureNameNoPath, AvatarData.pGeneralSettings.CLOSED_INDEX);
		}
		else if (text.Length > 0)
		{
			AvatarData.SetStyleTexture(inInstance, AvatarData.pPartSettings.AVATAR_PART_MOUTH, text, AvatarData.pGeneralSettings.CLOSED_INDEX);
		}
		text = iData.GetTextureNameNoPath(AvatarData.pTextureSettings.TEXTURE_TYPE_SKIN);
		if (text.Length > 0)
		{
			AvatarData.SetStyleTexture(inInstance, AvatarData.pPartSettings.AVATAR_PART_SKIN, text, AvatarData.pGeneralSettings.PRIMARY_INDEX);
		}
	}

	public void ApplyAttributes(AvatarData.InstanceInfo inInstance, ItemData inData)
	{
		AvatarData.SetAttributes(inInstance, _PrtTypeName, inData.Attribute);
	}

	public void ApplySelection(AvatarData.InstanceInfo inInstance, string resName, ItemData iData)
	{
		bool flag = false;
		if (iData.Relationship != null && iData.Relationship.Length != 0)
		{
			flag = Array.Exists(iData.Relationship, (ItemDataRelationship r) => r.Type.Equals("GroupParent"));
		}
		string text = _PrtTypeName.Replace("DEFAULT_", "");
		if (flag)
		{
			AvatarData.SetGroupPart(inInstance, iData.ItemID);
		}
		else if (text == AvatarData.pPartSettings.AVATAR_PART_LEGS || text == AvatarData.pPartSettings.AVATAR_PART_FEET || text == AvatarData.pPartSettings.AVATAR_PART_TOP || text == AvatarData.pPartSettings.AVATAR_PART_HAIR || text == AvatarData.pPartSettings.AVATAR_PART_HAT || text == AvatarData.pPartSettings.AVATAR_PART_FACEMASK || text == AvatarData.pPartSettings.AVATAR_PART_HAND_PROP_RIGHT || text == AvatarData.pPartSettings.AVATAR_PART_WRISTBAND || text == AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD)
		{
			ApplyGeom(inInstance, resName, iData);
		}
		else
		{
			if (text == AvatarData.pPartSettings.AVATAR_PART_BACK)
			{
				ApplyGeom(inInstance, resName, iData);
				ApplyAttributes(inInstance, iData);
				UpdatePartShader(_PrtTypeName);
				return;
			}
			if (text == AvatarData.pPartSettings.AVATAR_PART_HEAD)
			{
				ApplyGeom(inInstance, resName, iData);
				ApplyPreset(inInstance, iData);
			}
			else if (text == AvatarData.pPartSettings.AVATAR_PART_EYES || text == AvatarData.pPartSettings.AVATAR_PART_MOUTH || text == AvatarData.pPartSettings.AVATAR_PART_SKIN)
			{
				ApplyPreset(inInstance, iData);
			}
		}
		ApplyAttributes(inInstance, iData);
	}

	public void UpdatePartShader(string part)
	{
		if (CustomAvatarState.mCurrentInstance != null)
		{
			CustomAvatarState.mCurrentInstance.UpdatePartShader(AvAvatar.pObject, part, part, AvatarData.pInstanceInfo);
		}
	}
}
