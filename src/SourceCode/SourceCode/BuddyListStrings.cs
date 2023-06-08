using System;

[Serializable]
public class BuddyListStrings
{
	public LocaleString _InviteSentText = new LocaleString("Invite sent!");

	public LocaleString _JoinBuddyErrorText = new LocaleString("You cannot visit your Friend at this time.");

	public LocaleString _BuddyLeftText = new LocaleString("Your Friend left this area.");

	public LocaleString _IgnorePlayerText = new LocaleString("Are you sure you'd like to ignore this Friend?");

	public LocaleString _DeletePlayerText = new LocaleString("Are you sure you'd like to remove this Friend from your Friend list?");

	public LocaleString _GenericErrorText = new LocaleString("You cannot do that at this time.  Please try again later.");

	public LocaleString _BuddyCodeSuccessText = new LocaleString("Friend Code accepted.  Waiting for approval.");

	public LocaleString _BuddyListFullText = new LocaleString("Your Friend list is full. You cannot add a new Friend.");

	public LocaleString _OtherBuddyListFullText = new LocaleString("That Friend's Friend list is full.");

	public LocaleString _BlockedByOtherText = new LocaleString("That Friend is not accepting Friend requests.");

	public LocaleString _AlreadyInListText = new LocaleString("That Friend is already on your list.");

	public LocaleString _WaitingForApprovalText = new LocaleString("[Review] Waiting for approval from your friend.");

	public LocaleString _InvalidBuddyCodeText = new LocaleString("That is not a valid Friend Code.");

	public LocaleString _AddSelfText = new LocaleString("You cannot add yourself.");
}
