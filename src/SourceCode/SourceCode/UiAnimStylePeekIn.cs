using UnityEngine;

public class UiAnimStylePeekIn : UiAnimStyle
{
	public Vector3 _Offset = Vector3.zero;

	public float _Delay = 1f;

	protected override void Start()
	{
		base.Start();
	}

	public override void StartEffect()
	{
		base.StartEffect();
		mIndex = 0;
		if (mCompactUI.pExpanded)
		{
			for (int num = base.pChildList.Length - 1; num >= 0; num--)
			{
				base.pChildList[num].MoveTo(base.pChildInitialPos[num] + _Offset, mTime + _Delay * (float)num);
			}
		}
		else
		{
			for (int i = 0; i < base.pChildList.Length; i++)
			{
				base.pChildList[i].MoveTo(base.pChildInitialPos[i], mTime + _Delay * (float)i);
			}
		}
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
