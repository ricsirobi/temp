using UnityEngine;

namespace StableAbility;

public class AbilityCSM : ObContextSensitive
{
	public ABILITY _Ability;

	public string _UIAssetPath;

	public ContextSensitiveState[] _Menus;

	protected GameObject mStableAbilityUI;

	private AvAvatarState mCachedAvatarState;

	public void OnContextAction()
	{
		if (!string.IsNullOrWhiteSpace(_UIAssetPath))
		{
			mCachedAvatarState = AvAvatar.pState;
			AvAvatar.pState = AvAvatarState.PAUSED;
			AvAvatar.SetUIActive(inActive: false);
			KAUICursorManager.SetDefaultCursor("Loading");
			string[] array = _UIAssetPath.Split('/');
			RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnUILoaded, typeof(GameObject));
			DestroyMenu(checkProximity: false);
		}
	}

	private void OnUILoaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			KAUICursorManager.SetDefaultCursor("Arrow");
			mStableAbilityUI = Object.Instantiate((GameObject)inObject);
			if ((bool)mStableAbilityUI)
			{
				mStableAbilityUI.name = "PfUiStableAbility";
				UiStableAbility component = mStableAbilityUI.GetComponent<UiStableAbility>();
				component.Init(_Ability);
				component.UiStableAbilityClosed = ActivateAbilityCSM;
				DestroyMenu(checkProximity: false);
				_Active = false;
			}
			RsResourceManager.ReleaseBundleData(inURL);
			break;
		case RsResourceLoadEvent.ERROR:
			UtDebug.LogError($"Failed to load UI for {_Ability}");
			KAUICursorManager.SetDefaultCursor("Arrow");
			AvAvatar.pState = mCachedAvatarState;
			AvAvatar.SetUIActive(inActive: true);
			break;
		}
	}

	private void ActivateAbilityCSM()
	{
		_Active = true;
	}

	protected override void UpdateData(ref ContextSensitiveState[] inStatesArrData)
	{
		inStatesArrData = _Menus;
	}
}
