using UnityEngine;

namespace Xsolla;

public class OpenUrlHelper : MonoBehaviour
{
	public void OpenUrl(string urlToOpen)
	{
		Application.OpenURL(urlToOpen);
	}
}
