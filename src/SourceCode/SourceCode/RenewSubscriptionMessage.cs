internal class RenewSubscriptionMessage : GenericMessage
{
	public RenewSubscriptionMessage(MessageInfo inMessage)
		: base(inMessage)
	{
	}

	public override void Show()
	{
		_Tagged = true;
		base.Show();
	}
}
