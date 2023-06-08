using System;

[Serializable]
public class BuddyMessageStrings
{
	public LocaleString _NoDisplayNameText = new LocaleString("A friend");

	public LocaleString _BuddyLocationErrorText = new LocaleString("You cannot visit your buddy at this time.");

	public LocaleString _GenericErrorText = new LocaleString("You cannot do that at this time.  Please try again later.");

	public LocaleString _CoinsAwardedText = new LocaleString("Your buddy code has been entered by {{NumBuddies}} of your best buddies. You have earned {{Coins}} coins!");

	public LocaleString _FriendRequestTitleText = new LocaleString("Friend Request");

	public LocaleString _JoinClanRequestTitleText = new LocaleString("Clan Request");

	public LocaleString _InviteNotValidText = new LocaleString("This invite is no longer valid.");

	public LocaleString _JoinGroupConfirmText = new LocaleString("Doing this will remove you from your current clan, are you sure?");
}
