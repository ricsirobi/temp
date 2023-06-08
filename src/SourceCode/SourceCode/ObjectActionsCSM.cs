using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectActionsCSM : ObContextSensitive
{
	public ContextSensitiveState[] _Menus;

	public GameObject _RenderingObject;

	private List<ActionBase> mActionScripts;

	protected override void Start()
	{
		base.Start();
		AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
		if (component != null)
		{
			component.OnAvatarStateChange = (AvAvatarController.OnAvatarStateChanged)Delegate.Combine(component.OnAvatarStateChange, new AvAvatarController.OnAvatarStateChanged(OnAvatarStateChange));
		}
	}

	protected override void UpdateData(ref ContextSensitiveState[] inStatesArrData)
	{
		for (int i = 0; i < _Menus.Length; i++)
		{
			inStatesArrData[i] = (ContextSensitiveState)_Menus[i].Clone();
		}
		if (mActionScripts == null)
		{
			return;
		}
		foreach (ActionBase mActionScript in mActionScripts)
		{
			if (!mActionScript.IsActionAllowed())
			{
				ContextSensitiveState[] array = inStatesArrData;
				foreach (ContextSensitiveState contextSensitiveState in array)
				{
					if (contextSensitiveState._CurrentContextNamesList != null && contextSensitiveState._CurrentContextNamesList.Length != 0)
					{
						List<string> list = new List<string>(contextSensitiveState._CurrentContextNamesList);
						if (list.Contains(mActionScript._ActionName))
						{
							list.Remove(mActionScript._ActionName);
						}
						if (list.Count == 0)
						{
							contextSensitiveState._CurrentContextNamesList = null;
							continue;
						}
						contextSensitiveState._CurrentContextNamesList = new string[list.Count];
						contextSensitiveState._CurrentContextNamesList = list.ToArray();
					}
				}
			}
			SetInteractiveEnabledData(mActionScript._ActionName, mActionScript.IsActionAllowed());
		}
	}

	public void OnContextAction(string inName)
	{
		foreach (ActionBase mActionScript in mActionScripts)
		{
			if (mActionScript._ActionName == inName)
			{
				CloseMenu(checkProximity: true);
				mActionScript.ExecuteAction();
			}
		}
	}

	public void RegisterCSMAction(ActionBase inNewAction)
	{
		if (mActionScripts == null)
		{
			mActionScripts = new List<ActionBase>();
		}
		if (mActionScripts.Find((ActionBase a) => a == inNewAction) == null)
		{
			mActionScripts.Add(inNewAction);
		}
	}

	public void OnActionDone()
	{
		if (_RenderingObject != null)
		{
			_RenderingObject.SetActive(value: false);
		}
	}

	public void DestroyCSM()
	{
		DestroyMenu(checkProximity: false);
	}

	public void SetProximityEntered()
	{
		SetProximityAlreadyEntered(isEntered: false);
	}

	public void RefreshUI()
	{
		SetProximityEntered();
		Refresh();
	}

	protected override void Update()
	{
		if (ObContextSensitive.pExclusiveUI != null && ObContextSensitive.pExclusiveUI != base.pUI)
		{
			SetProximityEntered();
			DestroyCSM();
		}
		else
		{
			base.Update();
		}
	}

	public void OnAvatarStateChange()
	{
		if (AvAvatar.pState == AvAvatarState.PAUSED || AvAvatar.pPrevState == AvAvatarState.PAUSED)
		{
			RefreshUI();
		}
	}

	protected override void OnDestroy()
	{
		if (AvAvatar.pObject != null)
		{
			AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
			if (component != null)
			{
				component.OnAvatarStateChange = (AvAvatarController.OnAvatarStateChanged)Delegate.Remove(component.OnAvatarStateChange, new AvAvatarController.OnAvatarStateChanged(OnAvatarStateChange));
			}
		}
		base.OnDestroy();
	}

	public void UpdateButtonSprite(string item, string sprite)
	{
		ContextData contextData = GetContextData(item);
		if (contextData != null)
		{
			contextData._IconSpriteName = sprite;
			RefreshUI();
		}
	}
}
