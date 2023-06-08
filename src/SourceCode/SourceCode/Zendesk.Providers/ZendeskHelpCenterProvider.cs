using System;
using UnityEngine;
using Zendesk.APIs;
using Zendesk.Common;
using Zendesk.Internal.Models.Common;
using Zendesk.Internal.Models.Core;
using Zendesk.Internal.Models.HelpCenter;
using Zendesk.Repositories;

namespace Zendesk.Providers;

public class ZendeskHelpCenterProvider : MonoBehaviour, IZendeskHelpCenterApiProvider
{
	private IHelpCenterApiProvider provider;

	private ZendeskCore zendeskCore;

	public void DeleteVote(Action<ZendeskResponse<object>> callback, long voteId)
	{
		StartCoroutine(provider.DeleteVote(callback, voteId));
	}

	public void DownvoteArticle(Action<ZendeskResponse<VoteResponse>> callback, long articleId, string requestBody = null)
	{
		StartCoroutine(provider.DownvoteArticle(callback, articleId, requestBody));
	}

	public void GetArticle(Action<ZendeskResponse<ArticleResponse>> callback, long id, string locale = null)
	{
		string paramLocale = GetParamLocale(locale);
		StartCoroutine(provider.GetArticle(callback, paramLocale, id));
	}

	public void GetArticles(Action<ZendeskResponse<ArticlesResponse>> callback, string locale = null, string labelNames = null, string include = null, string sortBy = null, string sortOrder = null, int? page = null, int? perPage = null)
	{
		string paramLocale = GetParamLocale(locale);
		StartCoroutine(provider.GetArticles(callback, paramLocale, labelNames, include, sortBy, sortOrder, page, perPage));
	}

	public void GetArticlesByCategoryId(Action<ZendeskResponse<ArticlesResponse>> callback, long categoryId, string locale = null, string labelNames = null, string include = null, int? perPage = null)
	{
		string paramLocale = GetParamLocale(locale);
		StartCoroutine(provider.GetArticlesByCategoryId(callback, paramLocale, categoryId, labelNames, include, perPage));
	}

	public void GetArticlesBySectionId(Action<ZendeskResponse<ArticlesResponse>> callback, long sectionId, string locale = null, string labelNames = null, string include = null, int? perPage = null)
	{
		string paramLocale = GetParamLocale(locale);
		StartCoroutine(provider.GetArticlesBySectionId(callback, paramLocale, sectionId, labelNames, include, perPage));
	}

	public void GetArticlesByUserId(Action<ZendeskResponse<ArticlesResponse>> callback, long userId, string labelNames = null, string include = null, int? page = null, int? perPage = null, string sortBy = null, string sortOrder = null)
	{
		StartCoroutine(provider.GetArticlesByUserId(callback, userId, labelNames, include, page, perPage, sortBy, sortOrder));
	}

	public void GetArticlesIncrementalByDate(Action<ZendeskResponse<ArticlesResponse>> callback, string startTime)
	{
		StartCoroutine(provider.GetArticlesIncrementalByDate(callback, startTime));
	}

	public void GetCategories(Action<ZendeskResponse<CategoriesResponse>> callback, string locale = null, string sort_by = null, string sort_order = null)
	{
		string paramLocale = GetParamLocale(locale);
		StartCoroutine(provider.GetCategories(callback, paramLocale, sort_by, sort_order));
	}

	public void GetSection(Action<ZendeskResponse<SectionResponse>> callback, long sectionId, string locale = null)
	{
		string paramLocale = GetParamLocale(locale);
		StartCoroutine(provider.GetSection(callback, paramLocale, sectionId));
	}

	public void GetSections(Action<ZendeskResponse<SectionsResponse>> callback, string locale = null, long? categoryId = null, string sortBy = null, string sortOrder = null)
	{
		try
		{
			string paramLocale = GetParamLocale(locale);
			if (!categoryId.HasValue)
			{
				StartCoroutine(provider.GetSections(callback, paramLocale, sortBy, sortOrder));
			}
			else
			{
				StartCoroutine(provider.GetSectionsByCategoryId(callback, paramLocale, categoryId.Value, sortBy, sortOrder));
			}
		}
		catch (Exception ex)
		{
			ZendeskResponse<SectionsResponse> zendeskResponse = new ZendeskResponse<SectionsResponse>();
			ZendeskUtils.CheckErrorAndAssign(null, ex.Message, zendeskResponse);
			callback(zendeskResponse);
		}
	}

	public void UpvoteArticle(Action<ZendeskResponse<VoteResponse>> callback, long articleId, string requestBody = null)
	{
		StartCoroutine(provider.UpvoteArticle(callback, articleId, requestBody));
	}

	public void Initialize(ZendeskAuthHandler zendeskAuthHandler, ZendeskCore zendeskCore, ZendeskSettings zendeskSettings)
	{
		try
		{
			HelpCenterApi helpCenterApi = base.gameObject.AddComponent<HelpCenterApi>();
			helpCenterApi.Init(zendeskAuthHandler, zendeskCore, zendeskSettings);
			this.zendeskCore = zendeskCore;
			provider = new HelpCenterApiProvider(new HelpCenterApiRepository(helpCenterApi));
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	private string GetParamLocale(string locale)
	{
		if (locale == null)
		{
			return zendeskCore.zendeskSettings.HelpCenter.Locale;
		}
		return locale;
	}
}
