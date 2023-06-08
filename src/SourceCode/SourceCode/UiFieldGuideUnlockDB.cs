using UnityEngine;

public class UiFieldGuideUnlockDB : KAUIGenericDB
{
	public KAWidget _PaperObject;

	public LocaleString _UnlockText = new LocaleString("Field Guide Updated!");

	public LocaleString _NotificationTitleText = new LocaleString("Notification");

	public float _Duration = 2f;

	protected override void Start()
	{
		base.Start();
		string localizedString = _UnlockText.GetLocalizedString();
		string text = "";
		if (UiFieldGuide.pRecentlyUnlockedChapters != null)
		{
			foreach (FieldGuideChapter pRecentlyUnlockedChapter in UiFieldGuide.pRecentlyUnlockedChapters)
			{
				text = text + localizedString + " " + pRecentlyUnlockedChapter.Title.Data?.ToString() + "\n";
			}
		}
		SetText(text, interactive: false);
		SetTitle(_NotificationTitleText.GetLocalizedString());
	}

	public override void EndMoveTo(KAWidget widget)
	{
		base.EndMoveTo(widget);
		KAUI.RemoveExclusive(this);
		Object.Destroy(base.gameObject);
	}
}
