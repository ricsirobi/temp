using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using SFB;
using UnityEngine;
using UnityEngine.UI;
using Zendesk.Common;
using Zendesk.Internal.Models.Common;
using Zendesk.Internal.Models.Core;
using Zendesk.Internal.Models.HelpCenter;
using Zendesk.Internal.Models.Support;

namespace Zendesk.UI;

public class ZendeskSupportUI : MonoBehaviour
{
	public GameObject attachmentPrefab;

	public List<string> attachmentPaths = new List<string>();

	public GameObject ticketResponseAttachmentCountContainer;

	public Text ticketResponseAttachmentCountText;

	public GameObject createRequestAttachmentCountContainer;

	public Text createRequestAttachmentCountText;

	public GameObject attachmentsView;

	public GameObject attachmentButton;

	public GameObject sendCommentButton;

	public GameObject attachmentsViewNonConversational;

	public GameObject addCommentButtonImage;

	public GameObject agentResponsePrefab;

	public GameObject answerBotResponsePrefab;

	public GameObject ticketConfirmationMessagePrefab;

	public GameObject backButtonContainer;

	public Button createRequestFooterPanelSendButton;

	public Button createRequestFooterPanelAttachmentButton;

	public GameObject createRequestPanel;

	public GameObject createRequestPanelFieldContainer;

	public GameObject createRequestPanelAttachmentLine;

	public GameObject createRequestPanelEmailValidation;

	public GameObject createRequestPanelEmailContainer;

	public GameObject createRequestPanelFeedbackToast;

	public GameObject createRequestPanelFeedbackPanel;

	public Text createRequestPanelFeedbackPanelTitle;

	public Text createRequestPanelFeedbackPanelDescription;

	public Text createRequestPanelFeedbackPanelButtonText;

	public InputField createRequestPanelInputEmail;

	public InputField createRequestPanelInputMessage;

	public Text createRequestPanelNameText;

	public Text createRequestPanelInvalidNameText;

	public Text createRequestPanelEmailText;

	public Text createRequestPanelInvalidEmailText;

	public Text createRequestPanelMessageText;

	public Text typeMessageText;

	public Text createRequestPanelInvalidMessageText;

	public InputField createRequestPanelInputName;

	public GameObject createRequestPanelMessageValidation;

	public GameObject createRequestPanelNameValidation;

	public GameObject createRequestAttachmentLine;

	public GameObject createRequestPanelNameContainer;

	public GameObject CustomTextFieldPrefab;

	public GameObject CustomTextMultilineFieldPrefab;

	public GameObject CustomCheckboxFieldPrefab;

	public GameObject CustomNumericFieldPrefab;

	public GameObject CustomDecimalFieldPrefab;

	public GameObject CustomDropdownFieldPrefab;

	public TextAsset customFieldTranslations;

	public TextAsset customFieldsFile;

	public GameObject endOfConversationPrefab;

	public GameObject endUserResponsePrefab;

	public GameObject footerClosedStatus;

	public GameObject footerLineMain;

	public Text headerTitle;

	public GameObject messageDatePrefab;

	public GameObject myTicketsFooter;

	public Text myTicketsFooterNeedHelpText;

	public Text myTicketsFooterContactUsButtonText;

	public Text ticketResponseFooterContactUsButtonText;

	public Text conversationalFooterNeedHelpText;

	public GameObject myTicketsPanel;

	public GameObject newCommentEndUser;

	public GameObject noTicketsFooter;

	public GameObject noTicketsPanel;

	public Text noTicketsPanelGeneralInfoText;

	public Text noTicketsPanelStartConversationText;

	public RequestFormExtended requestForm;

	public GameObject satisfactionRatingPrefab;

	public GameObject satisfactionRatingCommentPrefab;

	public GameObject solvedConversationPrefab;

	public GameObject ticketContainerPrefab;

	internal string ticketId;

	public GameObject ticketListView;

	public List<GameObject> ticketResponseContainerList = new List<GameObject>();

	public GameObject ticketResponseListView;

	public GameObject ticketResponseScrollView;

	public GameObject ticketResponsePanel;

	public GameObject createRequestPanelMessageContainer;

	public GameObject createRequestScrollView;

	public Dictionary<long, Texture2D> ticketContentImageIds = new Dictionary<long, Texture2D>();

	public List<Coroutine> ticketConfirmationMessageCoroutines = new List<Coroutine>();

	[HideInInspector]
	public ZendeskAuthType zendeskAuthType = ZendeskAuthType.Anonymous;

	public GameObject keyboardBackground;

	public Color keyboardBackgroundColor;

	public GameObject headerPanel;

	public bool alwaysShowNameAndEmail;

	public string requiredSymbol = "*";

	private readonly List<string> tags = new List<string>();

	private string authScreenErrorTitle;

	private string authScreenErrorBody;

	private string jwtIntegratorError;

	private bool isCsatEnabled;

	private string ticketConfirmationMessageString = "";

	private bool isConversationsEnabled;

	private ZendeskErrorUI zendeskErrorUI;

	private ZendeskMain zendeskMain;

	private ZendeskUI zendeskUI;

	private ZendeskLocalizationHandler zendeskLocalizationHandler;

	private Dictionary<GameObject, FieldType> customFieldsPrefabs = new Dictionary<GameObject, FieldType>();

	private bool emailValid = true;

	private bool errorField;

	private bool customFieldValid = true;

	private DateTime lastCommentDatetime;

	private bool messageValid = true;

	private bool nameValid = true;

	private DateTime prevDate = DateTime.MinValue;

	private Color redOutline;

	private Request request;

	private Color greyOutline;

	private GameObject satisfactionRatingPrefabInstatiated;

	private List<GameObject> ticketContainersList = new List<GameObject>();

	private bool isUserAuthenticatedBefore;

	private long maxAttachmentSize;

	private ZendeskLocalizationHandler zendeskCustomFieldLocalizationHandler;

	private string labelKey = "usdk_cf_{cf-id}_label";

	private string validationMessageKey = "usdk_cf_{cf-id}_validation_message";

	private List<CustomField> customFields;

	private GameObject zendeskCanvas;

	private Action OnRequestSentSuccess;

	public string customFieldsWarning => "Please refer to the documentation before adding, or making changes to custom fields.";

	public void InitWithErrorHandler(ZendeskMain zendeskMain, ZendeskUI zendeskUI, ZendeskErrorUI zendeskErrorUI)
	{
		this.zendeskMain = zendeskMain;
		this.zendeskUI = zendeskUI;
		this.zendeskErrorUI = zendeskErrorUI;
		zendeskCanvas = this.zendeskUI.zendeskCanvas;
	}

	public void Init(ZendeskMain zendeskMain, ZendeskUI zendeskUI, ZendeskSettings zendeskSettings, ZendeskErrorUI zendeskErrorUI, string commonTags = "", Dictionary<long, string> invisibleCustomFields = null)
	{
		try
		{
			this.zendeskMain = zendeskMain;
			zendeskLocalizationHandler = zendeskMain.GetComponent<ZendeskLocalizationHandler>();
			this.zendeskUI = zendeskUI;
			zendeskCanvas = this.zendeskUI.zendeskCanvas;
			this.zendeskErrorUI = zendeskErrorUI;
			SetSupportStrings();
			maxAttachmentSize = zendeskSettings.Support.Attachments.MaxAttachmentSize;
			isCsatEnabled = zendeskSettings.Support.Csat.Enabled;
			isConversationsEnabled = zendeskSettings.Support.Conversations.Enabled;
			ticketConfirmationMessageString = zendeskSettings.Support.SystemMessage;
			if (zendeskSettings.Core.Authentication == ZendeskAuthType.Jwt.Value)
			{
				zendeskAuthType = ZendeskAuthType.Jwt;
			}
			if (!string.IsNullOrEmpty(commonTags))
			{
				tags.AddRange(commonTags.Split(',').ToList());
			}
			if (invisibleCustomFields != null)
			{
				SetInvisibleCustomFields(invisibleCustomFields);
			}
			if (isConversationsEnabled)
			{
				newCommentEndUser.GetComponent<InputField>().onValueChanged.AddListener(delegate
				{
					CheckAvailabilityOfSendButton(null);
				});
			}
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	private void SetSupportStrings()
	{
		myTicketsFooterNeedHelpText.text = zendeskMain.zendeskLocalizationHandler.translationGameObjects["usdk_need_more_help_label"];
		conversationalFooterNeedHelpText.text = zendeskMain.zendeskLocalizationHandler.translationGameObjects["usdk_need_more_help_label"];
		myTicketsFooterContactUsButtonText.text = zendeskMain.zendeskLocalizationHandler.translationGameObjects["usdk_get_in_touch_label_saddle_bar"];
		ticketResponseFooterContactUsButtonText.text = zendeskMain.zendeskLocalizationHandler.translationGameObjects["usdk_get_in_touch_label_saddle_bar"];
		noTicketsPanelGeneralInfoText.text = zendeskMain.zendeskLocalizationHandler.translationGameObjects["usdk_help_center_description"];
		noTicketsPanelStartConversationText.text = zendeskMain.zendeskLocalizationHandler.translationGameObjects["usdk_get_in_touch_label_saddle_bar"];
		createRequestPanelNameText.text = zendeskMain.zendeskLocalizationHandler.translationGameObjects["usdk_label_name"];
		createRequestPanelInvalidNameText.text = zendeskMain.zendeskLocalizationHandler.translationGameObjects["usdk_required_field_label"];
		createRequestPanelEmailText.text = zendeskMain.zendeskLocalizationHandler.translationGameObjects["usdk_label_email"];
		createRequestPanelInvalidEmailText.text = zendeskMain.zendeskLocalizationHandler.translationGameObjects["usdk_enter_valid_email"];
		createRequestPanelMessageText.text = zendeskMain.zendeskLocalizationHandler.translationGameObjects["usdk_label_message"];
		createRequestPanelInvalidMessageText.text = zendeskMain.zendeskLocalizationHandler.translationGameObjects["usdk_required_field_label"];
		typeMessageText.text = zendeskMain.zendeskLocalizationHandler.translationGameObjects["usdk_type_a_message_label_without_dots"];
		createRequestPanelFeedbackPanelTitle.text = zendeskMain.zendeskLocalizationHandler.translationGameObjects["usdk_got_the_message_label"];
		createRequestPanelFeedbackPanelDescription.text = (string.IsNullOrEmpty(ticketConfirmationMessageString) ? zendeskMain.zendeskLocalizationHandler.translationGameObjects["usdk_create_request_feedback_message_label"] : ticketConfirmationMessageString);
		createRequestPanelFeedbackPanelButtonText.text = zendeskMain.zendeskLocalizationHandler.translationGameObjects["usdk_back_button_label"];
		authScreenErrorTitle = zendeskLocalizationHandler.translationGameObjects["usdk_jwt_error_message"];
		authScreenErrorBody = zendeskLocalizationHandler.translationGameObjects["usdk_error_page_message_new_design"];
		jwtIntegratorError = zendeskLocalizationHandler.translationGameObjects["usdk_jwt_token_integrator_error"];
	}

	public void OpenSupportPanel(string requestTags = "")
	{
		try
		{
			if (!zendeskCanvas.activeSelf)
			{
				zendeskCanvas.SetActive(value: true);
			}
			base.gameObject.SetActive(value: true);
			ShowMyTicketsWindow();
			tags.AddRange(requestTags.Split(',').ToList());
		}
		catch
		{
			zendeskErrorUI.NavigateError(null, true, true);
		}
	}

	private void InitCreateRequestPanel()
	{
		try
		{
			isUserAuthenticatedBefore = zendeskMain.IsUserAuthenticatedBefore();
			createRequestFooterPanelSendButton.interactable = true;
			createRequestFooterPanelAttachmentButton.interactable = true;
			createRequestPanelFeedbackToast.SetActive(value: false);
			createRequestPanelFeedbackPanel.SetActive(value: false);
			createRequestPanelInputEmail.interactable = true;
			createRequestPanelInputName.interactable = true;
			createRequestPanelInputMessage.interactable = true;
			ColorUtility.TryParseHtmlString("#D8DCDE", out greyOutline);
			ColorUtility.TryParseHtmlString("#CC3340", out redOutline);
			createRequestPanelInputMessage.GetComponentInParent<Image>().color = greyOutline;
			createRequestPanelInputMessage.text = "";
			createRequestPanelMessageValidation.SetActive(value: false);
			float num = 0f;
			if (zendeskAuthType.Value == ZendeskAuthType.Jwt.Value || (isUserAuthenticatedBefore && !alwaysShowNameAndEmail))
			{
				createRequestPanelNameContainer.SetActive(value: false);
				createRequestPanelEmailContainer.SetActive(value: false);
			}
			else
			{
				createRequestPanelNameContainer.SetActive(value: true);
				createRequestPanelEmailContainer.SetActive(value: true);
				createRequestPanelInputEmail.GetComponentInParent<Image>().color = greyOutline;
				createRequestPanelInputEmail.text = (isUserAuthenticatedBefore ? zendeskMain.ZendeskAuthHandler.GetUserEmail() : "");
				createRequestPanelInputName.GetComponentInParent<Image>().color = greyOutline;
				createRequestPanelInputName.text = (isUserAuthenticatedBefore ? zendeskMain.ZendeskAuthHandler.GetUserName() : "");
				createRequestPanelEmailValidation.SetActive(value: false);
				createRequestPanelNameValidation.SetActive(value: false);
				num += createRequestPanelNameContainer.GetComponent<RectTransform>().rect.height + createRequestPanelEmailContainer.GetComponent<RectTransform>().rect.height + createRequestPanelFieldContainer.GetComponent<VerticalLayoutGroup>().spacing * 2f;
			}
			num += (float)(createRequestPanelFieldContainer.GetComponent<VerticalLayoutGroup>().padding.bottom + createRequestPanelFieldContainer.GetComponent<VerticalLayoutGroup>().padding.top);
			num = createRequestScrollView.GetComponent<RectTransform>().rect.height - num;
			if ((requestForm == null || requestForm.Fields == null || requestForm.Fields.Count == 0) && createRequestPanelMessageContainer.GetComponent<LayoutElement>().minHeight < num)
			{
				createRequestPanelMessageContainer.GetComponent<LayoutElement>().minHeight = num;
			}
			AddCustomFieldsUi();
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	private void InitLocalizationHandlerCustomFields()
	{
		try
		{
			if (customFieldTranslations != null && zendeskCustomFieldLocalizationHandler == null)
			{
				zendeskCustomFieldLocalizationHandler = base.gameObject.AddComponent<ZendeskLocalizationHandler>();
				zendeskCustomFieldLocalizationHandler.translationsFile = customFieldTranslations;
				zendeskCustomFieldLocalizationHandler.SetLocaleISO((int)zendeskMain.locale);
				zendeskCustomFieldLocalizationHandler.ReadData("", checkSuccess: false);
			}
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	private string GetTranslatedCustomField(string key)
	{
		string result = "";
		try
		{
			result = zendeskCustomFieldLocalizationHandler.translationGameObjects[key];
		}
		catch
		{
		}
		return result;
	}

	private void AddTags(string requestTags)
	{
		try
		{
			tags.AddRange(requestTags.Split(',').ToList());
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	public void SendRequest()
	{
		if (customFields == null)
		{
			customFields = new List<CustomField>();
		}
		customFields.AddRange(GetCustomFieldsValues());
		try
		{
			if (ValidateRequestForm())
			{
				zendeskUI.StartLoading(headerPanel);
				createRequestFooterPanelSendButton.interactable = false;
				createRequestFooterPanelAttachmentButton.interactable = false;
				createRequestPanelInputEmail.interactable = false;
				createRequestPanelInputName.interactable = false;
				createRequestPanelInputMessage.interactable = false;
				CreateRequestWrapper wrapper = CreateRequestWrapperObject(createRequestPanelInputName.text, createRequestPanelInputEmail.text, createRequestPanelInputMessage.text, customFields);
				zendeskMain.supportProvider.CreateRequest(CreateRequestCallback, wrapper, attachmentPaths);
			}
		}
		catch
		{
			zendeskErrorUI.NavigateError(null, true, true);
			zendeskUI.FinishLoading();
		}
	}

	public void SendRequest(Action OnRequestSentCallback = null, bool fakeSend = false, MinMax fakeSendWaitRange = null)
	{
		OnRequestSentSuccess = OnRequestSentCallback;
		if (customFields == null)
		{
			customFields = new List<CustomField>();
		}
		else
		{
			customFields.Clear();
		}
		customFields.AddRange(GetCustomFieldsValues());
		try
		{
			if (fakeSend)
			{
				createRequestFooterPanelSendButton.interactable = false;
				createRequestFooterPanelAttachmentButton.interactable = false;
				zendeskUI.StartLoading(headerPanel);
				StartCoroutine(PretendToSend(fakeSendWaitRange));
				Debug.Log(requestForm.messageId);
			}
			else if (ValidateRequestForm())
			{
				zendeskUI.StartLoading(headerPanel);
				createRequestFooterPanelSendButton.interactable = false;
				createRequestFooterPanelAttachmentButton.interactable = false;
				createRequestPanelInputEmail.interactable = false;
				createRequestPanelInputName.interactable = false;
				createRequestPanelInputMessage.interactable = false;
				CreateRequestWrapper wrapper = CreateRequestWrapperObject(createRequestPanelInputName.text, createRequestPanelInputEmail.text, createRequestPanelInputMessage.text, customFields);
				zendeskMain.supportProvider.CreateRequest(CreateRequestCallback, wrapper, attachmentPaths);
			}
		}
		catch
		{
			zendeskErrorUI.NavigateError(null, true, true);
			zendeskUI.FinishLoading();
		}
	}

	private IEnumerator PretendToSend(MinMax inWaitRange)
	{
		float seconds = UnityEngine.Random.Range(inWaitRange.Min, inWaitRange.Max);
		float cachedTimeScale = Time.timeScale;
		Time.timeScale = 1f;
		yield return new WaitForSeconds(seconds);
		Time.timeScale = cachedTimeScale;
		RemoveAllAttachments();
		ShowCreateTicketFormFeedback(success: true);
		OnRequestSentSuccess?.Invoke();
		zendeskUI.FinishLoading();
	}

	private CreateRequestWrapper CreateRequestWrapperObject(string userName, string email, string message, List<CustomField> customFields = null)
	{
		try
		{
			CreateRequest createRequest = new CreateRequest();
			createRequest.Requester = new Requester();
			if (isUserAuthenticatedBefore)
			{
				zendeskMain.ZendeskAuthHandler.DeleteUserInfo();
				zendeskMain.ZendeskAuthHandler.SetUserEmail(email);
				zendeskMain.ZendeskAuthHandler.SetUserName(userName);
			}
			createRequest.Requester.Name = userName;
			createRequest.Requester.Email = email;
			createRequest.TicketFormId = requestForm.ticketFormId;
			createRequest.Comment = new Comment();
			createRequest.CustomFields = customFields;
			if (requestForm.messageId > 0)
			{
				CustomField customField = customFields.Find((CustomField c) => c.Id == requestForm.messageId);
				if (customField != null)
				{
					message = customField.Value;
				}
			}
			createRequest.Comment.Body = message;
			createRequest.Tags = tags;
			if (requestForm.subjectId > 0)
			{
				CustomField customField2 = customFields.Find((CustomField c) => c.Id == requestForm.subjectId);
				if (customField2 != null)
				{
					message = customField2.Value;
				}
			}
			createRequest.Subject = message;
			return new CreateRequestWrapper
			{
				Request = createRequest
			};
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	private void CreateRequestCallback(ZendeskResponse<RequestResponse> requestResponse)
	{
		try
		{
			createRequestFooterPanelAttachmentButton.interactable = true;
			if (requestResponse.IsError)
			{
				if (requestResponse.ErrorResponse.IsAuthError && requestResponse.ErrorResponse.Status == 401)
				{
					if (zendeskAuthType.Value == ZendeskAuthType.Jwt.Value)
					{
						Debug.Log(jwtIntegratorError);
					}
					zendeskErrorUI.NavigateError(authScreenErrorBody, true, false, authScreenErrorTitle);
					ClearCreateRequestPage();
				}
				else
				{
					ShowCreateTicketFormFeedback(success: false);
				}
			}
			else
			{
				RemoveAllAttachments();
				if (!isConversationsEnabled)
				{
					ShowCreateTicketFormFeedback(success: true, requestResponse);
				}
			}
		}
		catch (Exception ex)
		{
			throw ex;
		}
		finally
		{
			zendeskUI.FinishLoading();
		}
	}

	private void CreateRequestConversationalCallback(ZendeskResponse<RequestResponse> requestResponse)
	{
		try
		{
			if (!(ticketResponseListView != null) || !ticketResponseListView.activeSelf || !ticketResponseListView.activeInHierarchy || !string.IsNullOrEmpty(ticketId))
			{
				return;
			}
			attachmentButton.GetComponent<Button>().interactable = true;
			newCommentEndUser.GetComponent<InputField>().interactable = true;
			sendCommentButton.GetComponent<Button>().interactable = true;
			if (requestResponse.IsError)
			{
				if (!zendeskErrorUI.IfAuthError(requestResponse.ErrorResponse, zendeskAuthType))
				{
					zendeskErrorUI.NavigateError(zendeskLocalizationHandler.translationGameObjects["usdk_label_failed"], false, true);
				}
				CheckAvailabilityOfSendButton(true);
			}
			else
			{
				ClearConversationalFooter();
				ticketId = requestResponse.Result.Request.Id;
				CheckAvailabilityOfSendButton(null);
				zendeskUI.backStateGO[zendeskUI.backStateGO.Count - 1].Parameter = requestResponse.Result.Request;
				prevDate = DateTime.Today;
				zendeskMain.supportProvider.GetComments(GetCommentsCreateRequestConversationalCallback, ticketId);
			}
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	private void GetCommentsCreateRequestConversationalCallback(ZendeskResponse<CommentsResponse> commentsResponse)
	{
		try
		{
			if (string.IsNullOrEmpty(ticketId))
			{
				return;
			}
			prevDate = DateTime.MinValue;
			if (commentsResponse.IsError)
			{
				if (!zendeskErrorUI.IfAuthError(commentsResponse.ErrorResponse, zendeskAuthType))
				{
					zendeskErrorUI.NavigateError(commentsResponse.ErrorResponse.Reason, false, true);
				}
				return;
			}
			CommentsResponse result = commentsResponse.Result;
			long num = -1L;
			foreach (CommentResponse comment in result.Comments)
			{
				GameObject gameObject = null;
				if (comment.CreatedAt.HasValue && !comment.CreatedAt.Value.Date.Equals(prevDate.Date))
				{
					prevDate = comment.CreatedAt.Value;
					num = -1L;
				}
				if (comment.Agent)
				{
					gameObject = UnityEngine.Object.Instantiate(agentResponsePrefab, ticketResponseListView.transform);
				}
				else
				{
					gameObject = UnityEngine.Object.Instantiate(endUserResponsePrefab, ticketResponseListView.transform);
					num = -1L;
				}
				bool flag = num == comment.AuthorId;
				User user = result.Users.FirstOrDefault((User u) => u.Id == comment.AuthorId);
				if (user != null)
				{
					if (flag)
					{
						DeleteAgentImageFromPreviousResponse(ticketResponseContainerList[ticketResponseContainerList.Count - 1]);
					}
					gameObject.GetComponent<ZendeskTicketResponse>().Init(comment, zendeskMain.supportProvider, this, zendeskErrorUI, user, flag, (from u in result.Users
						where u.Id == comment.AuthorId
						select u.Photo).First(), null, zendeskMain.zendeskHtmlHandler, zendeskLocalizationHandler);
				}
				else
				{
					gameObject.GetComponent<ZendeskTicketResponse>().Init(comment, zendeskMain.supportProvider, this, zendeskErrorUI, null, flag, null, null, zendeskMain.zendeskHtmlHandler, zendeskLocalizationHandler);
				}
				if (!comment.Agent)
				{
					gameObject.GetComponent<ZendeskTicketResponse>().MessageSentStatus(sent: true);
				}
				num = comment.AuthorId;
				ClearConversationalFooter();
				ticketResponseContainerList.Add(gameObject);
			}
			createRequestFooterPanelSendButton.interactable = false;
			if (ticketResponseListView != null && ticketResponseListView.activeSelf && ticketResponseListView.activeInHierarchy)
			{
				ScrollDownTicketResponse(forceScroll: true);
			}
			if (!string.IsNullOrEmpty(ticketConfirmationMessageString))
			{
				StartCoroutine(AddConfirmationMessageConversational(ticketConfirmationMessageString, 2f));
			}
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	private void ClearCreateRequestPage()
	{
		createRequestFooterPanelSendButton.interactable = true;
		createRequestPanelInputEmail.interactable = true;
		createRequestPanelInputName.interactable = true;
		createRequestPanelInputMessage.interactable = true;
	}

	private void ShowCreateTicketFormFeedback(bool success, ZendeskResponse<RequestResponse> requestResponse = null)
	{
		try
		{
			if (success)
			{
				createRequestPanelFeedbackPanel.SetActive(value: true);
				OnRequestSentSuccess?.Invoke();
			}
			else
			{
				zendeskErrorUI.NavigateError(zendeskLocalizationHandler.translationGameObjects["usdk_label_failed"], false, true);
				ClearCreateRequestPage();
			}
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	private void LoadTickets()
	{
		try
		{
			zendeskUI.StartLoading(headerPanel);
			ClearExistingTickets();
			zendeskMain.supportProvider.GetAllRequests(AllRequestsCallback, "");
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	private void AllRequestsCallback(ZendeskResponse<RequestsResponse> requestsResponse)
	{
		try
		{
			if (requestsResponse.IsError)
			{
				_ = requestsResponse.ErrorResponse.IsNetworkError;
				if (!zendeskErrorUI.IfAuthError(requestsResponse.ErrorResponse, zendeskAuthType))
				{
					zendeskErrorUI.NavigateError(null, true, true);
				}
				return;
			}
			RequestsResponse result = requestsResponse.Result;
			if (result != null && result.Requests != null && result.Requests.Count > 0)
			{
				myTicketsFooter.SetActive(value: true);
				noTicketsPanel.SetActive(value: false);
				noTicketsFooter.SetActive(value: false);
				ClearExistingTickets();
				{
					foreach (Request request in result.Requests)
					{
						GameObject gameObject = UnityEngine.Object.Instantiate(ticketContainerPrefab, ticketListView.transform);
						gameObject.GetComponent<ZendeskTicketContentOverview>().Init(request, zendeskMain.supportProvider, this, zendeskErrorUI, zendeskMain.zendeskHtmlHandler, zendeskLocalizationHandler);
						ticketContainersList.Add(gameObject);
					}
					return;
				}
			}
			myTicketsFooter.SetActive(value: false);
			noTicketsPanel.SetActive(value: true);
			noTicketsFooter.SetActive(value: true);
		}
		catch (Exception ex)
		{
			throw ex;
		}
		finally
		{
			zendeskUI.FinishLoading();
		}
	}

	private void ClearExistingTickets()
	{
		try
		{
			foreach (GameObject ticketContainers in ticketContainersList)
			{
				Text[] componentsInChildren = ticketContainers.GetComponentsInChildren<Text>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].text = "";
				}
				UnityEngine.Object.Destroy(ticketContainers);
			}
			ticketContainersList.Clear();
			ticketContainersList = new List<GameObject>();
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	private void HighlightErrorField(RectTransform rectTransform)
	{
		if (!errorField)
		{
			errorField = true;
			CenterToItem(rectTransform);
		}
	}

	public void CenterToItem(RectTransform obj)
	{
		ScrollRect component = createRequestScrollView.GetComponent<ScrollRect>();
		float num = createRequestScrollView.GetComponent<RectTransform>().anchorMin.y - obj.anchoredPosition.y;
		num += (float)obj.transform.GetSiblingIndex() / (float)component.content.transform.childCount;
		num /= 1000f;
		num -= 0.45f;
		num = Mathf.Clamp01(1f - num);
		component.verticalNormalizedPosition = num;
	}

	public bool ValidateRequestForm()
	{
		try
		{
			errorField = false;
			if (zendeskAuthType.Value == ZendeskAuthType.Anonymous.Value && !isUserAuthenticatedBefore)
			{
				ValidateEmail();
				ValidateName();
			}
			else
			{
				emailValid = true;
				nameValid = true;
			}
			ValidateMessage();
			ValidateCustomFields();
			return (emailValid & nameValid & messageValid) && customFieldValid;
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	private void ValidateCustomFields()
	{
		try
		{
			customFieldValid = true;
			foreach (KeyValuePair<GameObject, FieldType> customFieldsPrefab in customFieldsPrefabs)
			{
				if (customFieldsPrefab.Value == FieldType.Text || customFieldsPrefab.Value == FieldType.Multiline || customFieldsPrefab.Value == FieldType.Numeric || customFieldsPrefab.Value == FieldType.Decimal)
				{
					ZendeskCustomTextFieldScript component = customFieldsPrefab.Key.GetComponent<ZendeskCustomTextFieldScript>();
					if (component.required)
					{
						string text = component.inputField.text.Trim();
						if (string.IsNullOrEmpty(text) && text.Length == 0)
						{
							component.inputField.GetComponentInParent<Image>().color = redOutline;
							component.validationPanel.SetActive(value: true);
							customFieldValid = false;
							HighlightErrorField(customFieldsPrefab.Key.GetComponent<RectTransform>());
						}
						else
						{
							component.inputField.GetComponentInParent<Image>().color = greyOutline;
							component.validationPanel.SetActive(value: false);
						}
					}
				}
				else
				{
					if (customFieldsPrefab.Value != 0)
					{
						continue;
					}
					ZendeskCustomDropdownFieldScript component2 = customFieldsPrefab.Key.GetComponent<ZendeskCustomDropdownFieldScript>();
					if ((bool)component2 && component2.required)
					{
						if (component2.dropdown.captionText.text == "-")
						{
							component2.dropdown.GetComponentInChildren<Image>().color = redOutline;
							component2.validationPanel.SetActive(value: true);
							customFieldValid = false;
							HighlightErrorField(customFieldsPrefab.Key.GetComponent<RectTransform>());
						}
						else
						{
							component2.dropdown.GetComponentInChildren<Image>().color = greyOutline;
							component2.validationPanel.SetActive(value: false);
						}
					}
				}
			}
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	public void ValidateEmail()
	{
		try
		{
			string text = createRequestPanelInputEmail.text.Trim();
			emailValid = true;
			if (!text.Contains("@"))
			{
				createRequestPanelInputEmail.GetComponentInParent<Image>().color = redOutline;
				if ((requestForm == null || requestForm.Fields == null || requestForm.Fields.Count == 0) && !createRequestPanelEmailValidation.activeSelf)
				{
					createRequestPanelMessageContainer.GetComponent<LayoutElement>().minHeight -= createRequestPanelEmailValidation.GetComponent<RectTransform>().rect.height + createRequestPanelEmailContainer.GetComponent<VerticalLayoutGroup>().spacing;
				}
				createRequestPanelEmailValidation.SetActive(value: true);
				emailValid = false;
				HighlightErrorField(createRequestPanelMessageContainer.GetComponent<RectTransform>());
			}
			else if (text.Contains("@"))
			{
				createRequestPanelInputEmail.GetComponentInParent<Image>().color = greyOutline;
				if ((requestForm == null || requestForm.Fields == null || requestForm.Fields.Count == 0) && createRequestPanelEmailValidation.activeSelf)
				{
					createRequestPanelMessageContainer.GetComponent<LayoutElement>().minHeight += createRequestPanelEmailValidation.GetComponent<RectTransform>().rect.height + createRequestPanelEmailContainer.GetComponent<VerticalLayoutGroup>().spacing;
				}
				createRequestPanelEmailValidation.SetActive(value: false);
			}
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	public void ValidateName()
	{
		try
		{
			string text = createRequestPanelInputName.text.Trim();
			nameValid = true;
			if (string.IsNullOrEmpty(text) && text.Length == 0)
			{
				createRequestPanelInputName.GetComponentInParent<Image>().color = redOutline;
				if ((requestForm == null || requestForm.Fields == null || requestForm.Fields.Count == 0) && !createRequestPanelNameValidation.activeSelf)
				{
					createRequestPanelMessageContainer.GetComponent<LayoutElement>().minHeight -= createRequestPanelNameValidation.GetComponent<RectTransform>().rect.height + createRequestPanelNameContainer.GetComponent<VerticalLayoutGroup>().spacing;
				}
				createRequestPanelNameValidation.SetActive(value: true);
				nameValid = false;
				HighlightErrorField(createRequestPanelNameValidation.GetComponent<RectTransform>());
			}
			else if (!string.IsNullOrEmpty(text) && text.Length != 0)
			{
				createRequestPanelInputName.GetComponentInParent<Image>().color = greyOutline;
				if ((requestForm == null || requestForm.Fields == null || requestForm.Fields.Count == 0) && createRequestPanelNameValidation.activeSelf)
				{
					createRequestPanelMessageContainer.GetComponent<LayoutElement>().minHeight += createRequestPanelNameValidation.GetComponent<RectTransform>().rect.height + createRequestPanelNameContainer.GetComponent<VerticalLayoutGroup>().spacing;
				}
				createRequestPanelNameValidation.SetActive(value: false);
			}
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	public void ValidateMessage()
	{
		try
		{
			string text = createRequestPanelInputMessage.text.Trim();
			messageValid = true;
			if (createRequestPanelInputMessage.gameObject.activeInHierarchy)
			{
				if (string.IsNullOrEmpty(text) && text.Length == 0)
				{
					createRequestPanelInputMessage.GetComponentInParent<Image>().color = redOutline;
					createRequestPanelMessageValidation.SetActive(value: true);
					messageValid = false;
					HighlightErrorField(createRequestPanelInputMessage.GetComponent<RectTransform>());
				}
				else
				{
					createRequestPanelInputMessage.GetComponentInParent<Image>().color = greyOutline;
					createRequestPanelMessageValidation.SetActive(value: false);
				}
			}
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	private void GetRequestComments(string id, bool refreshFooter)
	{
		try
		{
			ticketId = id;
			zendeskMain.supportProvider.GetComments(delegate(ZendeskResponse<CommentsResponse> commentsResponse)
			{
				GetRequestCommentsCallback(commentsResponse, refreshFooter);
			}, id);
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	private void RefreshComments(string id)
	{
		try
		{
			ticketId = id;
			zendeskMain.supportProvider.GetComments(RefreshCommentsCallback, id, lastCommentDatetime);
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	private IEnumerator AddConfirmationMessageConversational(string message, float delay = 0f)
	{
		Coroutine ticketConfirmationMessageCoroutine2 = null;
		if (delay > 0f)
		{
			ticketConfirmationMessageCoroutine2 = StartCoroutine(WaitForRealSeconds(delay));
			ticketConfirmationMessageCoroutines.Add(ticketConfirmationMessageCoroutine2);
			yield return ticketConfirmationMessageCoroutine2;
			GameObject gameObject = UnityEngine.Object.Instantiate(ticketConfirmationMessagePrefab, ticketResponseListView.transform);
			gameObject.GetComponent<ZendeskTicketConfirmationMessage>().Init(message);
			ticketResponseContainerList.Add(gameObject);
			ScrollDownTicketResponse();
			ticketConfirmationMessageCoroutines.Remove(ticketConfirmationMessageCoroutine2);
			yield break;
		}
		try
		{
			if (delay == 0f || (ticketConfirmationMessageCoroutines.Count > 0 && ticketConfirmationMessageCoroutines.Contains(ticketConfirmationMessageCoroutine2)))
			{
				GameObject gameObject2 = UnityEngine.Object.Instantiate(ticketConfirmationMessagePrefab, ticketResponseListView.transform);
				gameObject2.GetComponent<ZendeskTicketConfirmationMessage>().Init(message);
				ticketResponseContainerList.Add(gameObject2);
				ScrollDownTicketResponse();
				ticketConfirmationMessageCoroutines.Remove(ticketConfirmationMessageCoroutine2);
			}
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	private void AddConfirmationMessageConversationalNoFade(string message)
	{
		try
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(ticketConfirmationMessagePrefab, ticketResponseListView.transform);
			gameObject.GetComponent<ZendeskTicketConfirmationMessage>().Init(message);
			ticketResponseContainerList.Add(gameObject);
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	public GameObject AddAnswerBotCSAT(string message)
	{
		try
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(answerBotResponsePrefab, ticketResponseListView.transform);
			gameObject.GetComponent<ZendeskAnswerBotTicketResponse>().Init(message, zendeskErrorUI, zendeskLocalizationHandler);
			ticketResponseContainerList.Add(gameObject);
			return gameObject;
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	private IEnumerator WaitForRealSeconds(float time)
	{
		float start = Time.realtimeSinceStartup;
		while (Time.realtimeSinceStartup < start + time)
		{
			yield return null;
		}
	}

	private void GetRequestCommentsCallback(ZendeskResponse<CommentsResponse> commentsResponse, bool refreshFooter)
	{
		try
		{
			bool flag = false;
			prevDate = DateTime.MinValue;
			if (commentsResponse.IsError)
			{
				if (!zendeskErrorUI.IfAuthError(commentsResponse.ErrorResponse, zendeskAuthType))
				{
					zendeskErrorUI.NavigateError(null, true, true);
				}
				return;
			}
			CommentsResponse result = commentsResponse.Result;
			ClearTicketResponse();
			RetrieveTicketResponseData();
			if (refreshFooter)
			{
				ClearConversationalFooter();
			}
			long num = -1L;
			foreach (CommentResponse comment in result.Comments)
			{
				if (comment.RequestId != ticketId)
				{
					return;
				}
				GameObject gameObject = null;
				if (comment.CreatedAt.HasValue && !comment.CreatedAt.Value.Date.Equals(prevDate.Date))
				{
					prevDate = comment.CreatedAt.Value;
					num = -1L;
					AddMessageDatePrefab();
				}
				gameObject = ((!comment.Agent) ? UnityEngine.Object.Instantiate(endUserResponsePrefab, ticketResponseListView.transform) : UnityEngine.Object.Instantiate(agentResponsePrefab, ticketResponseListView.transform));
				if (!string.IsNullOrEmpty(ticketConfirmationMessageString) && !flag)
				{
					AddConfirmationMessageConversationalNoFade(ticketConfirmationMessageString);
					flag = true;
				}
				bool flag2 = num == comment.AuthorId;
				if (result.Users.FirstOrDefault((User u) => u.Id == comment.AuthorId) != null)
				{
					if (flag2)
					{
						if (comment.Agent)
						{
							DeleteAgentImageFromPreviousResponse(ticketResponseContainerList[ticketResponseContainerList.Count - 1]);
						}
					}
					else
					{
						gameObject.GetComponent<HorizontalLayoutGroup>().padding.top = 24;
					}
					gameObject.GetComponent<ZendeskTicketResponse>().Init(comment, zendeskMain.supportProvider, this, zendeskErrorUI, null, flag2, (from u in result.Users
						where u.Id == comment.AuthorId
						select u.Photo).First(), null, zendeskMain.zendeskHtmlHandler, zendeskLocalizationHandler);
				}
				else
				{
					gameObject.GetComponent<ZendeskTicketResponse>().Init(comment, zendeskMain.supportProvider, this, zendeskErrorUI, null, flag2, null, null, zendeskMain.zendeskHtmlHandler, zendeskLocalizationHandler);
				}
				if (!comment.Agent)
				{
					gameObject.GetComponent<ZendeskTicketResponse>().MessageSentStatus(sent: true);
				}
				num = comment.AuthorId;
				ticketResponseContainerList.Add(gameObject);
			}
			if (request.StatusEnum == RequestStatus.Closed)
			{
				GameObject gameObject2 = UnityEngine.Object.Instantiate(endOfConversationPrefab, ticketResponseListView.transform);
				gameObject2.GetComponent<ZendeskTextContentScript>().Init(new string[1] { zendeskLocalizationHandler.translationGameObjects["usdk_conversation_ended"] });
				ticketResponseContainerList.Add(gameObject2);
				createRequestAttachmentLine.SetActive(value: false);
				footerLineMain.SetActive(value: false);
				footerClosedStatus.SetActive(value: true);
			}
			else if (request.StatusEnum == RequestStatus.Solved && isCsatEnabled)
			{
				satisfactionRatingPrefabInstatiated = UnityEngine.Object.Instantiate(satisfactionRatingPrefab, ticketResponseListView.transform);
				satisfactionRatingPrefabInstatiated.SetActive(value: true);
				ticketResponseContainerList.Add(satisfactionRatingPrefabInstatiated);
				satisfactionRatingPrefabInstatiated.GetComponent<ZendeskCustomerSatisfactionRating>().Init(request, zendeskMain.supportProvider, zendeskMain, ticketResponseListView, zendeskErrorUI, zendeskLocalizationHandler);
			}
			createRequestFooterPanelSendButton.interactable = false;
			ScrollDownTicketResponse(forceScroll: true);
			SupportRequestStorageItem supportRequest = ZendeskLocalStorage.GetSupportRequest(request.Id);
			ZendeskLocalStorage.SaveSupportRequest(request.Id, result.Comments.Count(), isRead: true, supportRequest.score, supportRequest.ratingComment);
			lastCommentDatetime = result.Comments.LastOrDefault().CreatedAt.Value.AddSeconds(1.0);
		}
		catch
		{
			zendeskErrorUI.NavigateError(null, true, true);
		}
	}

	private void Update()
	{
	}

	private void OnDisable()
	{
		zendeskMain.zendeskPauseHandler.ResumeApplicationInternal();
	}

	public void ScrollDownTicketResponse(bool forceScroll = false)
	{
		try
		{
			if (!(ticketResponseListView != null) || !ticketResponseListView.activeSelf || ticketResponseListView.transform.childCount <= 0)
			{
				return;
			}
			if (forceScroll)
			{
				ticketResponseListView.GetComponentInParent<RectTransform>().anchoredPosition = new Vector2(0f, 10000000f);
				return;
			}
			float componentsHeighSum = 0f;
			ticketResponseContainerList.ForEach(delegate(GameObject a)
			{
				componentsHeighSum += a.GetComponent<RectTransform>().rect.height;
			});
			if (componentsHeighSum >= ticketResponseScrollView.GetComponent<RectTransform>().rect.height - 100f)
			{
				if (ticketResponseListView != null && ticketResponseListView.activeSelf)
				{
					ticketResponseListView.GetComponentInParent<RectTransform>().anchoredPosition = new Vector2(0f, 10000000f);
				}
			}
			else if (ticketResponseListView != null && ticketResponseListView.activeSelf)
			{
				ticketResponseListView.GetComponentInParent<ScrollRect>().verticalNormalizedPosition = -1f;
			}
		}
		catch (Exception)
		{
		}
	}

	private void DeleteAgentImageFromPreviousResponse(GameObject previousAgentResponse)
	{
		try
		{
			previousAgentResponse.GetComponent<ZendeskTicketResponse>().agentPhotoContainerGO.gameObject.SetActive(value: false);
		}
		catch
		{
		}
	}

	private void RefreshCommentsCallback(ZendeskResponse<CommentsResponse> commentsResponse)
	{
		try
		{
			SupportRequestStorageItem supportRequest = ZendeskLocalStorage.GetSupportRequest(request.Id);
			if (commentsResponse.IsError)
			{
				if (!zendeskErrorUI.IfAuthError(commentsResponse.ErrorResponse, zendeskAuthType))
				{
					zendeskErrorUI.NavigateError(commentsResponse.ErrorResponse.Reason, false, true);
				}
				return;
			}
			CommentsResponse result = commentsResponse.Result;
			long num = -1L;
			foreach (CommentResponse comment in result.Comments)
			{
				GameObject gameObject = null;
				if (comment.CreatedAt.HasValue && !comment.CreatedAt.Value.Date.Equals(prevDate.Date))
				{
					prevDate = comment.CreatedAt.Value;
					AddMessageDatePrefab();
				}
				if (comment.Agent)
				{
					gameObject = UnityEngine.Object.Instantiate(agentResponsePrefab, ticketResponseListView.transform);
				}
				else
				{
					gameObject = UnityEngine.Object.Instantiate(endUserResponsePrefab, ticketResponseListView.transform);
					num = -1L;
				}
				bool flag = num == comment.AuthorId;
				User user = result.Users.FirstOrDefault((User u) => u.Id == comment.AuthorId);
				if (user != null)
				{
					if (flag)
					{
						DeleteAgentImageFromPreviousResponse(ticketResponseContainerList[ticketResponseContainerList.Count - 1]);
					}
					gameObject.GetComponent<ZendeskTicketResponse>().Init(comment, zendeskMain.supportProvider, this, zendeskErrorUI, user, flag, (from u in result.Users
						where u.Id == comment.AuthorId
						select u.Photo).First(), null, zendeskMain.zendeskHtmlHandler, zendeskLocalizationHandler);
				}
				else
				{
					gameObject.GetComponent<ZendeskTicketResponse>().Init(comment, zendeskMain.supportProvider, this, zendeskErrorUI, null, flag, null, null, zendeskMain.zendeskHtmlHandler, zendeskLocalizationHandler);
				}
				num = comment.AuthorId;
				ticketResponseContainerList.Add(gameObject);
				if (!comment.Agent)
				{
					gameObject.GetComponent<ZendeskTicketResponse>().MessageSentStatus(sent: true);
				}
			}
			ScrollDownTicketResponse(forceScroll: true);
			ZendeskLocalStorage.SaveSupportRequest(request.Id, supportRequest.commentCount + result.Comments.Count(), isRead: true, supportRequest.score, supportRequest.ratingComment);
			lastCommentDatetime = DateTime.Now;
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	private void ClearTicketResponse()
	{
		try
		{
			foreach (GameObject ticketResponseContainer in ticketResponseContainerList)
			{
				if (ticketResponseContainer != null)
				{
					Text[] componentsInChildren = ticketResponseContainer.GetComponentsInChildren<Text>();
					for (int i = 0; i < componentsInChildren.Length; i++)
					{
						componentsInChildren[i].text = "";
					}
					UnityEngine.Object.Destroy(ticketResponseContainer);
				}
			}
			ticketResponseContainerList.Clear();
			ticketResponseContainerList = new List<GameObject>();
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	public void AddComment()
	{
		try
		{
			if (satisfactionRatingPrefabInstatiated != null)
			{
				satisfactionRatingPrefabInstatiated.GetComponent<ZendeskCustomerSatisfactionRating>().ClearCSAT();
				UnityEngine.Object.Destroy(satisfactionRatingPrefabInstatiated);
			}
			CheckAvailabilityOfSendButton(false);
			attachmentButton.GetComponent<Button>().interactable = false;
			newCommentEndUser.GetComponent<InputField>().interactable = false;
			sendCommentButton.GetComponent<Button>().interactable = false;
			InputField component = newCommentEndUser.GetComponent<InputField>();
			string text = "";
			if (attachmentPaths != null && attachmentPaths.Count > 0 && string.IsNullOrEmpty(component.text))
			{
				text = "[" + string.Join(",", attachmentPaths.ToArray()) + "]";
			}
			else if (!string.IsNullOrEmpty(component.text))
			{
				text = component.text;
			}
			if ((string.IsNullOrEmpty(ticketId) && string.IsNullOrEmpty(text)) || (!string.IsNullOrEmpty(ticketId) && string.IsNullOrEmpty(text) && (attachmentPaths == null || attachmentPaths.Count == 0)))
			{
				ZendeskResponse<object> zendeskResponse = new ZendeskResponse<object>();
				ZendeskUtils.CheckErrorAndAssign(null, zendeskMain.zendeskLocalizationHandler.translationGameObjects["usdk_required_field_label"], zendeskResponse);
				zendeskErrorUI.NavigateError(zendeskResponse.ErrorResponse.Reason, false, true);
				return;
			}
			if (string.IsNullOrEmpty(ticketId))
			{
				CreateRequestWrapper wrapper = CreateRequestWrapperObject(zendeskMain.GetUserName(), zendeskMain.GetUserEmail(), text, customFields);
				zendeskMain.supportProvider.CreateRequest(CreateRequestConversationalCallback, wrapper, attachmentPaths);
			}
			else
			{
				zendeskMain.supportProvider.AddComment(AddCommentCallback, ticketId, text, attachmentPaths);
			}
			if (attachmentPaths.Count > 0)
			{
				zendeskErrorUI.NavigateError(zendeskLocalizationHandler.translationGameObjects["usdk_sending_label"], false, true);
			}
		}
		catch
		{
			zendeskErrorUI.NavigateError(null, true, true);
		}
	}

	private void CheckAvailabilityOfSendButton(bool? forceButton)
	{
		try
		{
			InputField component = newCommentEndUser.GetComponent<InputField>();
			if (!string.IsNullOrEmpty(ticketId))
			{
				attachmentButton.GetComponent<Button>().interactable = true;
				attachmentButton.GetComponentInChildren<RawImage>().color = new Color32(104, 115, 125, byte.MaxValue);
			}
			bool flag = !string.IsNullOrEmpty(component.text) || (attachmentPaths.Count != 0 && !string.IsNullOrEmpty(ticketId));
			if (forceButton.HasValue)
			{
				flag = forceButton.Value;
			}
			if (!(addCommentButtonImage != null) || !(addCommentButtonImage.gameObject != null) || !(addCommentButtonImage.gameObject.GetComponentInParent<Button>() != null))
			{
				return;
			}
			addCommentButtonImage.gameObject.GetComponentInParent<Button>().interactable = flag;
			if (flag)
			{
				addCommentButtonImage.gameObject.SetActive(value: true);
				addCommentButtonImage.GetComponent<RawImage>().color = new Color32(31, 115, 183, byte.MaxValue);
				attachmentButton.GetComponent<Button>().interactable = true;
				attachmentButton.GetComponentInChildren<RawImage>().color = new Color32(104, 115, 125, byte.MaxValue);
				return;
			}
			addCommentButtonImage.gameObject.SetActive(value: false);
			addCommentButtonImage.GetComponent<RawImage>().color = new Color32(194, 194, 194, byte.MaxValue);
			if (string.IsNullOrEmpty(ticketId))
			{
				attachmentButton.GetComponent<Button>().interactable = false;
				attachmentButton.GetComponentInChildren<RawImage>().color = new Color32(104, 115, 125, 69);
			}
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	private void ClearConversationalFooter()
	{
		try
		{
			typeMessageText.text = zendeskMain.zendeskLocalizationHandler.translationGameObjects["usdk_type_a_message_label_without_dots"];
			newCommentEndUser.GetComponent<InputField>().text = "";
			createRequestAttachmentLine.SetActive(value: false);
			footerLineMain.SetActive(value: true);
			footerClosedStatus.SetActive(value: false);
			RemoveAllAttachments();
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	private void RemoveAllAttachments()
	{
		try
		{
			attachmentPaths.Clear();
			Transform transform;
			if (isConversationsEnabled)
			{
				createRequestAttachmentLine.SetActive(value: false);
				ticketResponseAttachmentCountContainer.SetActive(value: false);
				ticketResponseAttachmentCountText.text = string.Empty;
				transform = createRequestAttachmentLine.transform.GetChild(0).GetChild(0).transform;
			}
			else
			{
				createRequestPanelAttachmentLine.SetActive(value: false);
				createRequestAttachmentCountContainer.SetActive(value: false);
				createRequestAttachmentCountText.text = string.Empty;
				transform = createRequestPanelAttachmentLine.transform.GetChild(0).GetChild(0).transform;
			}
			foreach (Transform item in transform)
			{
				UnityEngine.Object.Destroy(item.gameObject);
			}
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	private void AddCommentCallback(ZendeskResponse<Comment> commentResponse)
	{
		try
		{
			attachmentButton.GetComponent<Button>().interactable = true;
			newCommentEndUser.GetComponent<InputField>().interactable = true;
			sendCommentButton.GetComponent<Button>().interactable = true;
			if (commentResponse.IsError)
			{
				if (!zendeskErrorUI.IfAuthError(commentResponse.ErrorResponse, zendeskAuthType))
				{
					zendeskErrorUI.NavigateError(zendeskLocalizationHandler.translationGameObjects["usdk_label_failed"], false, true);
				}
				CheckAvailabilityOfSendButton(true);
				return;
			}
			if (!prevDate.Date.Equals(DateTime.Today.Date))
			{
				prevDate = DateTime.Today;
				AddMessageDatePrefab();
			}
			ClearConversationalFooter();
			newCommentEndUser.GetComponent<InputField>().text = "";
			CheckAvailabilityOfSendButton(false);
			lastCommentDatetime = commentResponse.Result.CreatedAt.Value.AddSeconds(1.0);
			GameObject gameObject = UnityEngine.Object.Instantiate(endUserResponsePrefab, ticketResponseListView.transform);
			gameObject.GetComponent<ZendeskTicketResponse>().Init(commentResponse.Result, this, zendeskMain.supportProvider, zendeskErrorUI, zendeskLocalizationHandler);
			gameObject.GetComponent<ZendeskTicketResponse>().MessageSentStatus(sent: true);
			ticketResponseContainerList.Add(gameObject);
			ScrollDownTicketResponse(forceScroll: true);
		}
		catch
		{
			zendeskErrorUI.NavigateError(null, true, true);
		}
	}

	public void AddAttachment()
	{
		try
		{
			ExtensionFilter[] extensions = new ExtensionFilter[3]
			{
				new ExtensionFilter("Image Files", "png", "jpg", "jpeg"),
				new ExtensionFilter("Video Files", "mp4", "avi", "wmv"),
				new ExtensionFilter("Text Files", "txt")
			};
			Cursor.visible = true;
			string[] array = StandaloneFileBrowser.OpenFilePanel("Open File", "", extensions, multiselect: true);
			Cursor.visible = false;
			string[] array2 = array;
			foreach (string text in array2)
			{
				if (text.Length > 0)
				{
					ImageSavedToTemporaryPath(text);
				}
			}
		}
		catch (Exception)
		{
			throw;
		}
	}

	private void OnWSAFilePicked(List<string> filePaths)
	{
		foreach (string filePath in filePaths)
		{
			ImageSavedToTemporaryPath(filePath);
		}
	}

	private void OnOpenFilesStart()
	{
		Cursor.visible = true;
	}

	private void OnOpenFilesComplete(bool selected, string singlefile, string[] files)
	{
		if (files.Length == 1 && files[0] == string.Empty)
		{
			Cursor.visible = false;
			return;
		}
		foreach (string text in files)
		{
			if (text.Length > 0)
			{
				ImageSavedToTemporaryPath(text);
			}
		}
		Cursor.visible = false;
	}

	private void RetrieveTicketResponseData()
	{
		try
		{
			zendeskMain.zendeskPauseHandler.PauseApplicationInternal();
			myTicketsPanel.SetActive(value: false);
			createRequestPanel.SetActive(value: false);
			ticketResponsePanel.SetActive(value: true);
			zendeskUI.ShowBackButton(backButtonContainer);
			headerTitle.text = zendeskMain.zendeskLocalizationHandler.translationGameObjects["usdk_get_in_touch_label_conversation_header"];
			CheckAvailabilityOfSendButton(null);
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	private void ShowMyTicketsWindow()
	{
		try
		{
			zendeskUI.AddScreenBackState(myTicketsPanel, ZendeskScreen.MyTickets);
			zendeskMain.zendeskPauseHandler.PauseApplicationInternal();
			myTicketsPanel.SetActive(value: true);
			createRequestPanel.SetActive(value: false);
			ticketResponsePanel.SetActive(value: false);
			zendeskUI.ShowBackButton(backButtonContainer);
			headerTitle.text = zendeskLocalizationHandler.translationGameObjects["usdk_conversations_title"];
			if (isConversationsEnabled)
			{
				LoadTickets();
			}
			else
			{
				zendeskErrorUI.NavigateError(zendeskLocalizationHandler.translationGameObjects["usdk_conversations_are_not_enabled"], true, false, "hide");
			}
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	public void SubmitNewRequest(string requestTags = "")
	{
		zendeskUI.FinishLoading();
		try
		{
			if (!zendeskCanvas.activeSelf)
			{
				zendeskCanvas.SetActive(value: true);
			}
			if (zendeskMain.supportProvider != null)
			{
				lastCommentDatetime = DateTime.MinValue;
				prevDate = DateTime.Now;
				zendeskMain.zendeskPauseHandler.PauseApplicationInternal();
				base.gameObject.SetActive(value: true);
				zendeskErrorUI.DestroyZendeskErrorToast();
				myTicketsPanel.SetActive(value: false);
				AddTags(requestTags);
				if (isConversationsEnabled)
				{
					ticketId = null;
					newCommentEndUser.GetComponent<InputField>().interactable = true;
					ClearTicketResponse();
					zendeskUI.AddScreenBackState(ticketResponsePanel, ZendeskScreen.TicketResponse);
					ticketResponsePanel.SetActive(value: true);
					createRequestPanel.SetActive(value: false);
					ClearConversationalFooter();
					CheckAvailabilityOfSendButton(null);
					AddMessageDatePrefab();
				}
				else
				{
					RemoveAllAttachments();
					createRequestScrollView.GetComponent<ScrollRect>().verticalNormalizedPosition = 1f;
					zendeskUI.AddScreenBackState(createRequestPanel, ZendeskScreen.CreateTicket, requestTags);
					ticketResponsePanel.SetActive(value: false);
					createRequestPanel.SetActive(value: true);
					InitCreateRequestPanel();
				}
				headerTitle.text = zendeskMain.zendeskLocalizationHandler.translationGameObjects["usdk_get_in_touch_label_conversation_header"];
				zendeskUI.ShowBackButton(backButtonContainer);
				return;
			}
			zendeskUI.AddScreenBackState(createRequestPanel, ZendeskScreen.CreateTicket);
			throw new Exception();
		}
		catch
		{
			zendeskErrorUI.NavigateError(null, true, true);
		}
	}

	public void RetrieveTicketResponseData(Request requestParam, bool clearLastCommentDatetime = false, bool refreshFooter = true)
	{
		try
		{
			zendeskMain.zendeskPauseHandler.PauseApplicationInternal();
			if (clearLastCommentDatetime)
			{
				lastCommentDatetime = DateTime.MinValue;
			}
			if (requestParam != null)
			{
				zendeskUI.StartLoading(headerPanel);
				zendeskMain.supportProvider.GetRequest(delegate(ZendeskResponse<RequestResponse> requestResponse)
				{
					RetrieveRequestCallback(requestResponse, refreshFooter);
				}, requestParam.Id);
				return;
			}
			if (refreshFooter)
			{
				ClearConversationalFooter();
			}
			zendeskUI.AddScreenBackState(ticketResponsePanel, ZendeskScreen.TicketResponse);
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	private void RetrieveRequestCallback(ZendeskResponse<RequestResponse> requestResponse, bool refreshFooter)
	{
		if (requestResponse.IsError)
		{
			if (!zendeskErrorUI.IfAuthError(requestResponse.ErrorResponse, zendeskAuthType))
			{
				zendeskErrorUI.NavigateError(null, true, true);
			}
			zendeskUI.FinishLoading();
			return;
		}
		Request request = requestResponse.Result.Request;
		if (lastCommentDatetime == DateTime.MinValue)
		{
			if (zendeskUI.backStateGO.Count > 1 && zendeskUI.backStateGO[zendeskUI.backStateGO.Count - 1].ScreenGO == ticketResponsePanel)
			{
				zendeskUI.backStateGO[zendeskUI.backStateGO.Count - 1].Parameter = request.Id;
			}
			else
			{
				zendeskUI.AddScreenBackState(ticketResponsePanel, ZendeskScreen.TicketResponse, request);
			}
			if (this.request == null || this.request.Id != request.Id || this.request.Status != request.Status)
			{
				this.request = request;
			}
			GetRequestComments(this.request.Id, refreshFooter);
		}
		else
		{
			zendeskUI.AddScreenBackState(ticketResponsePanel, ZendeskScreen.TicketResponse, request);
			if (refreshFooter)
			{
				ClearConversationalFooter();
			}
			RefreshComments(this.request.Id);
		}
		zendeskUI.FinishLoading();
	}

	public void Close()
	{
		try
		{
			lastCommentDatetime = DateTime.MinValue;
			zendeskUI.CloseButton();
		}
		catch
		{
			zendeskErrorUI.NavigateError(null, true, true);
		}
	}

	public void CloseSupportUI()
	{
		try
		{
			base.gameObject.SetActive(value: false);
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	private void ClearCustomFields()
	{
		try
		{
			foreach (KeyValuePair<GameObject, FieldType> customFieldsPrefab in customFieldsPrefabs)
			{
				UnityEngine.Object.Destroy(customFieldsPrefab.Key);
			}
			customFieldsPrefabs.Clear();
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	private void AddCustomFieldsUi()
	{
		try
		{
			ClearCustomFields();
			InitLocalizationHandlerCustomFields();
			foreach (RequestFormField field in requestForm.Fields)
			{
				if (!CheckCustomFieldConfiguration(field))
				{
					continue;
				}
				GameObject gameObject = UnityEngine.Object.Instantiate(field.UIComponent, createRequestPanelFieldContainer.transform);
				string key = labelKey.Replace("{cf-id}", field.Id.ToString());
				string key2 = validationMessageKey.Replace("{cf-id}", field.Id.ToString());
				string translatedCustomField = GetTranslatedCustomField(key);
				translatedCustomField = (string.IsNullOrEmpty(translatedCustomField) ? field.Label : translatedCustomField);
				string translatedCustomField2 = GetTranslatedCustomField(key2);
				translatedCustomField2 = (string.IsNullOrEmpty(translatedCustomField2) ? field.ValidationMessage : translatedCustomField2);
				if (field.FieldType == FieldType.Text || field.FieldType == FieldType.Numeric || field.FieldType == FieldType.Decimal || field.FieldType == FieldType.Multiline)
				{
					gameObject.GetComponent<ZendeskCustomTextFieldScript>().init(field.Id, translatedCustomField, translatedCustomField2, field.Placeholder, field.Required, zendeskLocalizationHandler.translationGameObjects["usdk_required_field_label"], requiredSymbol);
				}
				else if (field.FieldType == FieldType.Checkbox)
				{
					gameObject.GetComponent<ZendeskCustomCheckboxFieldScript>().init(field.Id, translatedCustomField);
				}
				else if (field.FieldType == FieldType.Unknown)
				{
					ZendeskCustomDropdownFieldScript component = gameObject.GetComponent<ZendeskCustomDropdownFieldScript>();
					if (component != null)
					{
						List<DropdownOptions> list = new List<DropdownOptions>();
						DropdownOptions dropdownOptions = new DropdownOptions();
						dropdownOptions.Text = "-";
						dropdownOptions.Tag = "-";
						list.Add(dropdownOptions);
						list.AddRange(field.DropDownOptions);
						foreach (DropdownOptions item in list)
						{
							string translatedCustomField3 = GetTranslatedCustomField("usdk_" + field.Id + "_" + item.Text);
							if (!string.IsNullOrEmpty(translatedCustomField3))
							{
								item.Text = translatedCustomField3;
							}
						}
						component.init(field.Id, translatedCustomField, list.ToArray(), translatedCustomField2, field.Placeholder, field.Required);
					}
				}
				customFieldsPrefabs.Add(gameObject, field.FieldType);
			}
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	private bool CheckCustomFieldConfiguration(RequestFormField item)
	{
		try
		{
			if (item == null)
			{
				Debug.Log(zendeskLocalizationHandler.translationGameObjects["usdk_custom_field_conf_success"]);
				return false;
			}
			if (item.Id == 0L)
			{
				Debug.Log(zendeskLocalizationHandler.translationGameObjects["usdk_custom_field_conf_id_fail"]);
				return false;
			}
			if (item.UIComponent == null)
			{
				Debug.Log(zendeskLocalizationHandler.translationGameObjects["usdk_custom_field_conf_ui_component_fail"]);
				return false;
			}
		}
		catch (Exception ex)
		{
			throw ex;
		}
		return true;
	}

	public void LoadCustomFieldsFromCSV()
	{
		if (customFieldsFile == null)
		{
			Debug.Log(zendeskLocalizationHandler.translationGameObjects["usdk_custom_field_conf_csv_not_found_error"]);
			return;
		}
		List<RequestFormField> list = new List<RequestFormField>();
		requestForm = new RequestFormExtended();
		List<string> list2 = new List<string>();
		string[] array = customFieldsFile.text.Split('\n');
		bool flag = true;
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (string.IsNullOrEmpty(text))
			{
				continue;
			}
			string[] array3 = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))").Split(text);
			string[] array4;
			if (flag)
			{
				array4 = array3;
				foreach (string item in array4)
				{
					list2.Add(item);
				}
				flag = false;
				continue;
			}
			int num = 0;
			RequestFormField requestFormField = new RequestFormField();
			array4 = array3;
			foreach (string text2 in array4)
			{
				if (list2[num].ToUpper() == "TITLE")
				{
					if (text2.ToUpper().Equals("SUBJECT") || text2.ToUpper().Equals("DESCRIPTION") || text2.ToUpper().Equals("STATUS") || text2.ToUpper().Equals("TYPE") || text2.ToUpper().Equals("PRIORITY") || text2.ToUpper().Equals("GROUP") || text2.ToUpper().Equals("ASSIGNEE"))
					{
						requestFormField = null;
						break;
					}
					requestFormField.Label = text2;
				}
				else if (list2[num].ToUpper().StartsWith("FIELD ID"))
				{
					requestFormField.Id = long.Parse(text2);
				}
				else if (list2[num].ToUpper().StartsWith("REQUIRED FOR END USERS"))
				{
					requestFormField.Required = bool.Parse(text2);
				}
				else if (list2[num].ToUpper().Equals("TYPE"))
				{
					string text3 = text2;
					if (text3.ToUpper().StartsWith("MULTI-LINE"))
					{
						text3 = "Multiline";
					}
					else if (text3.ToUpper().StartsWith("DROP-DOWN"))
					{
						text3 = "Dropdown";
					}
					try
					{
						requestFormField.FieldType = (FieldType)Enum.Parse(typeof(FieldType), text3, ignoreCase: true);
					}
					catch
					{
						requestFormField = null;
						break;
					}
					if (requestFormField.FieldType == FieldType.Text)
					{
						requestFormField.UIComponent = CustomTextFieldPrefab;
					}
					else if (requestFormField.FieldType == FieldType.Multiline)
					{
						requestFormField.UIComponent = CustomTextMultilineFieldPrefab;
					}
					if (requestFormField.FieldType == FieldType.Checkbox)
					{
						requestFormField.UIComponent = CustomCheckboxFieldPrefab;
					}
					else if (requestFormField.FieldType == FieldType.Decimal)
					{
						requestFormField.UIComponent = CustomDecimalFieldPrefab;
					}
					else if (requestFormField.FieldType == FieldType.Numeric)
					{
						requestFormField.UIComponent = CustomNumericFieldPrefab;
					}
				}
				else if (list2[num].ToUpper() == "EDITABLE FOR END USERS")
				{
					if (bool.Parse(text2))
					{
						num++;
						continue;
					}
					requestFormField = null;
					break;
				}
				num++;
			}
			if (requestFormField != null)
			{
				list.Add(requestFormField);
			}
		}
		requestForm.Fields = list;
	}

	private List<CustomField> GetCustomFieldsValues()
	{
		List<CustomField> list = new List<CustomField>();
		foreach (KeyValuePair<GameObject, FieldType> customFieldsPrefab in customFieldsPrefabs)
		{
			if (customFieldsPrefab.Value == FieldType.Text || customFieldsPrefab.Value == FieldType.Numeric || customFieldsPrefab.Value == FieldType.Decimal || customFieldsPrefab.Value == FieldType.Multiline)
			{
				ZendeskCustomTextFieldScript component = customFieldsPrefab.Key.GetComponent<ZendeskCustomTextFieldScript>();
				list.Add(new CustomField
				{
					Id = component.idCustomField,
					Value = component.inputField.text
				});
			}
			else if (customFieldsPrefab.Value == FieldType.Checkbox)
			{
				ZendeskCustomCheckboxFieldScript component2 = customFieldsPrefab.Key.GetComponent<ZendeskCustomCheckboxFieldScript>();
				list.Add(new CustomField
				{
					Id = component2.idCustomField,
					Value = component2.inputField.isOn.ToString().ToLower()
				});
			}
			else
			{
				if (customFieldsPrefab.Value != 0)
				{
					continue;
				}
				ZendeskCustomDropdownFieldScript component3 = customFieldsPrefab.Key.GetComponent<ZendeskCustomDropdownFieldScript>();
				if (!(component3 != null))
				{
					continue;
				}
				string text = component3.dropdown.captionText.text;
				string value = null;
				DropdownOptions[] dropdownOptions = component3.dropdownOptions;
				foreach (DropdownOptions dropdownOptions2 in dropdownOptions)
				{
					if (dropdownOptions2.Text == text)
					{
						value = dropdownOptions2.Tag;
					}
				}
				list.Add(new CustomField
				{
					Id = component3.idCustomField,
					Value = value
				});
			}
		}
		return list;
	}

	public void SetInvisibleCustomFields(Dictionary<long, string> customFieldsDictionary)
	{
		try
		{
			if (customFields == null)
			{
				customFields = new List<CustomField>();
			}
			foreach (KeyValuePair<long, string> item in customFieldsDictionary)
			{
				if (!ValidateInvisibleCustomFieldItem(item.Key, item.Value))
				{
					continue;
				}
				if (customFields.Select((CustomField a) => a.Id).Contains(item.Key))
				{
					customFields.FirstOrDefault((CustomField a) => a.Id == item.Key).Value = item.Value;
				}
				else
				{
					customFields.Add(new CustomField
					{
						Id = item.Key,
						Value = item.Value
					});
				}
			}
		}
		catch
		{
		}
	}

	public void AddInvisibleCustomField(long id, string value)
	{
		try
		{
			if (customFields == null)
			{
				customFields = new List<CustomField>();
			}
			if (!ValidateInvisibleCustomFieldItem(id, value))
			{
				return;
			}
			if (customFields.Select((CustomField a) => a.Id).Contains(id))
			{
				customFields.FirstOrDefault((CustomField a) => a.Id == id).Value = value;
			}
			else
			{
				customFields.Add(new CustomField
				{
					Id = id,
					Value = value
				});
			}
		}
		catch
		{
		}
	}

	private bool ValidateInvisibleCustomFieldItem(long id, string value)
	{
		bool result = true;
		if (id < 1)
		{
			Debug.Log(zendeskLocalizationHandler.translationGameObjects["usdk_custom_field_conf_id_fail"]);
			return false;
		}
		return result;
	}

	public void BackToMyTicketsScreen()
	{
		try
		{
			lastCommentDatetime = DateTime.MinValue;
			prevDate = DateTime.MinValue;
			ShowMyTicketsWindow();
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	public void AttachmentDeleteButtonClick(GameObject attachment)
	{
		if (attachmentPaths.Contains(attachment.GetComponent<ZendeskTicketAttachmentThumbnail>().GetPath()))
		{
			attachmentPaths = attachmentPaths.Where((string val) => val != attachment.GetComponent<ZendeskTicketAttachmentThumbnail>().GetPath()).ToList();
		}
		UnityEngine.Object.Destroy(attachment);
		if (isConversationsEnabled)
		{
			ticketResponseAttachmentCountText.text = attachmentPaths.Count.ToString();
			if (attachmentPaths.Count == 0)
			{
				ticketResponseAttachmentCountContainer.SetActive(value: false);
				ticketResponseAttachmentCountText.text = string.Empty;
				createRequestAttachmentLine.SetActive(value: false);
				CheckAvailabilityOfSendButton(null);
			}
		}
		else
		{
			createRequestAttachmentCountText.text = attachmentPaths.Count.ToString();
			if (attachmentPaths.Count == 0)
			{
				createRequestAttachmentCountContainer.SetActive(value: false);
				createRequestAttachmentCountText.text = string.Empty;
				createRequestPanelAttachmentLine.SetActive(value: false);
			}
		}
	}

	public void ImageSavedToTemporaryPath(string path)
	{
		GameObject gameObject = null;
		try
		{
			if (string.IsNullOrEmpty(path))
			{
				return;
			}
			if (path.Equals("error"))
			{
				zendeskErrorUI.NavigateError(zendeskLocalizationHandler.translationGameObjects["usdk_file_permission_denied_generic"], false, true);
			}
			else if (path.Equals("error_res"))
			{
				zendeskErrorUI.NavigateError(zendeskLocalizationHandler.translationGameObjects["usdk_file_exceeds_max_resolution"], false, true);
			}
			else if (attachmentPaths.Count == 0 || !attachmentPaths.Contains(path))
			{
				if (isConversationsEnabled)
				{
					gameObject = UnityEngine.Object.Instantiate(attachmentPrefab, attachmentsView.transform);
					if (string.IsNullOrEmpty(ticketId))
					{
						CheckAvailabilityOfSendButton(null);
					}
					else
					{
						CheckAvailabilityOfSendButton(true);
					}
				}
				else
				{
					gameObject = UnityEngine.Object.Instantiate(attachmentPrefab, attachmentsViewNonConversational.transform);
				}
				gameObject.transform.SetAsFirstSibling();
				gameObject.GetComponent<ZendeskTicketAttachmentThumbnail>().Init(path, maxAttachmentSize, this, zendeskLocalizationHandler.translationGameObjects["usdk_file_exceeds_max_size_20_mb"], zendeskLocalizationHandler.translationGameObjects["usdk_file_exceeds_max_resolution"]);
				attachmentPaths.Add(path);
				if (isConversationsEnabled)
				{
					ticketResponseAttachmentCountText.text = attachmentPaths.Count.ToString();
				}
				else
				{
					createRequestAttachmentCountText.text = attachmentPaths.Count.ToString();
				}
				if (attachmentPaths.Count == 1)
				{
					createRequestAttachmentLine.SetActive(value: true);
					if (isConversationsEnabled)
					{
						ticketResponseAttachmentCountContainer.SetActive(value: true);
						createRequestAttachmentLine.SetActive(value: true);
					}
					else
					{
						createRequestAttachmentCountContainer.SetActive(value: true);
						createRequestPanelAttachmentLine.SetActive(value: true);
					}
				}
			}
			else
			{
				zendeskErrorUI.NavigateError(zendeskLocalizationHandler.translationGameObjects["usdk_attachment_is_already_selected"], false, true);
			}
		}
		catch (Exception ex)
		{
			if (gameObject != null)
			{
				UnityEngine.Object.Destroy(gameObject);
			}
			zendeskErrorUI.NavigateError(ex.Message, false, true);
			CheckAvailabilityOfSendButton(null);
		}
	}

	private void AddMessageDatePrefab()
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(messageDatePrefab, ticketResponseListView.transform);
		if (prevDate.Date.Equals(DateTime.Today.Date))
		{
			gameObject.GetComponentInChildren<Text>().text = zendeskMain.zendeskLocalizationHandler.translationGameObjects["usdk_today"];
		}
		else if (prevDate.Date.Equals(DateTime.Today.AddDays(-1.0).Date))
		{
			gameObject.GetComponentInChildren<Text>().text = zendeskMain.zendeskLocalizationHandler.translationGameObjects["usdk_yesterday"];
		}
		else
		{
			gameObject.GetComponentInChildren<Text>().text = prevDate.Day + " " + prevDate.ToString("MMMM", CultureInfo.CreateSpecificCulture(zendeskMain.zendeskLocalizationHandler.Locale));
			if (prevDate.Date.Year != DateTime.Today.Year)
			{
				Text componentInChildren = gameObject.GetComponentInChildren<Text>();
				componentInChildren.text = componentInChildren.text + " " + prevDate.Date.Year;
			}
		}
		ticketResponseContainerList.Add(gameObject);
	}
}
