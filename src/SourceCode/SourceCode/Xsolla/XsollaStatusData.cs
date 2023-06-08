using SimpleJSON;

namespace Xsolla;

public class XsollaStatusData
{
	public enum Status
	{
		CREATED,
		INVOICE,
		DONE,
		CANCELED,
		UNKNOWN
	}

	public static string SD_EMAIL = "email";

	public static string SD_INVOICE = "invoice";

	public static string SD_USER_ID = "v1";

	public static string SD_STATE = "state";

	public static string SD_STATETEXT = "stateText";

	public static string SD_PID = "pid";

	public static string SD_OUT = "out";

	private string email;

	private long invoice;

	private Status status;

	private string userId;

	private string currencyAmount;

	private long paymentSystemId;

	public XsollaStatusData(JSONNode statusDataNode)
	{
		email = statusDataNode[SD_EMAIL];
		invoice = statusDataNode[SD_INVOICE].AsInt;
		userId = statusDataNode[SD_USER_ID];
		currencyAmount = statusDataNode[SD_OUT];
		switch (statusDataNode[SD_STATE].AsInt)
		{
		case 0:
			status = Status.CREATED;
			break;
		case 1:
			status = Status.INVOICE;
			break;
		case 2:
			status = Status.CANCELED;
			break;
		case 3:
			status = Status.DONE;
			break;
		default:
			status = Status.UNKNOWN;
			break;
		}
	}

	public string GetInvoice()
	{
		return invoice.ToString();
	}

	public Status GetStatus()
	{
		return status;
	}

	public override string ToString()
	{
		return string.Format("[XsollaStatusData]\n email= " + email + "\n invoice= " + invoice + "\n userId= " + userId + "\n currencyAmount= " + currencyAmount + "\n status= " + status);
	}
}
