using UnityEngine;
using UnityEngine.UI;

namespace Zendesk.UI;

public class ZendeskCustomTextFieldScript : ZendeskGenericComponentScript
{
	private Color redOutline;

	private Color greyOutline;

	public InputField inputField;

	public Text validationMessageComponent;

	public GameObject validationPanel;

	public bool required;

	private void Start()
	{
		if (inputField != null)
		{
			inputField.onEndEdit.AddListener(delegate
			{
				ValidateCustomField(inputField);
			});
		}
		ColorUtility.TryParseHtmlString("#D8DCDE", out greyOutline);
		ColorUtility.TryParseHtmlString("#CC3340", out redOutline);
	}

	public void init(long id, string displayText, string validationMessage, string placeholder, bool requiredField, string defaultValidationMessage, string requiredSymbol = "*")
	{
		idCustomField = id;
		if (string.IsNullOrEmpty(validationMessage))
		{
			validationMessage = defaultValidationMessage;
		}
		validationMessageComponent.text = validationMessage;
		required = requiredField;
		labelComponent.text = (required ? (displayText + requiredSymbol) : displayText);
	}

	public void ValidateCustomField(InputField selectedCustomField)
	{
		if (!required)
		{
			return;
		}
		string text = selectedCustomField.text.Trim();
		if (string.IsNullOrEmpty(text) && text.Length == 0)
		{
			selectedCustomField.Select();
			selectedCustomField.GetComponentInParent<Image>().color = redOutline;
			if (!string.IsNullOrEmpty(validationMessageComponent.text))
			{
				validationPanel.SetActive(value: true);
			}
		}
		else
		{
			selectedCustomField.GetComponentInParent<Image>().color = greyOutline;
			validationPanel.SetActive(value: false);
		}
	}
}
