using UnityEngine;
using UnityEngine.UI;

public class TextColorController : MonoBehaviour
{
	private const float onePercent = 2.55f;

	private Color thisBaseTextColor;

	private Color thisTextColor;

	public int Red = 255;

	public int Green = 255;

	public int Blue = 255;

	private void Start()
	{
		thisTextColor = GetComponent<Text>().color;
		thisBaseTextColor = new Color(thisTextColor.r, thisTextColor.g, thisTextColor.b);
	}

	public void SelectBaseColor(bool b)
	{
		if (b)
		{
			GetComponent<Text>().color = thisBaseTextColor;
		}
		else
		{
			GetComponent<Text>().color = new Color((float)Red / 2.55f, (float)Green / 2.55f, (float)Blue / 2.55f);
		}
	}
}
