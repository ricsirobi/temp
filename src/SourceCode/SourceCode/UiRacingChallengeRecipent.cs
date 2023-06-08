using UnityEngine;

public class UiRacingChallengeRecipent : KAUI
{
	public class AvatarPic
	{
		private KAWidget mItem;

		public AvatarPic(KAWidget item)
		{
			mItem = item;
		}

		public void PhotoCallback(Texture tex, object inUserData)
		{
			mItem.SetTexture(tex);
		}
	}

	protected override void Start()
	{
		base.Start();
	}

	protected override void Update()
	{
		base.Update();
	}
}
