using UnityEngine;

public class UiRaceRewards : KAUI
{
	public GameObject _MessageObject;

	public string _YesMessage = "";

	public string _NoMessage = "";

	public string _OKMessage = "";

	public string _CloseMessage = "";

	public void ShowRewards(int DragonXP, int PlayerXP, int numCoins, int numTrophies)
	{
		if (DragonXP > 0)
		{
			FindItem("TxtXPDragon").SetText(DragonXP.ToString());
		}
		else
		{
			FindItem("TxtXPDragon").SetVisibility(inVisible: false);
			FindItem("IcoXPDragon").SetVisibility(inVisible: false);
		}
		if (PlayerXP > 0)
		{
			FindItem("TxtXPPlayer").SetText(PlayerXP.ToString());
		}
		else
		{
			FindItem("TxtXPPlayer").SetVisibility(inVisible: false);
			FindItem("IcoXPPlayer").SetVisibility(inVisible: false);
		}
		if (numCoins > 0)
		{
			FindItem("TxtCoins").SetText(numCoins.ToString());
		}
		else
		{
			FindItem("TxtCoins").SetVisibility(inVisible: false);
			FindItem("IcoCoins").SetVisibility(inVisible: false);
		}
		if (numTrophies != 0)
		{
			FindItem("TxtTrophies").SetText(numTrophies.ToString());
		}
		else
		{
			FindItem("TxtTrophies").SetVisibility(inVisible: false);
			FindItem("IcoTrophies").SetVisibility(inVisible: false);
		}
		FindItem("OKBtn").SetVisibility(inVisible: true);
	}

	public void SetButtonVisibility(bool OkBtn, bool CloseBtn, bool YesBtn, bool NoBtn)
	{
		FindItem("OKBtn").SetVisibility(OkBtn);
		FindItem("CloseBtn").SetVisibility(CloseBtn);
		FindItem("NoBtn").SetVisibility(NoBtn);
		FindItem("YesBtn").SetVisibility(YesBtn);
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget.name == "YesBtn" && _YesMessage.Length > 0)
		{
			if (_MessageObject != null)
			{
				_MessageObject.SendMessage(_YesMessage, null, SendMessageOptions.DontRequireReceiver);
			}
		}
		else if (inWidget.name == "NoBtn" && _NoMessage.Length > 0)
		{
			if (_MessageObject != null)
			{
				_MessageObject.SendMessage(_NoMessage, null, SendMessageOptions.DontRequireReceiver);
			}
		}
		else if (inWidget.name == "OKBtn" && _OKMessage.Length > 0)
		{
			if (_MessageObject != null)
			{
				_MessageObject.SendMessage(_OKMessage, null, SendMessageOptions.DontRequireReceiver);
			}
		}
		else if (inWidget.name == "CloseBtn" && _CloseMessage.Length > 0 && _MessageObject != null)
		{
			_MessageObject.SendMessage(_CloseMessage, null, SendMessageOptions.DontRequireReceiver);
		}
	}
}
