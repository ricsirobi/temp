using System;
using System.Collections;
using UnityEngine;
using Zendesk.Internal.Models.Common;
using Zendesk.UI;

public class DragonsZendesk : ZendeskMain
{
	public delegate void OnLoaded(DragonsZendesk inUiDragonsZendesk);

	public delegate void OnClosed();

	public static OnClosed pOnClosedDelegate;

	public static ZendeskMain pZendeskInstance;

	private static AvAvatarState mCachedState;

	public static void OpenCreateRequest()
	{
		KAUICursorManager.SetDefaultCursor("Loading");
		mCachedState = AvAvatar.pState;
		AvAvatar.pState = AvAvatarState.PAUSED;
		AvAvatar.SetUIActive(inActive: false);
		if ((bool)pZendeskInstance)
		{
			pZendeskInstance.StartCoroutine(SendPostZendeskLoadMessage(pZendeskInstance, "OpenCreateRequest"));
		}
		else
		{
			RsResourceManager.LoadAssetFromBundle(GameConfig.GetKeyData("ZendeskAsset_Release"), OnCreateRequestLoaded, typeof(GameObject));
		}
	}

	public static void OpenHelpCenter()
	{
		KAUICursorManager.SetDefaultCursor("Loading");
		mCachedState = AvAvatar.pState;
		AvAvatar.pState = AvAvatarState.PAUSED;
		AvAvatar.SetUIActive(inActive: false);
		if ((bool)pZendeskInstance)
		{
			pZendeskInstance.StartCoroutine(SendPostZendeskLoadMessage(pZendeskInstance, "OpenHelpCenter"));
		}
		else
		{
			RsResourceManager.LoadAssetFromBundle(GameConfig.GetKeyData("ZendeskAsset_Release"), OnHelpCenterLoaded, typeof(GameObject));
		}
	}

	private static void OnCreateRequestLoaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			GameObject obj = UnityEngine.Object.Instantiate((GameObject)inObject);
			obj.name = ((GameObject)inObject).name;
			ZendeskMain component = obj.GetComponent<ZendeskMain>();
			if ((bool)component)
			{
				pZendeskInstance = component;
				component.StartCoroutine(SendPostZendeskLoadMessage(component, "OpenCreateRequest"));
			}
			((OnLoaded)inUserData)?.Invoke(null);
			break;
		}
		case RsResourceLoadEvent.ERROR:
			KAUICursorManager.SetDefaultCursor("Arrow");
			AvAvatar.pState = mCachedState;
			AvAvatar.SetUIActive(inActive: true);
			Debug.LogError("Error loading Zendesk Prefab!" + inURL);
			((OnLoaded)inUserData)?.Invoke(null);
			break;
		}
	}

	private static void OnHelpCenterLoaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			GameObject obj = UnityEngine.Object.Instantiate((GameObject)inObject);
			obj.name = ((GameObject)inObject).name;
			ZendeskMain component = obj.GetComponent<ZendeskMain>();
			if ((bool)component)
			{
				pZendeskInstance = component;
				component.StartCoroutine(SendPostZendeskLoadMessage(component, "OpenHelpCenter"));
			}
			((OnLoaded)inUserData)?.Invoke(null);
			break;
		}
		case RsResourceLoadEvent.ERROR:
			KAUICursorManager.SetDefaultCursor("Arrow");
			AvAvatar.pState = AvAvatarState.IDLE;
			AvAvatar.SetUIActive(inActive: true);
			Debug.LogError("Error loading Zendesk Prefab!" + inURL);
			((OnLoaded)inUserData)?.Invoke(null);
			break;
		}
	}

	public static IEnumerator SendPostZendeskLoadMessage(ZendeskMain inZD, string inMessage)
	{
		while (inZD.InitialisationStatus != InitialisationStatus.Initialised)
		{
			if (inZD.InitialisationStatus == InitialisationStatus.Failed)
			{
				Debug.LogError("Zendesk initialization failed!");
				AvAvatar.pState = mCachedState;
				AvAvatar.SetUIActive(inActive: true);
				yield break;
			}
			yield return new WaitForEndOfFrame();
		}
		ZendeskUI obj = pZendeskInstance.zendeskUI;
		obj.OnSupportUIClose = (Action<bool>)Delegate.Combine(obj.OnSupportUIClose, new Action<bool>(OnSupportClosed));
		inZD.SendMessage(inMessage);
		KAUICursorManager.SetDefaultCursor("Arrow");
		yield return null;
	}

	public static void OnSupportClosed(bool enable)
	{
		AvAvatar.pState = mCachedState;
		AvAvatar.SetUIActive(inActive: true);
		ZendeskUI obj = pZendeskInstance.zendeskUI;
		obj.OnSupportUIClose = (Action<bool>)Delegate.Remove(obj.OnSupportUIClose, new Action<bool>(OnSupportClosed));
		pOnClosedDelegate();
	}
}
