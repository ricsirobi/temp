using UnityEngine;

public class SubscriptionBillboard : KAMonoBase
{
	public Texture _MemberImage;

	public Texture _NonMemberImage;

	public int _MaterialID;

	private bool mInitialized;

	private void Update()
	{
		if (!SubscriptionInfo.pIsReady || mInitialized)
		{
			return;
		}
		mInitialized = true;
		if (SubscriptionInfo.pIsMember)
		{
			if (_MemberImage != null)
			{
				base.renderer.materials[_MaterialID].mainTexture = _MemberImage;
			}
		}
		else if (_NonMemberImage != null)
		{
			base.renderer.materials[_MaterialID].mainTexture = _NonMemberImage;
		}
	}
}
