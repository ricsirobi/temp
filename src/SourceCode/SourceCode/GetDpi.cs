using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class GetDpi : MonoBehaviour
{
	public Text text;

	private void Start()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("dpi=").Append(Screen.dpi).Append("\n");
		stringBuilder.Append("currentResolution height=").Append(Screen.currentResolution.height).Append(" width=")
			.Append(Screen.currentResolution.width);
		text.text = stringBuilder.ToString();
	}
}
