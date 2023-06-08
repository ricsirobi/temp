public class UiAnimStyleFallThrough : UiAnimStyle
{
	public override void StartEffect()
	{
		base.StartEffect();
		if (mCompactUI.pExpanded)
		{
			mIndex = base.pChildList.Length - 1;
		}
		else
		{
			mIndex = 0;
		}
	}

	protected override void Update()
	{
		base.Update();
		if (mIndex < base.pChildList.Length && !base.pChildList[mIndex].pIsTweening)
		{
			if (mCompactUI.pExpanded)
			{
				base.pChildList[mIndex].MoveTo(base.pChildInitialPos[mIndex] + base.pChildTargetDirectionList[mIndex] * mCompactUI._Radius, mTime);
			}
			else
			{
				base.pChildList[mIndex].MoveTo(base.pChildInitialPos[mIndex], mTime);
			}
		}
	}

	protected override void OnWidgetMoveToDone(KAWidget inWidget)
	{
		base.OnWidgetMoveToDone(inWidget);
		if (mCompactUI.pExpanded)
		{
			mIndex--;
			if (mIndex < 0)
			{
				OnEffectDone();
			}
		}
		else
		{
			mIndex++;
			if (mIndex >= base.pChildList.Length)
			{
				OnEffectDone();
			}
		}
	}
}
