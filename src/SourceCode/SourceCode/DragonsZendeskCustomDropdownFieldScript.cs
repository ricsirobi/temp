using System.Collections;
using UnityEngine;
using Zendesk.Internal.Models.Support;
using Zendesk.UI;

public class DragonsZendeskCustomDropdownFieldScript : ZendeskCustomDropdownFieldScript
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
		yield return new WaitUntil(() => dropdown.options.Count <= 0);
		foreach (RequestFormField field in mZendeskSupportUI.requestForm.Fields)
		{
			if (field.Id == idCustomField)
			{
				dropdown.value = DragonsZendeskSupport.pInstance.FetchDropDownIndex(field.Label, field.DropDownOptions);
				break;
			}
		}
	}
}
