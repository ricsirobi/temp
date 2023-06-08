using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using Zendesk.Internal.Models.Common;
using Zendesk.Internal.Models.HelpCenter;
using Zendesk.Internal.Models.Support;
using Zendesk.UI;

public class ZendeskArticleRender : MonoBehaviour
{
	private ZendeskMain zendeskMain;

	public GameObject articleRenderTargetGO;

	public GameObject textPrefabGO;

	public GameObject imagePrefabGO;

	public GameObject headerPrefabGO;

	private GameObject textPrefabInstatiated;

	private string articleContent = "";

	private string lastText = "";

	public Texture imagePlaceholder;

	private ZendeskErrorUI zendeskErrorUI;

	private ZendeskLocalizationHandler zendeskLocalizationHandler;

	private ZendeskLinkHandler linkHandler;

	private List<GameObject> articleComponents;

	public void Init(ZendeskMain zendeskMain, ZendeskErrorUI zendeskErrorUI, ZendeskLocalizationHandler zendeskLocalizationHandler)
	{
		this.zendeskMain = zendeskMain;
		this.zendeskErrorUI = zendeskErrorUI;
		this.zendeskLocalizationHandler = zendeskLocalizationHandler;
		imagePrefabGO.GetComponentInChildren<RawImage>().texture = imagePlaceholder;
		linkHandler = zendeskMain.zendeskLinkHander.GetComponent<ZendeskLinkHandler>();
		articleComponents = new List<GameObject>();
	}

	private void Start()
	{
		linkHandler = zendeskMain.zendeskLinkHander.GetComponent<ZendeskLinkHandler>();
	}

	public void RenderArticle(Article article)
	{
		ClearArticleView();
		List<ZendeskHtmlComponent> list = zendeskMain.zendeskHtmlHandler.ParseHtml(article.Body);
		int num = 0;
		foreach (ZendeskHtmlComponent item in list)
		{
			switch (item.zendeskHtmlComponentType)
			{
			case ZendeskHtmlComponentType.HEADER:
				if (!lastText.Equals(item) && !PreviousTextIsSameLinkText(lastText, item.text))
				{
					AddHeaderComponent(item.text, item.attributes);
				}
				break;
			case ZendeskHtmlComponentType.TEXT:
				if (!lastText.Equals(item) && !PreviousTextIsSameLinkText(lastText, item.text))
				{
					articleContent += item.text;
					lastText = item.text;
				}
				break;
			case ZendeskHtmlComponentType.IMAGE:
				AddTextComponent();
				AddImage(item.link, num);
				num++;
				articleContent = string.Empty;
				break;
			case ZendeskHtmlComponentType.LINK:
				if (!lastText.Equals(item))
				{
					articleContent = articleContent + "<a href=" + linkHandler.AddLink(item.link) + ">";
					articleContent = articleContent + item.text + "</a>";
				}
				lastText = item.text;
				break;
			case ZendeskHtmlComponentType.LINEBREAK:
				AddTextComponent();
				articleContent = string.Empty;
				break;
			}
		}
		if (!string.IsNullOrEmpty(articleContent))
		{
			AddTextComponent();
		}
		zendeskMain.zendeskUI.FinishLoading();
	}

	private void AddImage(string imageURL, int orderNumber)
	{
		GameObject gameObjectImg = UnityEngine.Object.Instantiate(imagePrefabGO, articleRenderTargetGO.transform);
		gameObjectImg.GetComponentInChildren<LayoutElement>().preferredHeight = 512f;
		gameObjectImg.GetComponentInChildren<LayoutElement>().preferredWidth = 512f;
		GameObject obj = gameObjectImg;
		obj.name = obj.name + "_" + orderNumber;
		articleComponents.Add(gameObjectImg);
		zendeskMain.supportProvider.GetImage(delegate(ZendeskResponse<Texture2D> textureResponse)
		{
			try
			{
				if (textureResponse.IsError)
				{
					zendeskErrorUI.NavigateError(zendeskLocalizationHandler.translationGameObjects["usdk_fail_fetch_attachment"], false, true);
				}
				else
				{
					int widthUnity = 0;
					int heightUnity = 0;
					Texture2D result = textureResponse.Result;
					if (result != null)
					{
						if (IsImageResolutionAcceptable(result.height, result.width))
						{
							ParseImageDimensionsToSDK(result.height, result.width, out heightUnity, out widthUnity);
							gameObjectImg.GetComponentInChildren<LayoutElement>().preferredHeight = heightUnity;
							gameObjectImg.GetComponentInChildren<LayoutElement>().preferredWidth = widthUnity;
							gameObjectImg.GetComponentInChildren<RawImage>().texture = result;
							gameObjectImg.GetComponentInChildren<RawImage>().color = Color.white;
						}
						else
						{
							zendeskErrorUI.NavigateError(zendeskLocalizationHandler.translationGameObjects["usdk_fail_fetch_attachment"], false, true);
						}
					}
				}
			}
			catch
			{
				zendeskErrorUI.NavigateError(zendeskLocalizationHandler.translationGameObjects["usdk_fail_fetch_attachment"], false, true);
			}
			finally
			{
				gameObjectImg = null;
			}
		}, new Attachment
		{
			ContentUrl = imageURL
		}, useAuthentication: false);
	}

	private void AddTextComponent()
	{
		if (!string.IsNullOrEmpty(articleContent))
		{
			if (textPrefabInstatiated == null && !string.IsNullOrEmpty(articleContent))
			{
				textPrefabInstatiated = UnityEngine.Object.Instantiate(textPrefabGO, articleRenderTargetGO.transform);
			}
			textPrefabInstatiated.GetComponent<ZendeskTextPic>().init(articleContent, zendeskMain.zendeskLinkHander);
			articleComponents.Add(textPrefabInstatiated);
			textPrefabInstatiated = null;
			articleContent = string.Empty;
			lastText = string.Empty;
		}
	}

	private void AddHeaderComponent(string headerContent, Dictionary<ZendeskHtmlAttributeType, string> attributes)
	{
		if (string.IsNullOrEmpty(headerContent))
		{
			return;
		}
		if (textPrefabInstatiated == null && !string.IsNullOrEmpty(headerContent))
		{
			textPrefabInstatiated = UnityEngine.Object.Instantiate(headerPrefabGO, articleRenderTargetGO.transform);
		}
		ZendeskTextPic componentInChildren = textPrefabInstatiated.transform.GetComponentInChildren<ZendeskTextPic>();
		componentInChildren.init(headerContent, zendeskMain.zendeskLinkHander);
		if (attributes.ContainsKey(ZendeskHtmlAttributeType.HEADER))
		{
			string text = attributes[ZendeskHtmlAttributeType.HEADER].ToUpper();
			if (text.Equals("H1"))
			{
				componentInChildren.fontSize = 78;
			}
			else if (text.Equals("H2"))
			{
				componentInChildren.fontSize = 60;
			}
			else if (text.Equals("H3"))
			{
				componentInChildren.font = Resources.Load<Font>("ZendeskFonts/Roboto/Roboto-Regular");
				componentInChildren.fontSize = 54;
			}
			else if (text.Equals("H4"))
			{
				componentInChildren.fontSize = 48;
			}
		}
		articleComponents.Add(textPrefabInstatiated);
		textPrefabInstatiated = null;
	}

	private void ClearArticleView()
	{
		try
		{
			lastText = "";
			foreach (GameObject articleComponent in articleComponents)
			{
				UnityEngine.Object.Destroy(articleComponent);
			}
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	private void ParseImageDimensionsToSDK(int height, int width, out int heightUnity, out int widthUnity)
	{
		float num = articleRenderTargetGO.GetComponent<RectTransform>().rect.width - (float)(articleRenderTargetGO.GetComponent<VerticalLayoutGroup>().padding.left + articleRenderTargetGO.GetComponent<VerticalLayoutGroup>().padding.right);
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

	private bool PreviousTextIsSameLinkText(string lastText, string currentText)
	{
		try
		{
			bool result = false;
			if (lastText.Equals(currentText))
			{
				return true;
			}
			if (lastText.EndsWith(currentText + "</a>"))
			{
				return true;
			}
			return result;
		}
		catch (Exception ex)
		{
			throw ex;
		}
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
