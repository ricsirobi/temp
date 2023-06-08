using UnityEngine;

public class MOBASDManager : MOBAEntity
{
	public GameObject _Team1SheepPath;

	public GameObject _Team2SheepPath;

	public int _SheepPerTeam = 5;

	[Replicated]
	public static int pState;

	public static MOBASDManager pInstance;

	protected override void Awake()
	{
		pInstance = this;
		base.Awake();
	}

	public override void Init()
	{
		pState = 0;
		base.Init();
		AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
		if ((bool)component)
		{
			component._NoFlyingLanding = true;
		}
	}

	protected override void EntityUpdate(bool bIsAuthority)
	{
		if (bIsAuthority && pState == 0)
		{
			pState = 1;
		}
	}
}
