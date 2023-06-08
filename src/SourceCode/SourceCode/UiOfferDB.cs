using UnityEngine;

public class UiOfferDB : KAUI
{
	public GameObject _MessageObject;

	public string _RegisterMessage = "OnRegisterYes";

	public string _CloseMessage = "OnRegisterNo";

	public bool _DestroyAfterUse;

	private KAWidget mRegisterBtn;

	private KAWidget mCloseBtn;

	protected override void Start()
	{
		base.Start();
		mRegisterBtn = FindItem("BtnRegister");
		mCloseBtn = FindItem("BtnClose");
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mRegisterBtn)
		{
			if (_MessageObject != null)
			{
				_MessageObject.SendMessage(_RegisterMessage, null, SendMessageOptions.DontRequireReceiver);
			}
		}
		else if (inWidget == mCloseBtn && _MessageObject != null)
		{
			_MessageObject.SendMessage(_CloseMessage, null, SendMessageOptions.DontRequireReceiver);
		}
		KAUI.RemoveExclusive(this);
		if (_DestroyAfterUse)
		{
			Object.Destroy(base.gameObject);
			RsResourceManager.UnloadUnusedAssets();
		}
		AvAvatar.pState = AvAvatar.pPrevState;
	}

	public void SetText(string inText)
	{
		KAWidget kAWidget = FindItem("TxtRegister");
		if (kAWidget != null)
		{
			kAWidget.SetText(inText);
		}
	}
}
