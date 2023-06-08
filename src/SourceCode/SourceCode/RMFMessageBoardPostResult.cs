using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "RMFMessageBoardPostResult")]
public class RMFMessageBoardPostResult : MessageBoardPostResult
{
	public string ResponseID;

	public int MaxResult;

	public MessageResultStatus resultStatus;

	public string filteredMessage;

	public string originalMessage;

	public RMFMessageBoardPostResult()
	{
		resultStatus = MessageResultStatus.FAILED;
	}
}
