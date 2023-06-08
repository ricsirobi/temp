public class KAUINPCGenericDB : KAUIGenericDB
{
	public void SetNPCIcon(string iconName)
	{
		if (!string.IsNullOrEmpty(iconName))
		{
			KAWidget kAWidget = FindItem("NPCIcon");
			if (kAWidget != null)
			{
				kAWidget.PlayAnim(iconName);
			}
			KAWidget kAWidget2 = FindItem("TxtTitle");
			if (kAWidget2 != null)
			{
				kAWidget2.SetText(MissionManagerDO.GetNPCName(iconName));
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		RsResourceManager.UnloadUnusedAssets();
	}
}
