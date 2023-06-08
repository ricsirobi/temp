using UnityEngine;

public class UiLevelUpDB : KAUI
{
	private UiLevelUpDialogClosedCallback mDialogClosedCallback;

	public void Exit()
	{
		Object.Destroy(base.gameObject);
		if (mDialogClosedCallback != null)
		{
			mDialogClosedCallback();
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		if (inWidget.name == "BtnClose")
		{
			Exit();
		}
		base.OnClick(inWidget);
	}

	public void ShowLevelUpMessage(string inTitleText, string inMessageText, string npcName, UiLevelUpDialogClosedCallback DialogClosedCallback)
	{
		mDialogClosedCallback = DialogClosedCallback;
		KAWidget kAWidget = FindItem("TxtHelpHeader");
		if (kAWidget != null)
		{
			kAWidget.SetText(inTitleText);
		}
		KAWidget kAWidget2 = FindItem("uiText");
		if (kAWidget2 != null)
		{
			kAWidget2.SetText(inMessageText);
		}
		SetNPCIcon(npcName);
	}

	public virtual void SetNPCIcon(string npcName)
	{
		if (!string.IsNullOrEmpty(npcName))
		{
			KAWidget kAWidget = FindItem("NPCIcon");
			if (kAWidget != null)
			{
				kAWidget.PlayAnim(npcName);
			}
		}
	}
}
