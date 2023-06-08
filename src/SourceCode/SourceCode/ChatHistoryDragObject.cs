public class ChatHistoryDragObject : UIDragObject
{
	protected override void OnPress(bool inPressed)
	{
		base.OnPress(inPressed);
		if (UiJoystick.pInstance != null)
		{
			KAInput.ShowJoystick(UiJoystick.pInstance.pPos, !inPressed);
		}
	}
}
