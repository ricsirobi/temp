using System;

namespace Xsolla;

public struct LabelValue
{
	public string label;

	public string value;

	public string actionLabel;

	public Action action;

	public LabelValue(string pLabel, string pValue, string pActionLable = null, Action pAction = null)
	{
		label = pLabel;
		value = pValue;
		actionLabel = pActionLable;
		action = pAction;
	}
}
