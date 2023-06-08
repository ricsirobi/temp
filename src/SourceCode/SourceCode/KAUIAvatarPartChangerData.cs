using System;

public class KAUIAvatarPartChangerData : AvatarPartChanger
{
	public void ApplyGeom(KAUISelectItemData uData)
	{
		ApplyGeomWIndex(AvatarData.pInstanceInfo, uData._PrefResName, 0);
		if (uData._ItemData.Geometry2 != null && uData._ItemData.Geometry2.Length > 0)
		{
			ApplyGeomWIndex(AvatarData.pInstanceInfo, uData._ItemData.Geometry2, 1);
		}
	}

	public void ApplyPreset(KAUISelectItemData uData)
	{
		ApplyPreset(AvatarData.pInstanceInfo, uData._ItemData);
	}

	public void ApplySelection(KAWidget item)
	{
		KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)item.GetUserData();
		if (kAUISelectItemData != null)
		{
			ApplySelection(kAUISelectItemData);
		}
	}

	public void ApplySelection(KAUISelectItemData uData, bool checkDefaultPart = false)
	{
		if (uData == null)
		{
			return;
		}
		bool flag = false;
		if (uData._ItemData.Relationship != null && uData._ItemData.Relationship.Length != 0)
		{
			ItemDataRelationship[] relationship = uData._ItemData.Relationship;
			for (int i = 0; i < relationship.Length; i++)
			{
				if (relationship[i].Type.Equals("GroupParent"))
				{
					flag = true;
					break;
				}
			}
		}
		if (flag)
		{
			if (!uData._ItemData.AssetName.Equals("NULL", StringComparison.OrdinalIgnoreCase))
			{
				_SaveDefault = true;
				AvatarData.SetGroupPart(uData._ItemData.ItemID, saveData: false);
			}
		}
		else if (_PrtTypeName == AvatarData.pPartSettings.AVATAR_PART_LEGS || _PrtTypeName == AvatarData.pPartSettings.AVATAR_PART_FEET || _PrtTypeName == AvatarData.pPartSettings.AVATAR_PART_TOP || _PrtTypeName == AvatarData.pPartSettings.AVATAR_PART_HAIR || _PrtTypeName == AvatarData.pPartSettings.AVATAR_PART_HAT || _PrtTypeName == AvatarData.pPartSettings.AVATAR_PART_FACEMASK || _PrtTypeName == AvatarData.pPartSettings.AVATAR_PART_HAND_PROP_RIGHT || _PrtTypeName == AvatarData.pPartSettings.AVATAR_PART_BACK || _PrtTypeName == AvatarData.pPartSettings.AVATAR_PART_WRISTBAND || _PrtTypeName == AvatarData.pPartSettings.AVATAR_PART_HAND || _PrtTypeName == AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD)
		{
			if (checkDefaultPart && AvatarData.IsDefaultSaved(_PrtTypeName))
			{
				string prtTypeName = _PrtTypeName;
				_PrtTypeName = "DEFAULT_" + _PrtTypeName;
				ApplySelection(AvatarData.pInstanceInfo, uData._PrefResName, uData._ItemData);
				_PrtTypeName = prtTypeName;
			}
			else
			{
				ApplySelection(AvatarData.pInstanceInfo, uData._PrefResName, uData._ItemData);
				AvatarData.RestoreCurrentPartCheckBundle(_PrtTypeName);
			}
		}
		else if (_PrtTypeName == AvatarData.pPartSettings.AVATAR_PART_HEAD)
		{
			ApplySelection(AvatarData.pInstanceInfo, uData._PrefResName, uData._ItemData);
			AvatarData.RestoreCurrentPartCheckBundle(_PrtTypeName);
			AvatarData.RestoreCurrentPartCheckBundle(AvatarData.pPartSettings.AVATAR_PART_MOUTH);
			AvatarData.RestoreCurrentPartCheckBundle(AvatarData.pPartSettings.AVATAR_PART_SKIN);
			AvatarData.RestoreCurrentPartCheckBundle(AvatarData.pPartSettings.AVATAR_PART_EYES);
		}
		else if (_PrtTypeName == AvatarData.pPartSettings.AVATAR_PART_EYES || _PrtTypeName == AvatarData.pPartSettings.AVATAR_PART_MOUTH || _PrtTypeName == AvatarData.pPartSettings.AVATAR_PART_SKIN)
		{
			ApplySelection(AvatarData.pInstanceInfo, uData._PrefResName, uData._ItemData);
			AvatarData.RestoreCurrentPartCheckBundle(_PrtTypeName);
		}
		else if (_PrtTypeName == AvatarData.pPartSettings.AVATAR_PART_TAIL)
		{
			ApplyGeom(uData);
			AvatarData.RestoreCurrentPartCheckBundle(_PrtTypeName);
		}
	}
}
