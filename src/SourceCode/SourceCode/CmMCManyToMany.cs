public class CmMCManyToMany : CmContentBase
{
	public CmQuestionItem[] pGroupList;

	public bool pbDuped;

	public CmMCManyToMany()
	{
		pContentType = ContentTypes.MCMany;
	}
}
