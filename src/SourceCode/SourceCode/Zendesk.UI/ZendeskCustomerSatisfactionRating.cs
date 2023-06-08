using UnityEngine;
using UnityEngine.UI;
using Zendesk.Common;
using Zendesk.Internal.Models.Common;
using Zendesk.Internal.Models.Core;
using Zendesk.Internal.Models.Support;
using Zendesk.Providers;

namespace Zendesk.UI;

public class ZendeskCustomerSatisfactionRating : MonoBehaviour
{
	public GameObject goodScorePanel;

	public GameObject badScorePanel;

	private int localScore;

	private string badComment;

	private string goodComment;

	private string feedback;

	private ZendeskSupportProvider provider;

	private Request request;

	private ZendeskMain zMain;

	private GameObject targetGO;

	private GameObject ratingResponseInstantiated;

	private GameObject answerBotCSAT;

	private ZendeskErrorUI zendeskErrorUI;

	private ZendeskLocalizationHandler zendeskLocalizationHandler;

	private ZendeskAuthType zendeskAuthType;

	public Text cSatBad;

	public Text cSatGood;

	public Text cSatMessage;

	public GameObject ratingResponse;

	public Text answerBotLabelText;

	public void Init(Request r, ZendeskSupportProvider zendeskSupportProvider, ZendeskMain zendeskMain, GameObject targetGameObject, ZendeskErrorUI zendeskErrorUI, ZendeskLocalizationHandler localHandler)
	{
		zMain = zendeskMain;
		zendeskAuthType = zendeskMain.zendeskUI.zendeskSupportUI.zendeskAuthType;
		this.zendeskErrorUI = zendeskErrorUI;
		zendeskLocalizationHandler = localHandler;
		SetCSATStrings();
		request = r;
		provider = zendeskSupportProvider;
		targetGO = targetGameObject;
		RateRequestInternal();
	}

	public void RateRequest(int score)
	{
		if (ZendeskLocalStorage.GetSupportRequest(request.Id).score != score)
		{
			localScore = score;
			provider.CustomerSatisfactionRating(CustomerSatisfactionRatingCallback, request.Id, (CustomerSatisfactionRatingScore)localScore, "");
		}
	}

	private void RateRequestInternal()
	{
		Object.Destroy(ratingResponseInstantiated);
		Object.Destroy(answerBotCSAT);
		SupportRequestStorageItem supportRequest = ZendeskLocalStorage.GetSupportRequest(request.Id);
		localScore = supportRequest.score;
		switch ((CustomerSatisfactionRatingScore)supportRequest.score)
		{
		case CustomerSatisfactionRatingScore.Good:
			ratingResponseInstantiated = Object.Instantiate(ratingResponse, targetGO.transform);
			ratingResponseInstantiated.GetComponent<ZendeskTicketResponse>().Init(goodComment, zMain.zendeskUI.zendeskSupportUI, zendeskErrorUI, zendeskLocalizationHandler);
			answerBotCSAT = zMain.zendeskUI.zendeskSupportUI.AddAnswerBotCSAT(feedback);
			zMain.zendeskUI.zendeskSupportUI.ticketResponseContainerList.Add(ratingResponseInstantiated);
			zMain.zendeskUI.zendeskSupportUI.ticketResponseContainerList.Add(answerBotCSAT);
			break;
		case CustomerSatisfactionRatingScore.Bad:
			ratingResponseInstantiated = Object.Instantiate(ratingResponse, targetGO.transform);
			ratingResponseInstantiated.GetComponent<ZendeskTicketResponse>().Init(badComment, zMain.zendeskUI.zendeskSupportUI, zendeskErrorUI, zendeskLocalizationHandler);
			answerBotCSAT = zMain.zendeskUI.zendeskSupportUI.AddAnswerBotCSAT(feedback);
			zMain.zendeskUI.zendeskSupportUI.ticketResponseContainerList.Add(ratingResponseInstantiated);
			zMain.zendeskUI.zendeskSupportUI.ticketResponseContainerList.Add(answerBotCSAT);
			break;
		}
		zMain.zendeskUI.zendeskSupportUI.ScrollDownTicketResponse(forceScroll: true);
	}

	public void ClearCSAT()
	{
		Object.Destroy(ratingResponseInstantiated);
		Object.Destroy(answerBotCSAT);
	}

	private void CustomerSatisfactionRatingCallback(ZendeskResponse<CustomerSatisfactionRating> response)
	{
		if (response.IsError)
		{
			if (!zendeskErrorUI.IfAuthError(response.ErrorResponse, zendeskAuthType))
			{
				zendeskErrorUI.NavigateError(null, true, true);
			}
		}
		else
		{
			ZendeskLocalStorage.SaveSupportRequest(request.Id, isRead: true, localScore, "");
			RateRequestInternal();
		}
	}

	private void SetCSATStrings()
	{
		badComment = zMain.zendeskLocalizationHandler.translationGameObjects["usdk_csat_bad"];
		goodComment = zMain.zendeskLocalizationHandler.translationGameObjects["usdk_csat_good"];
		cSatBad.text = badComment;
		cSatGood.text = goodComment;
		feedback = zMain.zendeskLocalizationHandler.translationGameObjects["usdk_csat_feedback"];
		cSatMessage.text = zMain.zendeskLocalizationHandler.translationGameObjects["usdk_csat_question"];
		answerBotLabelText.text = zendeskLocalizationHandler.translationGameObjects["usdk_bot_label_pascalcase"];
	}
}
