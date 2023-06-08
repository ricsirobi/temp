using UnityEngine;
using UnityEngine.UI;

namespace Zendesk.UI;

public class ZendeskCustomCheckboxFieldScript : ZendeskGenericComponentScript
{
	public Toggle inputField;

	public GameObject background;

	public void init(long id, string displayText)
	{
		idCustomField = id;
		labelComponent.text = displayText;
		inputField.onValueChanged.AddListener(OnToggleValueChanged);
	}

	private void OnToggleValueChanged(bool isOn)
	{
		if (isOn)
		{
			background.GetComponent<Outline>().effectColor = new Color32(31, 115, 183, byte.MaxValue);
			background.GetComponent<RawImage>().color = new Color32(31, 115, 183, byte.MaxValue);
		}
		else
		{
			background.GetComponent<Outline>().effectColor = new Color32(194, 200, 204, byte.MaxValue);
			background.GetComponent<RawImage>().color = Color.white;
		}
	}
}
