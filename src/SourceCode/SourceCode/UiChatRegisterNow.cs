using System;
using System.Collections.Generic;
using UnityEngine;

public class UiChatRegisterNow : KAUI
{
	private KAButton mCloseBtn;

	private KAButton mRegisterBtn;

	private Action OnCloseAction;

	protected override void Start()
	{
		base.Start();
		KAUI.SetExclusive(this);
		mCloseBtn = (KAButton)FindItem("CloseBtn");
		mRegisterBtn = (KAButton)FindItem("RegisterBtn");
	}

	public void Init(Action closeAction)
	{
		OnCloseAction = closeAction;
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mRegisterBtn)
		{
			OnAcceptRegister();
		}
		else if (inWidget == mCloseBtn)
		{
			OnClose();
		}
	}

	private void OnAcceptRegister()
	{
		KAUI.RemoveExclusive(this);
		GameUtilities.LoadLoginLevel(showRegstration: true);
		UiRegister.pGuestChatPromptOpened = true;
		AnalyticAgent.LogEvent(AnalyticEvent.GUEST_CHAT, new Dictionary<string, object> { { "name", "accepted" } });
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void OnClose()
	{
		KAUI.RemoveExclusive(this);
		if ((bool)UiRacingMultiplayer.pInstance)
		{
			UiRacingMultiplayer.pInstance.SetInteractive(interactive: true);
		}
		OnCloseAction?.Invoke();
		AnalyticAgent.LogEvent(AnalyticEvent.GUEST_CHAT, new Dictionary<string, object> { { "name", "closed" } });
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
