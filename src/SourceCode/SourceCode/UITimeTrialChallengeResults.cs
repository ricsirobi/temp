using UnityEngine;

public class UITimeTrialChallengeResults : KAUI
{
	public delegate void TimeTrialResultsClosed();

	public LocaleString _WinTitleText = new LocaleString("Congratulations!");

	public LocaleString _WinMessgeText = new LocaleString("CHALLENGE WON");

	public LocaleString _LoseTitleText = new LocaleString("Better luck next time…");

	public LocaleString _LoseMessageText = new LocaleString("CHALLENGE LOST");

	public KAWidget _TitleText;

	public KAWidget _MessgeText;

	public KAWidget _BannerBkg;

	public KAWidget _TrophiesResults;

	public Color _WinTextColor = new Color(0f, 1f, 0f, 1f);

	public Color _WinBannerColor = new Color(0f, 0.7529f, 0.5f, 1f);

	public Color _LoseTextColor = new Color(1f, 0f, 0f, 1f);

	public Color _LoseBannerColor = new Color(1f, 0.6705883f, 0.2470588f, 1f);

	public KAWidget _PlayerTime;

	public KAWidget _OpponentTime;

	public event TimeTrialResultsClosed OnResultsClosed;

	protected override void Start()
	{
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget.name == "ContinueBtn")
		{
			SetVisibility(inVisible: false);
			this.OnResultsClosed();
		}
	}
}
