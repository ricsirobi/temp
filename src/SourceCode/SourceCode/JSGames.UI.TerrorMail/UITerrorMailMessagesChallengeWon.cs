using JSGames.UI.Util;

namespace JSGames.UI.TerrorMail;

public class UITerrorMailMessagesChallengeWon : UIMessagePopulator
{
	public LocaleString _ChallengeWonText = new LocaleString("Challenge won!");

	public LocaleString _ChallengeLostText = new LocaleString("Challenge lost...");

	public UIWidget _ChallengeWon;

	public UIWidget _ChallengeLost;

	public override void Populate(object inData)
	{
		MessageInfo messageInfo = inData as MessageInfo;
		Buddy buddy = BuddyList.pInstance.GetBuddy(messageInfo.FromUserID.ToString());
		if (buddy != null)
		{
			messageInfo.MemberMessage = messageInfo.MemberMessage.Replace("{{BuddyUserName}}", buddy.DisplayName);
		}
		int.TryParse(TaggedMessageHelper.Match(messageInfo.Data)["GameID"], out var result);
		messageInfo.MemberMessage = messageInfo.MemberMessage.Replace("{{Game Name}}", ChallengeInfo.GetGameTitle(result));
		UIUtil.FormatTaggedMessage(ref messageInfo.MemberMessage, messageInfo, new string[1] { "Line1" }, _TagAndDefaultText);
		base.pBody = messageInfo.MemberMessage;
		base.pReceivedDate = string.Format(_TxtReceivedDate.text, messageInfo.CreateDate.ToShortDateString());
		_ChallengeWon.gameObject.SetActive(value: false);
		_ChallengeLost.gameObject.SetActive(value: false);
		if (messageInfo.MessageID == 605 || messageInfo.MessageID == 182 || messageInfo.MessageID == 605)
		{
			base.pSubject = _ChallengeLostText.GetLocalizedString();
			_ChallengeLost.gameObject.SetActive(value: true);
		}
		else
		{
			base.pSubject = _ChallengeWonText.GetLocalizedString();
			_ChallengeWon.gameObject.SetActive(value: true);
		}
		SetFields();
	}
}
