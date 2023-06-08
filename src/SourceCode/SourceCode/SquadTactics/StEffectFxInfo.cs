using System;
using UnityEngine;

namespace SquadTactics;

[Serializable]
public class StEffectFxInfo
{
	public FxData _InFX;

	public FxData _LoopFX;

	public FxData _OutFX;

	public StEffectFxInfo(FxData inFxData, FxData loopFxData, FxData outFxData, Transform parent)
	{
		_InFX = new FxData(inFxData, parent);
		_LoopFX = new FxData(loopFxData, parent);
		_OutFX = new FxData(outFxData, parent);
	}

	public StEffectFxInfo(StEffectFxInfo effectFxInfo, Transform parent)
		: this(effectFxInfo._InFX, effectFxInfo._LoopFX, effectFxInfo._OutFX, parent)
	{
	}
}
