using System;
using UnityEngine;
using UnityEngine.UI;
using Zendesk.Providers;

namespace Zendesk.UI;

public class ZendeskAnswerBotTicketResponse : MonoBehaviour
{
	public GameObject answerBotResponseGO;

	private Text answerBotResponseText;

	public Text answerBotLabelText;

	private ZendeskSupportProvider supportProvider;

	public void Init(string answerBotMessage, ZendeskErrorUI errorUi, ZendeskLocalizationHandler zendeskLocalizationHandler, bool fade = false)
	{
		try
		{
			answerBotLabelText.text = zendeskLocalizationHandler.translationGameObjects["usdk_bot_label_pascalcase"];
			answerBotResponseText = answerBotResponseGO.GetComponent<Text>();
			answerBotResponseText.text = answerBotMessage;
			base.gameObject.SetActive(value: true);
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}
}
