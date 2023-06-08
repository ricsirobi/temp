using System;

public class UiStableToolbar : KAUI
{
	public override void OnClick(KAWidget item)
	{
		if (item.name == "BtnStables")
		{
			AvAvatar.pState = AvAvatarState.PAUSED;
			SetInteractive(interactive: false);
			UiDragonsStable.pOnStablesUILoadHandler = (OnStablesUILoad)Delegate.Combine(UiDragonsStable.pOnStablesUILoadHandler, new OnStablesUILoad(OnStableUILoad));
			UiDragonsStable.pOnStablesUIClosed = (OnStablesUIClosed)Delegate.Combine(UiDragonsStable.pOnStablesUIClosed, new OnStablesUIClosed(OnStableUIClosed));
			UiDragonsStable.OpenStableListUI(base.gameObject, setExclusive: false);
		}
	}

	public void OnStableUILoad(bool isSuccess)
	{
		UiDragonsStable.pOnStablesUILoadHandler = (OnStablesUILoad)Delegate.Remove(UiDragonsStable.pOnStablesUILoadHandler, new OnStablesUILoad(OnStableUILoad));
		AvAvatar.SetUIActive(inActive: false);
		AvAvatar.EnableAllInputs(inActive: false);
	}

	public void OnStableUIClosed()
	{
		UiDragonsStable.pOnStablesUIClosed = (OnStablesUIClosed)Delegate.Remove(UiDragonsStable.pOnStablesUIClosed, new OnStablesUIClosed(OnStableUIClosed));
		SetInteractive(interactive: true);
	}
}
