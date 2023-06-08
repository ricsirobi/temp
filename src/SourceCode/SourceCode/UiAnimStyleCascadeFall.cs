public class UiAnimStyleCascadeFall : UiAnimStyle
{
	protected int mDoneCount;

	public override void StartEffect()
	{
		base.StartEffect();
		if (mCompactUI.pExpanded)
		{
			mDoneCount = base.pChildList.Length - 1;
			mIndex = mDoneCount;
		}
		else
		{
			mDoneCount = 0;
			mIndex = 0;
		}
	}

	protected override void Update()
	{
		base.Update();
		if (mDoneCount >= base.pChildList.Length)
		{
			return;
		}
		if (mCompactUI.pExpanded)
		{
			for (int i = mDoneCount; i < base.pChildList.Length; i++)
			{
				if (mDoneCount - 1 < 0)
				{
					base.pChildList[i].MoveTo(base.pChildInitialPos[0] + base.pChildTargetDirectionList[0] * mCompactUI._Radius, mTime);
				}
				else
				{
					base.pChildList[i].MoveTo(base.pChildInitialPos[mDoneCount - 1], mTime);
				}
			}
		}
		else
		{
			for (int j = mDoneCount; j < base.pChildList.Length; j++)
			{
				base.pChildList[j].MoveTo(base.pChildInitialPos[mDoneCount], mTime);
			}
		}
	}

	protected override void OnWidgetMoveToDone(KAWidget inWidget)
	{
		base.OnWidgetMoveToDone(inWidget);
		if (mCompactUI.pExpanded)
		{
			mIndex++;
			if (mIndex >= base.pChildList.Length)
			{
				mDoneCount--;
				mIndex = mDoneCount;
				if (mDoneCount < 0)
				{
					OnEffectDone();
				}
			}
			return;
		}
		mIndex++;
		if (mIndex >= base.pChildList.Length)
		{
			mDoneCount++;
			mIndex = mDoneCount;
			if (mDoneCount >= base.pChildList.Length)
			{
				OnEffectDone();
			}
		}
	}
}
