public class CmPairItem : CmDisplayArea
{
	public int pPairID;

	public CmPairItem(CmDisplayArea da)
	{
		pVo = da.pVo;
		pImage = da.pImage;
		pText = da.pText;
		pRK = da.pRK;
		pID = da.pID;
	}
}
