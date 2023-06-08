using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using Zendesk.Internal.Models.Common;
using Zendesk.Internal.Models.Support;
using Zendesk.Providers;

namespace Zendesk.UI;

public class ZendeskTicketResponse : MonoBehaviour
{
	public GameObject agentNameContainerGO;

	public GameObject agentNameGO;

	public GameObject agentPhotoGO;

	public GameObject agentResponseGO;

	public GameObject imageResponseGO;

	private Text agentResponseText;

	public GameObject endUserResponseGO;

	private Text endUserResponseText;

	public GameObject endUserMessagePanelGO;

	private Text responseOwnerNameText;

	public GameObject agentPhotoContainerGO;

	public Texture placeholderLandscape;

	public Texture placeholderPortrait;

	private bool isAgent;

	private ZendeskSupportProvider supportProvider;

	private ZendeskErrorUI zendeskErrorUI;

	private ZendeskSupportUI zendeskSupportUI;

	private ZendeskHtmlHandler zendeskHtmlHandler;

	private ZendeskMain zendeskMain;

	private ZendeskLocalizationHandler zendeskLocalizationHandler;

	private List<string> supportedImageExtensions = new List<string> { "image/jpeg", "image/png" };

	private string requestId;

	public void Init(CommentResponse comment, ZendeskSupportProvider zendeskSupportProvider, ZendeskSupportUI zendeskSupportUI, ZendeskErrorUI zendeskErrorUI, User author, bool isPreviousAuthor, Attachment agentPhotoAttachment, Attachment attachment, ZendeskHtmlHandler zendeskHtmlHandler, ZendeskLocalizationHandler zendeskLocalizationHandler)
	{
		try
		{
			zendeskMain = zendeskHtmlHandler.GetComponent<ZendeskMain>();
			this.zendeskSupportUI = zendeskSupportUI;
			this.zendeskLocalizationHandler = zendeskLocalizationHandler;
			this.zendeskHtmlHandler = zendeskHtmlHandler;
			this.zendeskErrorUI = zendeskErrorUI;
			supportProvider = zendeskSupportProvider;
			if (comment != null)
			{
				requestId = comment.RequestId;
				if (comment.Agent)
				{
					string text = zendeskHtmlHandler.HandleHtmlComponents(this.zendeskHtmlHandler.ParseHtmlRequest(comment.HtmlBody));
					agentResponseGO.GetComponent<ZendeskTextPic>().init(text, zendeskMain.zendeskLinkHander);
					isAgent = comment.Agent;
					if (isPreviousAuthor)
					{
						agentNameContainerGO.gameObject.SetActive(value: false);
					}
					else
					{
						responseOwnerNameText = agentNameGO.GetComponent<Text>();
						string[] array = author.Name.Split(' ');
						if (author.Name != null && array.Length >= 1)
						{
							string text2 = array[0];
							if (text2.Length >= 36)
							{
								text2 = text2.Substring(0, 35);
							}
							author.Name = $"{text2}";
						}
						responseOwnerNameText.text = author.Name;
					}
					if (agentPhotoAttachment != null && agentPhotoAttachment.ContentUrl != null)
					{
						if (zendeskSupportUI.ticketContentImageIds.ContainsKey(agentPhotoAttachment.Id))
						{
							SetAgentImage(zendeskSupportUI.ticketContentImageIds[agentPhotoAttachment.Id]);
						}
						else
						{
							zendeskSupportProvider.GetImage(delegate(ZendeskResponse<Texture2D> textureResponse)
							{
								GetImageOfAgentCallback(textureResponse, agentPhotoAttachment.Id);
							}, agentPhotoAttachment);
						}
					}
					else if (agentPhotoAttachment != null && agentPhotoAttachment.ContentUrl != null)
					{
					}
				}
				else
				{
					endUserResponseText = endUserResponseGO.GetComponent<Text>();
					SetCommentBody(comment.Body);
				}
				AddAttachments(comment.Attachments);
			}
			base.gameObject.SetActive(value: true);
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	public void Init(Comment comment, ZendeskSupportUI zendeskSupportUI, ZendeskSupportProvider zendeskSupportProvider, ZendeskErrorUI zendeskErrorUI, ZendeskLocalizationHandler zendeskLocalizationHandler)
	{
		try
		{
			this.zendeskErrorUI = zendeskErrorUI;
			this.zendeskLocalizationHandler = zendeskLocalizationHandler;
			this.zendeskSupportUI = zendeskSupportUI;
			supportProvider = zendeskSupportProvider;
			requestId = comment.RequestId;
			endUserResponseText = endUserResponseGO.GetComponent<Text>();
			SetCommentBody(comment.Body);
			AddAttachments(comment.Attachments);
			base.gameObject.SetActive(value: true);
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	public void Init(string comment, ZendeskSupportUI zendeskSupportUI, ZendeskErrorUI zendeskErrorUI, ZendeskLocalizationHandler zendeskLocalizationHandler)
	{
		try
		{
			this.zendeskErrorUI = zendeskErrorUI;
			this.zendeskLocalizationHandler = zendeskLocalizationHandler;
			this.zendeskSupportUI = zendeskSupportUI;
			endUserResponseText = endUserResponseGO.GetComponent<Text>();
			endUserResponseText.text = comment;
			base.gameObject.SetActive(value: true);
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	private void SetCommentBody(string body)
	{
		try
		{
			if (body.StartsWith("[") && body.EndsWith("]"))
			{
				if (!isAgent)
				{
					endUserMessagePanelGO.SetActive(value: false);
					endUserResponseText.transform.parent.gameObject.SetActive(value: false);
				}
				else
				{
					agentResponseGO.transform.parent.gameObject.SetActive(value: false);
				}
			}
			else
			{
				endUserResponseText.text = body;
			}
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	private void AddAttachments(List<Attachment> attachments)
	{
		try
		{
			if (attachments == null || attachments.Count <= 0 || zendeskSupportUI.ticketId != requestId)
			{
				return;
			}
			for (int i = 1; i <= attachments.Count; i++)
			{
				if (!isContentTypeSupported(attachments[i - 1]))
				{
					return;
				}
				int widthUnity = 0;
				int heightUnity = 0;
				ParseImageDimensionsToSDK((int)attachments[i - 1].Height, (int)attachments[i - 1].Width, out heightUnity, out widthUnity);
				imageResponseGO.transform.GetChild(0).GetComponent<RawImage>().texture = GetPlaceholderTexture((int)attachments[i - 1].Height, (int)attachments[i - 1].Width);
				imageResponseGO.transform.GetChild(0).GetComponent<LayoutElement>().preferredHeight = heightUnity;
				imageResponseGO.transform.GetChild(0).GetComponent<LayoutElement>().preferredWidth = widthUnity;
				GameObject gameObject;
				if (isAgent)
				{
					gameObject = UnityEngine.Object.Instantiate(imageResponseGO, agentResponseGO.transform.parent.parent.transform);
				}
				else
				{
					gameObject = UnityEngine.Object.Instantiate(imageResponseGO, endUserResponseGO.transform.parent.parent.parent);
					gameObject.transform.SetAsLastSibling();
				}
				GameObject obj = gameObject;
				obj.name = obj.name + "_" + attachments[i - 1].Id;
			}
			foreach (Attachment att in attachments)
			{
				if (IsImageResolutionAcceptable((int)att.Height, (int)att.Width))
				{
					supportProvider.GetImage(delegate(ZendeskResponse<Texture2D> textureResponse)
					{
						GetAttachmentCallback(textureResponse, att);
					}, att);
				}
				else
				{
					zendeskErrorUI.NavigateError(zendeskLocalizationHandler.translationGameObjects["usdk_file_exceeds_max_resolution"], false, true);
				}
			}
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	private void GetImageOfAgentCallback(ZendeskResponse<Texture2D> textureResponse, long id)
	{
		try
		{
			if (textureResponse.IsError)
			{
				zendeskErrorUI.IfAuthError(textureResponse.ErrorResponse, zendeskSupportUI.zendeskAuthType);
			}
			else if (!textureResponse.IsError && textureResponse.Result != null)
			{
				if (!zendeskSupportUI.ticketContentImageIds.ContainsKey(id))
				{
					zendeskSupportUI.ticketContentImageIds.Add(id, textureResponse.Result);
				}
				SetAgentImage(textureResponse.Result);
			}
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	private bool isContentTypeSupported(Attachment attachment)
	{
		if (attachment != null && !string.IsNullOrEmpty(attachment.ContentType) && supportedImageExtensions.Contains(attachment.ContentType.ToLower()))
		{
			return true;
		}
		return false;
	}

	private void GetAttachmentCallback(ZendeskResponse<Texture2D> textureResponse, Attachment attachment)
	{
		try
		{
			if (zendeskSupportUI.ticketId != requestId)
			{
				return;
			}
			if (textureResponse.IsError && !zendeskErrorUI.IfAuthError(textureResponse.ErrorResponse, zendeskSupportUI.zendeskAuthType))
			{
				zendeskErrorUI.NavigateError(zendeskLocalizationHandler.translationGameObjects["usdk_fail_fetch_attachment"], false, true);
			}
			Texture result = textureResponse.Result;
			if (result != null)
			{
				Transform transform;
				float num;
				if (!isAgent)
				{
					transform = endUserResponseGO.transform.parent.parent.parent.Find(imageResponseGO.name + "(Clone)_" + attachment.Id).transform;
					num = endUserResponseGO.transform.parent.parent.parent.GetComponent<RectTransform>().rect.width - 24f;
				}
				else
				{
					transform = agentResponseGO.transform.parent.parent.Find(imageResponseGO.name + "(Clone)_" + attachment.Id).transform;
					transform.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;
					num = agentResponseGO.transform.parent.parent.parent.GetComponent<RectTransform>().rect.width - 24f;
				}
				Transform child = transform.transform.GetChild(0);
				child.GetComponent<RawImage>().texture = result;
				child.parent.GetComponent<RawImage>().texture = GetPlaceholderTexture(result.height, result.width);
				ParseImageDimensionsToSDK(result.height, result.width, out var heightUnity, out var widthUnity);
				child.GetComponent<LayoutElement>().preferredWidth = widthUnity;
				child.GetComponent<LayoutElement>().preferredHeight = heightUnity;
				if ((float)result.width > num)
				{
					child.GetComponent<LayoutElement>().preferredWidth = num;
					child.GetComponent<LayoutElement>().preferredHeight = num * (float)result.height / (float)result.width;
				}
				else
				{
					child.GetComponent<LayoutElement>().preferredWidth = result.width;
					child.GetComponent<LayoutElement>().preferredHeight = result.height;
				}
				child.GetComponent<RawImage>().color = Color.white;
			}
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	private void ParseImageDimensionsToSDK(int height, int width, out int heightUnity, out int widthUnity)
	{
		try
		{
			float num = zendeskSupportUI.ticketResponseScrollView.GetComponent<RectTransform>().rect.width - (float)zendeskSupportUI.endUserResponsePrefab.GetComponent<HorizontalLayoutGroup>().padding.left;
			if ((float)width > num)
			{
				widthUnity = (int)num;
				heightUnity = (int)(num * (float)height) / width;
			}
			else
			{
				widthUnity = width;
				heightUnity = height;
			}
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	private Texture GetPlaceholderTexture(int height, int width)
	{
		try
		{
			if (width > height)
			{
				return placeholderLandscape;
			}
			return placeholderPortrait;
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	public void MessageSentStatus(bool sent)
	{
	}

	private void SetAgentImage(Texture2D texture)
	{
		agentPhotoGO.gameObject.GetComponent<RawImage>().texture = texture;
		agentPhotoGO.gameObject.GetComponent<RawImage>().color = Color.white;
		((RectTransform)agentPhotoGO.transform).sizeDelta = new Vector2(96f, 96f);
	}

	private bool IsImageResolutionAcceptable(int height, int width)
	{
		if (height > 4096 || width > 4096)
		{
			return false;
		}
		return true;
	}
}
