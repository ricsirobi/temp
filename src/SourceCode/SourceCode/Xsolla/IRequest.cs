using System.Collections;
using System.Collections.Generic;

namespace Xsolla;

public interface IRequest
{
	IRequest Prepare(bool isSandbox, Dictionary<string, object> requestParams);

	IEnumerator Execute();
}
