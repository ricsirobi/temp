using UnityEngine;

public class UiNameSuggestion : KAUI
{
	public delegate void NameSelected(string name);

	public KAUIMenu _NameSuggestionMenu;

	private static NameSelected mNameSelectedCallback;

	private KAWidget mCloseBtn;

	private static string[] mSuggestedNames;

	public static void Init(SuggestionResult result, NameSelected callback)
	{
		KAUICursorManager.SetDefaultCursor("Loading");
		mSuggestedNames = result.Suggestion;
		mNameSelectedCallback = callback;
		RsResourceManager.LoadAssetFromBundle(GameConfig.GetKeyData("NameSuggestionAsset"), OnBundleReady, typeof(GameObject));
	}

	private static void OnBundleReady(string inURL, RsResourceLoadEvent inLoadEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inLoadEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			KAUICursorManager.SetDefaultCursor("Arrow");
			Object.Instantiate((GameObject)inObject);
			break;
		case RsResourceLoadEvent.ERROR:
			KAUICursorManager.SetDefaultCursor("Arrow");
			mSuggestedNames = null;
			if (mNameSelectedCallback != null)
			{
				mNameSelectedCallback("");
			}
			break;
		}
	}

	protected override void Start()
	{
		base.Start();
		KAUI.SetExclusive(this);
		mCloseBtn = FindItem("CloseBtn");
		if (mSuggestedNames != null)
		{
			string[] array = mSuggestedNames;
			foreach (string text in array)
			{
				ShowInMenu(text);
			}
		}
	}

	private void ShowInMenu(string name)
	{
		KAWidget kAWidget = _NameSuggestionMenu.AddWidget("SuggestedName");
		kAWidget.SetText(name);
		kAWidget.SetVisibility(inVisible: true);
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		if (item == mCloseBtn)
		{
			CloseUI("");
		}
		else if (item.name == "SuggestedName")
		{
			CloseUI(item.GetText());
		}
	}

	public void CloseUI(string name)
	{
		if (mNameSelectedCallback != null)
		{
			mNameSelectedCallback(name);
		}
		mSuggestedNames = null;
		KAUI.RemoveExclusive(this);
		Object.Destroy(base.gameObject);
	}
}
