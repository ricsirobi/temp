using System;
using UnityEngine;
using UnityEngine.UI;

namespace Zendesk.UI;

public class ZendeskTicketConfirmationMessage : MonoBehaviour
{
	public GameObject confirmationMessageGO;

	private Text confirmationMessageText;

	public void Init(string answerBotMessage)
	{
		try
		{
			confirmationMessageText = confirmationMessageGO.GetComponent<Text>();
			confirmationMessageText.text = answerBotMessage;
			base.gameObject.SetActive(value: true);
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}
}
