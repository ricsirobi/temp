using UnityEngine;
using UnityEngine.UI;

public class FavoriteController : MonoBehaviour
{
	public Toggle toggle;

	public Text text;

	private void Start()
	{
		ChangeValue(b: false);
		toggle.onValueChanged.AddListener(delegate(bool b)
		{
			ChangeValue(b);
		});
	}

	private void Update()
	{
	}

	public void ChangeValue(bool b)
	{
		if (toggle.isOn)
		{
			text.text = "\ue01e";
		}
		else
		{
			text.text = "\ue01d";
		}
	}
}
