using System;
using Zendesk.Internal.Models.Common;
using Zendesk.Internal.Models.HelpCenter;

namespace Zendesk.Providers;

public interface IZendeskHelpCenterApiProvider
{
	void DeleteVote(Action<ZendeskResponse<object>> callback, long voteId);

	void DownvoteArticle(Action<ZendeskResponse<VoteResponse>> callback, long articleId, string requestBody = null);

	void GetArticle(Action<ZendeskResponse<ArticleResponse>> callback, long id, string locale = null);

	void GetArticles(Action<ZendeskResponse<ArticlesResponse>> callback, string locale = null, string labelNames = null, string include = null, string sortBy = null, string sortOrder = null, int? page = null, int? perPage = null);

	void GetArticlesByCategoryId(Action<ZendeskResponse<ArticlesResponse>> callback, long categoryId, string locale = null, string labelNames = null, string include = null, int? perPage = null);

	void GetArticlesBySectionId(Action<ZendeskResponse<ArticlesResponse>> callback, long sectionId, string locale = null, string labelNames = null, string include = null, int? perPage = null);

	void GetArticlesByUserId(Action<ZendeskResponse<ArticlesResponse>> callback, long userId, string labelNames = null, string include = null, int? page = null, int? perPage = null, string sortBy = null, string sortOrder = null);

	void GetArticlesIncrementalByDate(Action<ZendeskResponse<ArticlesResponse>> callback, string startTime);

	void GetCategories(Action<ZendeskResponse<CategoriesResponse>> callback, string locale = null, string sort_by = null, string sort_order = null);

	void GetSection(Action<ZendeskResponse<SectionResponse>> callback, long sectionId, string locale = null);

	void GetSections(Action<ZendeskResponse<SectionsResponse>> callback, string locale = null, long? categoryId = null, string sort_by = null, string sort_order = null);

	void UpvoteArticle(Action<ZendeskResponse<VoteResponse>> callback, long articleId, string requestBody = null);
}
