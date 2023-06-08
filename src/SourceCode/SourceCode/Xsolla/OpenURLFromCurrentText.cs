using UnityEngine;
using UnityEngine.UI;

namespace Xsolla;

public class OpenURLFromCurrentText : MonoBehaviour
{
	public Text text;

	public void OpenPhoneURL()
	{
		Application.OpenURL("tel://" + text.text);
	}

	public void OpenEmailURL()
	{
		Application.OpenURL("mailto://" + text.text);
	}
}
