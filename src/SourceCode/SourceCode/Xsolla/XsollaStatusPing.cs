using SimpleJSON;

namespace Xsolla;

public class XsollaStatusPing : IParseble
{
	private string _status;

	private bool _final;

	private int _elapsedTime;

	private XsollaStatus.Group _group;

	public IParseble Parse(JSONNode rootNode)
	{
		if (rootNode != null && !"null".Equals(rootNode.ToString()))
		{
			_status = rootNode["status"];
			_final = rootNode["final"].AsBool;
			_elapsedTime = rootNode["elapsedTime"].AsInt;
			switch ((string)rootNode["group"])
			{
			case "invoice":
				_group = XsollaStatus.Group.INVOICE;
				break;
			case "done":
				_group = XsollaStatus.Group.DONE;
				break;
			case "delivering":
				_group = XsollaStatus.Group.DELIVERING;
				break;
			case "troubled":
				_group = XsollaStatus.Group.TROUBLED;
				break;
			default:
				_group = XsollaStatus.Group.UNKNOWN;
				break;
			}
		}
		return this;
	}

	public XsollaStatus.Group GetGroup()
	{
		return _group;
	}

	public string GetStatus()
	{
		return _status;
	}

	public bool isFinal()
	{
		return _final;
	}

	public int GetElapsedTiem()
	{
		return _elapsedTime;
	}
}
