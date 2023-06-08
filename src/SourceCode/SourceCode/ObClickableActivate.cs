public class ObClickableActivate : ObClickable
{
	public string _ObjectName;

	public override void OnActivate()
	{
		if (_ObjectName != null)
		{
			CoCommonLevel._ActivateObjectsOnLoad.Add(_ObjectName);
		}
		base.OnActivate();
	}
}
