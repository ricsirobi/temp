using UnityEngine;

public class UiSocialCrateInfo : KAUI
{
	private GameObject mMessageObject;

	private KAWidget mBtnOK;

	protected override void Start()
	{
		base.Start();
		mBtnOK = FindItem("OKBtn");
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mBtnOK)
		{
			KAUI.RemoveExclusive(this);
			mMessageObject.SendMessage("OnExit");
		}
	}

	public void SetMessageObject(GameObject msg)
	{
		mMessageObject = msg;
	}
}
