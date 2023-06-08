using UnityEngine;

public class BypassServerDown : KAConsole.Command
{
	public BypassServerDown()
		: base("BypassServerDown")
	{
	}

	public override void Execute(string[] args)
	{
		GameObject gameObject = GameObject.Find("ServerDown");
		if ((bool)gameObject)
		{
			Object.Destroy(gameObject);
			ProductStartup.pState = ProductStartup.State.WAITING_FOR_PREFETCH_CHECK;
			ProductStartup productStartup = Object.FindObjectOfType<ProductStartup>();
			if ((bool)productStartup)
			{
				productStartup.enabled = true;
			}
		}
	}
}
