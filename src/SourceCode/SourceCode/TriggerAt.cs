using System;

[Serializable]
public class TriggerAt
{
	public int _Hour = 1;

	public int _Min;

	public bool IsTriggered()
	{
		if (_Hour == DateTime.Now.Hour && _Min == DateTime.Now.Minute)
		{
			UtDebug.Log("IsTriggered true at : " + DateTime.Now, 200);
			return true;
		}
		return false;
	}
}
