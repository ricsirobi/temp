public class KAScrollButton : KAButton
{
	public delegate void OnScrollButtonClicked(KAScrollBar scrollBar, KAScrollButton scrollButton, bool isRepeated);

	public KAScrollBar _ScrollBar;

	public bool _DirectionUp;

	public OnScrollButtonClicked onButtonClicked;

	public override void OnClick()
	{
		base.OnClick();
		if (onButtonClicked != null)
		{
			onButtonClicked(_ScrollBar, this, isRepeated: false);
		}
	}

	public override void OnPressRepeated(bool inPressed)
	{
		base.OnPressRepeated(inPressed);
		if (onButtonClicked != null && (_ScrollBar == null || (_ScrollBar != null && _ScrollBar.CanScrollRepeated(inPressed))))
		{
			onButtonClicked(_ScrollBar, this, isRepeated: true);
		}
	}
}
