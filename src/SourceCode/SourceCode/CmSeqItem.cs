using System;
using System.Collections.Generic;

public class CmSeqItem : ICloneable
{
	public enum slotType
	{
		blank,
		ghost,
		display
	}

	public uint pRootPos;

	public int pPos;

	public uint pContentItemID;

	public slotType pDisplayType;

	public CmImageList pImage;

	public CmAudioList pVo;

	public List<string> pText;

	public CmRK pRK;

	public object Clone()
	{
		return new CmSeqItem
		{
			pRootPos = pRootPos,
			pPos = pPos,
			pContentItemID = pContentItemID,
			pDisplayType = pDisplayType,
			pImage = pImage,
			pVo = pVo,
			pText = pText,
			pRK = pRK
		};
	}
}
