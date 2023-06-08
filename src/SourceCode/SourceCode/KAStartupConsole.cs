using UnityEngine;

public class KAStartupConsole : KAConsole
{
	public override void Awake()
	{
		if (KAConsole.mObject != null)
		{
			if (KAConsole.mObject.GetComponent<KAConsole>() != null)
			{
				Object.Destroy(base.gameObject);
				return;
			}
			Object.Destroy(KAConsole.mObject);
			KAConsole.mObject = null;
		}
		KAConsole.mObject = base.gameObject;
	}

	public override void Start()
	{
		base.Start();
		KAConsole.AddCommand(new BypassServerDown());
	}
}
