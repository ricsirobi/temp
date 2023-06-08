using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Zendesk.Internal.Models.Support;

namespace Zendesk.UI;

public class ZendeskCustomDropdownFieldScript : ZendeskGenericComponentScript
{
	private Color redOutline;

	private Color greyOutline;

	public Dropdown dropdown;

	public Text validationMessageComponent;

	public GameObject validationPanel;

	public bool required;

	[HideInInspector]
	public DropdownOptions[] dropdownOptions;

	private void Start()
	{
		if (dropdown != null)
		{
			dropdown.onValueChanged.AddListener(delegate
			{
				ValidateCustomField();
			});
		}
		ColorUtility.TryParseHtmlString("#D8DCDE", out greyOutline);
		ColorUtility.TryParseHtmlString("#CC3340", out redOutline);
	}

	public void init(long id, string displayText, DropdownOptions[] options, string validationMessage, string placeholder, bool requiredField, string requiredSymbol = "*")
	{
		idCustomField = id;
		validationMessageComponent.text = validationMessage;
		required = requiredField;
		labelComponent.text = (required ? (displayText + requiredSymbol) : displayText);
		dropdownOptions = options;
		dropdown.AddOptions(options.Select((DropdownOptions a) => a.Text).ToList());
	}

	public void ValidateCustomField()
	{
		if (required)
		{
			if (dropdown.captionText.text == "-")
			{
				dropdown.GetComponentInChildren<Image>().color = redOutline;
				validationPanel.SetActive(value: true);
			}
			else
			{
				dropdown.GetComponentInChildren<Image>().color = greyOutline;
				validationPanel.SetActive(value: false);
			}
		}
	}
}
