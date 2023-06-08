using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Zendesk.Common;
using Zendesk.Internal.Models.Common;
using Zendesk.Internal.Models.Support;
using Zendesk.Providers;

namespace Zendesk.UI;

public class ZendeskTicketContentOverview : MonoBehaviour
{
	public GameObject agentImageGO;

	public GameObject closedImage;

	public GameObject pictureImage;

	private int commentCount;

	public GameObject lastCommentGO;

	private Text lastCommentText;

	public GameObject lastUpdateGO;

	private Text lastUpdateText;

	public GameObject newMessagesCountGO;

	private Text newMessagesCountText;

	public GameObject participantsGO;

	private Text participantsText;

	private Request request;

	private bool showNewMessagesCount;

	public GameObject subjectGO;

	public Font unreadFont;

	public GameObject unreadIcon;

	private ZendeskErrorUI zendeskErrorUI;

	private ZendeskSupportProvider zendeskSupportProvider;

	private ZendeskSupportUI ZendeskSupportUI;

	private ZendeskLocalizationHandler zendeskLocalizationHandler;

	public Text closedText;

	private void Start()
	{
		participantsText = participantsGO.GetComponent<Text>();
		lastCommentText = lastCommentGO.GetComponent<Text>();
		lastUpdateText = lastUpdateGO.GetComponent<Text>();
	}

	public void Init(Request r, ZendeskSupportProvider zendeskSupportProvider, ZendeskSupportUI zendeskSupportUI, ZendeskErrorUI zendeskErrorUI, ZendeskHtmlHandler zendeskHtmlHandler, ZendeskLocalizationHandler zendeskLocalizationHandler)
	{
		try
		{
			this.zendeskLocalizationHandler = zendeskLocalizationHandler;
			this.zendeskErrorUI = zendeskErrorUI;
			participantsText = participantsGO.GetComponent<Text>();
			lastCommentText = lastCommentGO.GetComponent<Text>();
			lastUpdateText = lastUpdateGO.GetComponent<Text>();
			ZendeskSupportUI = zendeskSupportUI;
			commentCount = r.CommentCount;
			closedText.text = zendeskLocalizationHandler.translationGameObjects["usdk_request_status_closed"];
			request = r;
			string text = "";
			if (r.LastCommentingAgents != null)
			{
				User lastAgent = r.LastCommentingAgents.FirstOrDefault();
				if (r.LastCommentingAgents.Count > 0 && lastAgent != null && lastAgent.Photo != null && lastAgent.Photo.ContentUrl != null)
				{
					if (zendeskSupportUI.ticketContentImageIds.ContainsKey(lastAgent.Photo.Id))
					{
						SetTextureToAgentImageGO(zendeskSupportUI.ticketContentImageIds[lastAgent.Photo.Id]);
					}
					else
					{
						zendeskSupportProvider.GetImage(delegate(ZendeskResponse<Texture2D> textureResponse)
						{
							GetImageOfAgentCallback(textureResponse, lastAgent.Photo.Id);
						}, lastAgent.Photo);
					}
				}
				else if (r.LastCommentingAgents.Count > 0 && lastAgent != null && lastAgent.Photo != null)
				{
					_ = lastAgent.Photo.ContentUrl;
				}
				using List<User>.Enumerator enumerator = r.LastCommentingAgents.GetEnumerator();
				if (enumerator.MoveNext())
				{
					User current = enumerator.Current;
					string[] array = current.Name.Split(' ');
					if (!string.IsNullOrEmpty(current.Name) && array.Length >= 1)
					{
						string text2 = array[0];
						if (text2.Length >= 24)
						{
							text2 = text2.Substring(0, 23);
						}
						text += $"{text2}";
					}
					else
					{
						text += $", {current.Name}";
					}
					text += ", ";
				}
			}
			text += zendeskLocalizationHandler.translationGameObjects["usdk_me"];
			string text3 = zendeskHtmlHandler.ClearHtmlText(r.LastComment.PlainBody);
			participantsText.text = zendeskHtmlHandler.ClearHtmlText(text);
			lastCommentText.text = text3;
			lastUpdateText.text = UpdatedAtDateFormat(r.PublicUpdatedAt);
			SupportRequestStorageItem supportRequest = ZendeskLocalStorage.GetSupportRequest(r.Id);
			if (supportRequest == null)
			{
				ZendeskLocalStorage.SaveSupportRequest(r, isRead: false);
				supportRequest = ZendeskLocalStorage.GetSupportRequest(r.Id);
			}
			if (supportRequest.newComments > 0 || commentCount > supportRequest.commentCount)
			{
				ZendeskLocalStorage.SaveSupportRequest(r, isRead: false);
				unreadIcon.SetActive(value: true);
				participantsText.GetComponent<Text>().font = unreadFont;
				lastCommentText.GetComponent<Text>().color = new Color32(47, 57, 65, byte.MaxValue);
				lastCommentText.GetComponent<Text>().font = unreadFont;
			}
			if (text3.StartsWith("[") && text3.EndsWith("]"))
			{
				pictureImage.SetActive(value: true);
				lastCommentText.text = zendeskLocalizationHandler.translationGameObjects["usdk_image"];
			}
			if (r.StatusEnum == RequestStatus.Closed)
			{
				closedImage.SetActive(value: true);
				lastCommentGO.SetActive(value: false);
				pictureImage.SetActive(value: false);
			}
			base.gameObject.SetActive(value: true);
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	private string UpdatedAtDateFormat(string date)
	{
		try
		{
			return ZendeskUtils.SetZendeskShortDateFormat(date, zendeskLocalizationHandler.Locale);
		}
		catch
		{
		}
		return string.Empty;
	}

	public void OpenFullTicket()
	{
		try
		{
			zendeskErrorUI.DestroyZendeskErrorToast();
			if (ZendeskSupportUI == null)
			{
				ZendeskSupportUI = GetComponentInParent<ZendeskSupportUI>();
			}
			if (request != null)
			{
				ZendeskSupportUI.RetrieveTicketResponseData(request, clearLastCommentDatetime: true);
			}
		}
		catch
		{
			zendeskErrorUI.NavigateError(null, true, true);
		}
	}

	private void GetImageOfAgentCallback(ZendeskResponse<Texture2D> textureResponse, long id)
	{
		try
		{
			if (textureResponse.IsError)
			{
				zendeskErrorUI.IfAuthError(textureResponse.ErrorResponse, ZendeskSupportUI.zendeskAuthType);
				return;
			}
			Texture2D result = textureResponse.Result;
			if (result != null)
			{
				if (!ZendeskSupportUI.ticketContentImageIds.ContainsKey(id))
				{
					ZendeskSupportUI.ticketContentImageIds.Add(id, result);
				}
				SetTextureToAgentImageGO(result);
			}
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	private void SetTextureToAgentImageGO(Texture2D texture)
	{
		agentImageGO.GetComponent<RawImage>().texture = texture;
		agentImageGO.GetComponent<RawImage>().color = Color.white;
		((RectTransform)agentImageGO.transform).sizeDelta = new Vector2(120f, 120f);
	}
}
