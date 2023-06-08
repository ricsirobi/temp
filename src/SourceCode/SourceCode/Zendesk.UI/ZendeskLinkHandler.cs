using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zendesk.Common;

namespace Zendesk.UI;

public class ZendeskLinkHandler : MonoBehaviour
{
	private ZendeskMain zendeskMain;

	private ZendeskCore zendeskCore;

	private string knowledgeBaseSuffix = "knowledge/";

	private string hcBaseSuffix = "hc/";

	private ZendeskErrorUI zendeskErrorUI;

	private Dictionary<string, string> linkDictionary;

	private void Start()
	{
		zendeskMain = GetComponent<ZendeskMain>();
		zendeskCore = GetComponent<ZendeskCore>();
		linkDictionary = new Dictionary<string, string>();
	}

	public void Init(ZendeskErrorUI zendeskErrorUI)
	{
		this.zendeskErrorUI = zendeskErrorUI;
		linkDictionary = new Dictionary<string, string>();
	}

	public string AddLink(string href)
	{
		if (linkDictionary == null)
		{
			linkDictionary = new Dictionary<string, string>();
		}
		string text = Guid.NewGuid().ToString("N").Substring(0, 2);
		while (linkDictionary.Keys.Contains(text))
		{
			text = Guid.NewGuid().ToString("N").Substring(0, 2);
		}
		linkDictionary.Add(text, href);
		return text;
	}

	public void HandleLink(string link)
	{
		link = linkDictionary.Where((KeyValuePair<string, string> a) => a.Key == link).FirstOrDefault().Value;
		try
		{
			if (IsInternalLink(link))
			{
				zendeskMain.zendeskUI.zendeskHelpCenterUI.LoadIndividualArticle(GetArticleId(link), GetArticleLocale(link));
			}
			else
			{
				Application.OpenURL(link);
			}
		}
		catch
		{
			zendeskErrorUI.NavigateError(null, true, true);
		}
	}

	public bool IsInternalLink(string link)
	{
		if (string.IsNullOrEmpty(link) || string.IsNullOrEmpty(zendeskCore.ZendeskUrl) || string.IsNullOrEmpty(knowledgeBaseSuffix) || string.IsNullOrEmpty(hcBaseSuffix))
		{
			return false;
		}
		try
		{
			string text = new Uri(zendeskCore.ZendeskUrl).Host + "/";
			if (link.ToUpper().Contains(text.ToUpper() + knowledgeBaseSuffix.ToUpper()) || link.ToUpper().Contains(text.ToUpper() + hcBaseSuffix.ToUpper()))
			{
				return true;
			}
		}
		catch (Exception ex)
		{
			throw ex;
		}
		return false;
	}

	public long GetArticleId(string articleURL)
	{
		try
		{
			if (!articleURL.ToUpper().Contains("ARTICLES"))
			{
				return 0L;
			}
			return long.Parse(articleURL.Split('/').Last().Split('-')
				.First());
		}
		catch
		{
			return 0L;
		}
	}

	public string GetArticleLocale(string articleURL)
	{
		try
		{
			return articleURL.Split('/')[4];
		}
		catch
		{
			return zendeskMain.zendeskLocalizationHandler.Locale;
		}
	}
}
