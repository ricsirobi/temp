using System;

public class UWSwimmingCSM : ObContextSensitive
{
	public ContextSensitiveState[] _Menus;

	public string _DiveCSItemName = "Dive";

	public string _SurfaceCSItemName = "Surface";

	public float _Offset = 4f;

	private bool mIsAllowed;

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
		inStatesArrData = ((AvAvatar.pState == AvAvatarState.PAUSED) ? null : _Menus);
	}

	protected override void OnMenuActive(ContextSensitiveStateType inMenuType)
	{
		base.OnMenuActive(inMenuType);
		if (base.pUI != null && base.pUI.gameObject.transform.parent != AvAvatar.pToolbar.transform)
		{
			base.pUI.gameObject.transform.parent = AvAvatar.pToolbar.transform;
		}
		SetContextData();
	}

	protected override bool IsAllowed(ContextSensitiveStateType inPriorityType)
	{
		if (!mIsAllowed)
		{
			return false;
		}
		return base.IsAllowed(inPriorityType);
	}

	public void OnContextAction(string inName)
	{
		if (inName.Equals(_SurfaceCSItemName))
		{
			AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
			component.pUWSwimZone.pDoAvatarTransit = true;
			component.pUWSwimZone.Exit();
			Show(isVisible: false);
		}
		else if (inName.Equals(_DiveCSItemName))
		{
			AvAvatar.pObject.GetComponent<AvAvatarController>().pUWSwimZone.pDoAvatarTransit = true;
			AvAvatar.pSubState = AvAvatarSubState.UWSWIMMING;
			Show(isVisible: false);
		}
	}

	public void Show(bool isVisible)
	{
		mIsAllowed = isVisible;
		if (!isVisible)
		{
			DestroyMenu(checkProximity: false);
		}
		else if (base.pUI == null)
		{
			Update();
		}
		else
		{
			SetContextData();
		}
	}

	private void SetContextData()
	{
		AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
		if (component.pUWSwimZone == null)
		{
			DestroyMenu(checkProximity: false);
			return;
		}
		string inName = ((component.pSubState == AvAvatarSubState.UWSWIMMING) ? _SurfaceCSItemName : _DiveCSItemName);
		string inName2 = ((component.pSubState == AvAvatarSubState.UWSWIMMING) ? _DiveCSItemName : _SurfaceCSItemName);
		ContextData contextData = GetContextData(inName);
		if (contextData != null)
		{
			base.pUI.AddContextDataIntoList(contextData, enableRefreshItems: true);
		}
		contextData = GetContextData(inName2);
		if (contextData != null)
		{
			base.pUI.RemoveContextDataFromList(contextData, enableRefreshItems: true);
		}
	}

	public void OnAvatarStateChange()
	{
		if ((AvAvatar.pState == AvAvatarState.PAUSED || AvAvatar.pPrevState == AvAvatarState.PAUSED) && mIsAllowed)
		{
			Refresh();
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
}
