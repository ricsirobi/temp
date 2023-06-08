using UnityEngine;

public class ObPlant : KAMonoBase
{
	private PlantState mPlantState;

	private void Update()
	{
		switch (mPlantState)
		{
		case PlantState.NONE:
			if (base.animation.IsPlaying("Grow"))
			{
				mPlantState = PlantState.GROW;
				break;
			}
			base.animation.CrossFade("Idle");
			base.animation.wrapMode = WrapMode.Loop;
			mPlantState = PlantState.IDLE;
			break;
		case PlantState.GROW:
			if (!base.animation.IsPlaying("Grow"))
			{
				base.animation.CrossFade("Bloom");
				mPlantState = PlantState.BLOOM;
			}
			break;
		case PlantState.BLOOM:
			if (!base.animation.IsPlaying("Bloom"))
			{
				base.animation.CrossFade("Idle");
				base.animation.wrapMode = WrapMode.Loop;
				mPlantState = PlantState.IDLE;
			}
			break;
		case PlantState.IDLE:
			if (!AvAvatar.pObject)
			{
				break;
			}
			if ((AvAvatar.position - base.transform.position).magnitude <= 6f && AvAvatar.pToolbar.activeInHierarchy)
			{
				if (!base.animation.IsPlaying("Dance"))
				{
					base.animation.CrossFade("Dance");
				}
			}
			else if (!base.animation.IsPlaying("Idle"))
			{
				base.animation.CrossFade("Idle");
			}
			break;
		}
	}
}
