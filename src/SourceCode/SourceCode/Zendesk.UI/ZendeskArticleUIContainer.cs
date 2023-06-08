using System;
using UnityEngine;
using UnityEngine.UI;
using Zendesk.Internal.Models.HelpCenter;
using Zendesk.UI.HelpCenter;

namespace Zendesk.UI;

public class ZendeskArticleUIContainer : MonoBehaviour
{
	public GameObject articleDetailsPanel;

	public Text articleTitleText;

	private ZendeskErrorUI zendeskErrorUI;

	public Text articleAuthorText;

	public Text articleVotingText;

	private Article article;

	private ZendeskHelpCenterUI zendeskHelpCenterUI;

	public void Init(Article article, ZendeskErrorUI zendeskErrorUI, ZendeskHelpCenterUI zendeskHelpCenterUi)
	{
		try
		{
			base.gameObject.name = $"Article_{article.Id.ToString()}";
			this.zendeskErrorUI = zendeskErrorUI;
			zendeskHelpCenterUI = zendeskHelpCenterUi;
			articleTitleText.text = zendeskHelpCenterUi.ClearHtmlText(article.Title);
			if (articleAuthorText != null)
			{
				articleAuthorText.text = zendeskHelpCenterUi.ClearHtmlText(article.Author.Name);
			}
			if (zendeskHelpCenterUI.showArticleVotes && article.VoteSum > 0)
			{
				if (articleVotingText != null)
				{
					articleVotingText.text = article.VoteSum.ToString();
				}
			}
			else
			{
				articleDetailsPanel.SetActive(value: false);
			}
			this.article = article;
			base.gameObject.SetActive(value: true);
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	public void button_OpenArticle()
	{
		try
		{
			zendeskHelpCenterUI.LoadIndividualArticle(article);
		}
		catch
		{
			zendeskErrorUI.NavigateError(null, true, true);
		}
	}
}
