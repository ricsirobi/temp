public class UiAnimStyleSlide : UiAnimStyle
{
	public override void StartEffect()
	{
		base.StartEffect();
		mIndex = 0;
		for (int i = 0; i < base.pChildList.Length; i++)
		{
			if (mCompactUI.pExpanded)
			{
				base.pChildList[i].MoveTo(base.pChildInitialPos[i] + base.pChildTargetDirectionList[i] * mCompactUI._Radius, mTime);
			}
			else
			{
				base.pChildList[i].MoveTo(base.pChildInitialPos[i], mTime);
			}
		}
	}

	protected override void Update()
	{
		base.Update();
	}

	protected override void OnWidgetMoveToDone(KAWidget inWidget)
	{
		base.OnWidgetMoveToDone(inWidget);
		mIndex++;
		if (mIndex >= base.pChildList.Length)
		{
			OnEffectDone();
		}
	}
}
