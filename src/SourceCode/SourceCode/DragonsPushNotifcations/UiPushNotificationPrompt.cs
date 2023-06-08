using System;
using JSGames.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DragonsPushNotifcations;

public class UiPushNotificationPrompt : UI
{
	public JSGames.UI.UIButton _CloseBtn;

	public JSGames.UI.UIButton _SubscribeBtn;

	private Action SubscribeAction;

	private Action CloseAction;

	protected override void OnClick(JSGames.UI.UIWidget inWidget, PointerEventData inData)
	{
		base.OnClick(inWidget, inData);
		if (inWidget == _CloseBtn)
		{
			CloseUI();
		}
		else if (inWidget == _SubscribeBtn)
		{
			SubscribeToPushNotifications();
		}
	}

	public void Init(Action subscribeAction, Action closeAction)
	{
		SubscribeAction = subscribeAction;
		CloseAction = closeAction;
		SetExclusive();
		AvAvatar.pState = AvAvatarState.PAUSED;
		AvAvatar.SetUIActive(inActive: false);
	}

	private void CloseUI()
	{
		CloseAction?.Invoke();
		Cleanup();
	}

	private void SubscribeToPushNotifications()
	{
		SubscribeAction?.Invoke();
		Cleanup();
	}

	private void Cleanup()
	{
		RemoveExclusive();
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.SetUIActive(inActive: true);
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void OnDestroy()
	{
		CloseAction = null;
		SubscribeAction = null;
	}
}
