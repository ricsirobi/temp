using System.Collections.Generic;
using UnityEngine;

public class ObMazeTrigger : MonoBehaviour
{
	public bool _DisableObClickable;

	public bool _DisableBuddyVisiting = true;

	public bool _CheckForTriggerStay;

	public string[] _SnPoolNames;

	public string[] _DeactivateObject;

	private static bool mIsStayCheckDone;

	private List<GameObject> mDeactivateObjectContainer = new List<GameObject>();

	private void OnEnable()
	{
		if (_DeactivateObject.Length == 0)
		{
			return;
		}
		for (int i = 0; i < _DeactivateObject.Length; i++)
		{
			GameObject gameObject = GameObject.Find(_DeactivateObject[i]);
			if (gameObject != null)
			{
				mDeactivateObjectContainer.Add(gameObject);
			}
		}
	}

	private void OnTriggerEnter(Collider inCollider)
	{
		if (!AvAvatar.IsCurrentPlayer(inCollider.gameObject))
		{
			return;
		}
		mIsStayCheckDone = true;
		if (AvAvatar.pToolbar != null)
		{
			UiToolbar uiToolbar = AvAvatar.pToolbar.GetComponent(typeof(UiToolbar)) as UiToolbar;
			if (uiToolbar != null)
			{
				KAWidget kAWidget = uiToolbar.FindItem("AniToolbarCounter");
				if (kAWidget != null)
				{
					kAWidget.SetState(KAUIState.NOT_INTERACTIVE);
				}
				else
				{
					UtDebug.LogError("ERROR: UNABLE TO FIND THE COUNTER BUTTON ITEM WHEN ENTERING THE HEART MAZE!!");
				}
			}
			else
			{
				UtDebug.LogError("ERROR: UNABLE TO GET THE TOOLBAR COMPONENT WHEN ENTERING THE HEART MAZE!!");
			}
		}
		else
		{
			UtDebug.LogError("ERROR: UNABLED TO GET THE TOOLBAR OBJECT WHEN ENTERING THE HEART MAZE!!");
		}
		if (_DisableObClickable)
		{
			ObClickable.pGlobalActive = false;
		}
		if (_DisableBuddyVisiting)
		{
			MainStreetMMOClient.pInstance.SetJoinAllowed(MMOJoinStatus.NOT_ALLOWED);
		}
		if (_SnPoolNames.Length != 0)
		{
			for (int i = 0; i < _SnPoolNames.Length; i++)
			{
				SnChannel.MutePool(_SnPoolNames[i], mute: true);
			}
		}
		if (mDeactivateObjectContainer.Count <= 0)
		{
			return;
		}
		foreach (GameObject item in mDeactivateObjectContainer)
		{
			if (item != null)
			{
				item.SetActive(value: false);
			}
		}
	}

	private void OnTriggerStay(Collider inCollider)
	{
		if (_CheckForTriggerStay && !mIsStayCheckDone && AvAvatar.IsCurrentPlayer(inCollider.gameObject))
		{
			OnTriggerEnter(inCollider);
			mIsStayCheckDone = true;
		}
	}

	private void OnTriggerExit(Collider inCollider)
	{
		if (!AvAvatar.IsCurrentPlayer(inCollider.gameObject))
		{
			return;
		}
		mIsStayCheckDone = false;
		if (AvAvatar.pToolbar != null)
		{
			UiToolbar uiToolbar = AvAvatar.pToolbar.GetComponent(typeof(UiToolbar)) as UiToolbar;
			if (uiToolbar != null)
			{
				KAWidget kAWidget = uiToolbar.FindItem("AniToolbarCounter");
				if (kAWidget != null)
				{
					kAWidget.SetState(KAUIState.INTERACTIVE);
				}
				else
				{
					UtDebug.LogError("ERROR: UNABLE TO FIND THE COUNTER BUTTON ITEM WHEN EXITING THE HEART MAZE!!");
				}
				kAWidget = uiToolbar.FindItem("BuddyListBtn");
				if (kAWidget != null)
				{
					kAWidget.SetDisabled(isDisabled: false);
				}
				else
				{
					UtDebug.LogError("ERROR: UNABLE TO FIND THE BUDDY LIST BUTTON ITEM WHEN EXITING THE HEART MAZE!!");
				}
			}
			else
			{
				UtDebug.LogError("ERROR: UNABLE TO GET THE TOOLBAR COMPONENT WHEN EXITING THE HEART MAZE!!");
			}
		}
		else
		{
			UtDebug.LogError("ERROR: UNABLED TO GET THE TOOLBAR OBJECT WHEN EXITING THE HEART MAZE!!");
		}
		if (_DisableObClickable)
		{
			ObClickable.pGlobalActive = true;
		}
		if (_DisableBuddyVisiting)
		{
			MainStreetMMOClient.pInstance.SetJoinAllowed(MMOJoinStatus.ALLOWED);
		}
		if (_SnPoolNames.Length != 0)
		{
			for (int i = 0; i < _SnPoolNames.Length; i++)
			{
				SnChannel.MutePool(_SnPoolNames[i], mute: false);
			}
		}
		if (mDeactivateObjectContainer.Count <= 0)
		{
			return;
		}
		foreach (GameObject item in mDeactivateObjectContainer)
		{
			if (item != null)
			{
				item.SetActive(value: true);
			}
		}
	}
}
