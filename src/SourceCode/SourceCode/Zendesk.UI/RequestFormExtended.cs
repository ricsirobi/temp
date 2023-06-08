using System;
using Zendesk.Internal.Models.Support;

namespace Zendesk.UI;

[Serializable]
public class RequestFormExtended : RequestForm
{
	public long ticketFormId;

	public long subjectId;

	public long messageId;
}
