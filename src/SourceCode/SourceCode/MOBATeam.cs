using System.Collections.Generic;

public class MOBATeam
{
	public string pName;

	private List<string> mMemberIds = new List<string>();

	private string mLeaderId;

	public List<string> pMemberIds
	{
		get
		{
			return mMemberIds;
		}
		set
		{
			mMemberIds = value;
			mLeaderId = mMemberIds[0];
		}
	}

	public string pLeaderId => mLeaderId;
}
