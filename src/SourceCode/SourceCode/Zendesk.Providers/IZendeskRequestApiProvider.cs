using System;
using System.Collections.Generic;
using UnityEngine;
using Zendesk.Internal.Models.Common;
using Zendesk.Internal.Models.Support;

namespace Zendesk.Providers;

public interface IZendeskRequestApiProvider
{
	void AddComment(Action<ZendeskResponse<Comment>> callback, string id, string value, List<string> attachments);

	void CreateRequest(Action<ZendeskResponse<RequestResponse>> callback, CreateRequestWrapper wrapper, List<string> attachments);

	void GetAllRequests(Action<ZendeskResponse<RequestsResponse>> callback, string status);

	void GetComments(Action<ZendeskResponse<CommentsResponse>> callback, string id, DateTime? since);

	void GetRequest(Action<ZendeskResponse<RequestResponse>> callback, string id);

	void GetImage(Action<ZendeskResponse<Texture2D>> callback, Attachment attachment, bool useAuthentication = true);

	void Upload(Action<ZendeskResponse<UploadResponse>> callback, string filename, string filePath);

	void CustomerSatisfactionRating(Action<ZendeskResponse<CustomerSatisfactionRating>> callback, string requestId, CustomerSatisfactionRatingScore score, string comment);

	void GetRequestUpdates(Action<ZendeskResponse<ZendeskRequestUpdates>> callback, List<string> requestIds = null);

	ZendeskResponse<bool> MarkRequestAsRead(List<string> requestIds = null);
}
