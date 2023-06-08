using SquadTactics;

public class UiAdQuizPopUp : KAUIPopup
{
	private KAWidget mBtnAds;

	private KAWidget mBtnOpenQuiz;

	public UiChestMenu _UiChestMenu;

	protected override void Start()
	{
		base.Start();
		mBtnAds = FindItem("BtnAds");
		mBtnOpenQuiz = FindItem("BtnOpenQuiz");
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		if (item == mBtnAds && AdManager.pInstance.AdAvailable(_UiChestMenu._AdEventType, AdType.REWARDED_VIDEO))
		{
			AdManager.DisplayAd(_UiChestMenu._AdEventType, AdType.REWARDED_VIDEO, _UiChestMenu.gameObject);
		}
		else if (item == mBtnOpenQuiz)
		{
			_UiChestMenu.OnClick(_UiChestMenu.pVideoAdchestItem);
		}
		SetVisibility(t: false);
	}
}
