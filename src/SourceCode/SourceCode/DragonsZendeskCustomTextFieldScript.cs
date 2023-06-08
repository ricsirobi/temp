using System.Collections;
using UnityEngine;
using Zendesk.Internal.Models.Support;
using Zendesk.UI;

public class DragonsZendeskCustomTextFieldScript : ZendeskCustomTextFieldScript
{
	private ZendeskSupportUI mZendeskSupportUI;

	private Coroutine mPrepopulateFieldCoroutine;

	public void Awake()
	{
		mZendeskSupportUI = Object.FindObjectOfType<ZendeskSupportUI>();
		if (mPrepopulateFieldCoroutine != null)
		{
			StopCoroutine(mPrepopulateFieldCoroutine);
		}
		mPrepopulateFieldCoroutine = StartCoroutine(PrePopulateField());
	}

	private IEnumerator PrePopulateField()
	{
		if (!(mZendeskSupportUI != null))
		{
			yield break;
		}
		yield return new WaitUntil(() => !string.IsNullOrEmpty(labelComponent.text));
		foreach (RequestFormField field in mZendeskSupportUI.requestForm.Fields)
		{
			if (field.Id == idCustomField)
			{
				inputField.text = DragonsZendeskSupport.pInstance.FetchPrepopulatedString(field.Label);
				inputField.interactable = string.IsNullOrEmpty(inputField.text);
				break;
			}
		}
	}
}
