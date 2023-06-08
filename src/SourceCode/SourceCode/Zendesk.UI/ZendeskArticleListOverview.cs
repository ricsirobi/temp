using System;
using UnityEngine;
using UnityEngine.UI;
using Zendesk.Internal.Models.HelpCenter;
using Zendesk.UI.HelpCenter;

namespace Zendesk.UI;

public class ZendeskArticleListOverview : MonoBehaviour
{
	public GameObject categoryGO;

	public GameObject categoryDividerLineGO;

	public Transform sectionContainer;

	public Text categoryText;

	public Text sectionText;

	[HideInInspector]
	public long sectionId;

	[HideInInspector]
	private ZendeskHelpCenterUI zendeskHelpCenterUI;

	private ZendeskErrorUI zendeskErrorUI;

	private Zendesk.Internal.Models.HelpCenter.Category category;

	private Action<Zendesk.Internal.Models.HelpCenter.Category> categorySelectedCallback;

	public void Init(Zendesk.Internal.Models.HelpCenter.Category cat, ZendeskHelpCenterUI helpcenter, ZendeskErrorUI zendeskErrorUI, Action<Zendesk.Internal.Models.HelpCenter.Category> callback)
	{
		try
		{
			category = cat;
			categorySelectedCallback = callback;
			zendeskHelpCenterUI = helpcenter;
			this.zendeskErrorUI = zendeskErrorUI;
			if (!string.IsNullOrEmpty(category.Name))
			{
				categoryText.text = category.Name;
				categoryGO.SetActive(value: true);
				categoryDividerLineGO.SetActive(value: true);
			}
			base.gameObject.SetActive(value: true);
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	public void Init(string sectionText, long sectionId, ZendeskHelpCenterUI helpcenter, ZendeskErrorUI zendeskErrorUI)
	{
		try
		{
			this.sectionId = sectionId;
			zendeskHelpCenterUI = helpcenter;
			this.zendeskErrorUI = zendeskErrorUI;
			this.sectionText.text = sectionText;
			base.gameObject.SetActive(value: true);
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	public void Init(string categoryText, string sectionText, long sectionId, ZendeskHelpCenterUI helpcenter, ZendeskErrorUI zendeskErrorUI)
	{
		try
		{
			this.sectionId = sectionId;
			zendeskHelpCenterUI = helpcenter;
			this.zendeskErrorUI = zendeskErrorUI;
			this.sectionText.text = sectionText;
			base.gameObject.SetActive(value: true);
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	public void button_OpenCategory()
	{
		categorySelectedCallback?.Invoke(category);
	}

	public void button_OpenSection()
	{
		try
		{
			zendeskHelpCenterUI.LoadArticlesFromSection(sectionId);
		}
		catch
		{
			zendeskErrorUI.NavigateError(null, true, true);
		}
	}
}
