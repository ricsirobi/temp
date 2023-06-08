using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Zendesk.APIs;
using Zendesk.Common;
using Zendesk.Internal.Models.Common;
using Zendesk.Internal.Models.Core;
using Zendesk.Internal.Models.Support;
using Zendesk.Repositories;
using Zendesk.UI;

namespace Zendesk.Providers;

public class ZendeskSupportProvider : MonoBehaviour, IZendeskRequestApiProvider
{
	private static readonly string ALL_REQUEST_STATUSES = "new,open,pending,hold,solved,closed";

	private static readonly string ALL_REQUEST_STATUSES_EXCEPT_FROM_CLOSED = "new,open,pending,hold,solved";

	private static readonly string GET_REQUESTS_SIDE_LOAD = "public_updated_at,last_commenting_agents,last_comment,first_comment";

	private readonly List<string> attachmentTokens = new List<string>();

	private Action<ZendeskResponse<Comment>> addCommentRequestResponse;

	private int attachmentCount;

	private int attachmentCounter;

	private string commentBody;

	private Action<ZendeskResponse<RequestResponse>> createRequestCallback;

	private CreateRequestWrapper createRequestWrapper;

	private IRequestApiProvider provider;

	private string requestId;

	private Action<ZendeskResponse<RequestResponse>> requestResponseCallback;

	private ZendeskCore zendeskCore;

	private ZendeskSettings zendeskSettings;

	private ZendeskLocalizationHandler zendeskLocalizationHandler;

	private ZendeskAuthHandler zendeskAuthorizationHandler;

	public void AddComment(Action<ZendeskResponse<Comment>> callback, string id, string value, List<string> attachments)
	{
		try
		{
			ClearGlobalVarsForReq();
			addCommentRequestResponse = callback;
			StartCoroutine(UploadMultipleAttachmentsForExistingReq(AddCommentInternalCallback, id, value, attachments));
		}
		catch
		{
			ZendeskResponse<Comment> obj = new ZendeskResponse<Comment>();
			ZendeskUtils.CheckErrorAndAssign(null, "", new ZendeskResponse<Comment>());
			callback(obj);
		}
	}

	public void GetAllRequests(Action<ZendeskResponse<RequestsResponse>> callback, string status)
	{
		try
		{
			status = ((!zendeskSettings.Support.ShowClosedRequests) ? (string.IsNullOrEmpty(status) ? ALL_REQUEST_STATUSES_EXCEPT_FROM_CLOSED : status) : (string.IsNullOrEmpty(status) ? ALL_REQUEST_STATUSES : status));
			if (zendeskSettings.Core.Authentication.Equals(ZendeskAuthType.Anonymous.Value))
			{
				List<string> supportRequestIds = ZendeskLocalStorage.GetSupportRequestIds();
				List<string> list = new List<string>();
				foreach (string item in supportRequestIds)
				{
					list.Add(item);
				}
				if (list.Count != 0)
				{
					StartCoroutine(provider.GetManyRequests(callback, string.Join(",", list.ToArray()), status, GET_REQUESTS_SIDE_LOAD, zendeskCore.ClientId, zendeskCore.zendeskUserAgent));
					return;
				}
				callback(new ZendeskResponse<RequestsResponse>
				{
					Result = new RequestsResponse()
				});
			}
			else
			{
				StartCoroutine(provider.GetAllRequests(callback, status, GET_REQUESTS_SIDE_LOAD, zendeskCore.ClientId, zendeskCore.zendeskUserAgent));
			}
		}
		catch (Exception ex)
		{
			ZendeskResponse<RequestsResponse> obj = new ZendeskResponse<RequestsResponse>();
			ZendeskUtils.CheckErrorAndAssign(null, ex.Message, new ZendeskResponse<RequestsResponse>());
			callback(obj);
		}
	}

	public void CreateRequest(Action<ZendeskResponse<RequestResponse>> callback, CreateRequestWrapper wrapper, List<string> attachments)
	{
		try
		{
			ClearGlobalVarsForReq();
			createRequestCallback = callback;
			createRequestWrapper = wrapper;
			if (zendeskSettings.Support.ContactUs.Tags != null && wrapper.Request != null)
			{
				if (wrapper.Request.Tags == null)
				{
					wrapper.Request.Tags = new List<string>();
				}
				wrapper.Request.Tags.AddRange(zendeskSettings.Support.ContactUs.Tags);
			}
			StartCoroutine(UploadMultipleAttachmentsForNewReq(CreateRequestInternalCallback, attachments));
		}
		catch (Exception ex)
		{
			ZendeskResponse<RequestResponse> zendeskResponse = new ZendeskResponse<RequestResponse>();
			ZendeskUtils.CheckErrorAndAssign(null, ex.Message, zendeskResponse);
			callback(zendeskResponse);
		}
	}

	public void GetComments(Action<ZendeskResponse<CommentsResponse>> callback, string id, DateTime? since = null)
	{
		try
		{
			StartCoroutine(provider.GetComments(callback, id, since, zendeskCore.zendeskUserAgent));
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	public void GetRequest(Action<ZendeskResponse<RequestResponse>> callback, string id)
	{
		try
		{
			if (zendeskSettings.Support.Conversations.Enabled)
			{
				StartCoroutine(provider.GetRequest(callback, id, zendeskCore.zendeskUserAgent, GET_REQUESTS_SIDE_LOAD));
				return;
			}
			ZendeskResponse<RequestResponse> zendeskResponse = new ZendeskResponse<RequestResponse>();
			ZendeskUtils.CheckErrorAndAssign(null, zendeskLocalizationHandler.translationGameObjects["usdk_conversations_are_not_enabled"], zendeskResponse);
			callback(zendeskResponse);
		}
		catch (Exception ex)
		{
			ZendeskResponse<RequestResponse> zendeskResponse2 = new ZendeskResponse<RequestResponse>();
			ZendeskUtils.CheckErrorAndAssign(null, ex.Message, zendeskResponse2);
			callback(zendeskResponse2);
		}
	}

	public void GetImage(Action<ZendeskResponse<Texture2D>> callback, Attachment attachment, bool useAuthentication = true)
	{
		try
		{
			if (attachment != null && attachment.ContentUrl != null)
			{
				StartCoroutine(provider.GetImage(callback, attachment, zendeskCore.ClientId, zendeskCore.zendeskUserAgent, useAuthentication));
				return;
			}
			ZendeskResponse<Texture2D> zendeskResponse = new ZendeskResponse<Texture2D>();
			ZendeskUtils.CheckErrorAndAssign(null, zendeskLocalizationHandler.translationGameObjects["usdk_fail_fetch_attachment"], zendeskResponse);
			callback(zendeskResponse);
		}
		catch
		{
			ZendeskResponse<Texture2D> zendeskResponse2 = new ZendeskResponse<Texture2D>();
			ZendeskUtils.CheckErrorAndAssign(null, zendeskLocalizationHandler.translationGameObjects["usdk_fail_fetch_attachment"], zendeskResponse2);
			callback(zendeskResponse2);
		}
	}

	public void CustomerSatisfactionRating(Action<ZendeskResponse<CustomerSatisfactionRating>> callback, string requestId, CustomerSatisfactionRatingScore score, string comment)
	{
		try
		{
			ClearGlobalVarsForReq();
			CustomerSatisfactionRatingWrapper wrapper = new CustomerSatisfactionRatingWrapper
			{
				CustomerSatisfactionRating = new CustomerSatisfactionRating
				{
					Score = score.ToString().ToLower(),
					Comment = comment
				}
			};
			StartCoroutine(provider.CustomerSatisfactionRating(callback, requestId, wrapper, zendeskCore.ClientId, zendeskCore.zendeskUserAgent));
		}
		catch (Exception ex)
		{
			ZendeskResponse<CustomerSatisfactionRating> obj = new ZendeskResponse<CustomerSatisfactionRating>();
			ZendeskUtils.CheckErrorAndAssign(null, ex.Message, new ZendeskResponse<CustomerSatisfactionRating>());
			callback(obj);
		}
	}

	public void Initialize(ZendeskAuthHandler zendeskAuthHandler, ZendeskCore zendeskCore, ZendeskSettings zendeskSettings, ZendeskLocalizationHandler zendeskLocalizationHandler)
	{
		try
		{
			RequestApi requestApi = null;
			if (base.gameObject.GetComponent<RequestApi>() == null)
			{
				requestApi = base.gameObject.AddComponent<RequestApi>();
			}
			this.zendeskCore = zendeskCore;
			this.zendeskLocalizationHandler = zendeskLocalizationHandler;
			this.zendeskSettings = zendeskSettings;
			zendeskAuthorizationHandler = zendeskAuthHandler;
			requestApi.Init(zendeskAuthHandler, zendeskCore, zendeskSettings);
			provider = new RequestApiProvider(new RequestApiRepository(requestApi));
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	private void CreateRequestInternalCallback(ZendeskResponse<RequestResponse> requestResponse)
	{
		try
		{
			if (!requestResponse.IsError)
			{
				ZendeskLocalStorage.SaveSupportRequest(requestResponse.Result.Request, isRead: true);
			}
			createRequestCallback(requestResponse);
		}
		catch (Exception ex)
		{
			ZendeskResponse<RequestResponse> zendeskResponse = new ZendeskResponse<RequestResponse>();
			ZendeskUtils.CheckErrorAndAssign(null, ex.Message, zendeskResponse);
			createRequestCallback(zendeskResponse);
		}
	}

	private void AddCommentInternalCallback(ZendeskResponse<RequestResponse> requestResponse)
	{
		ZendeskResponse<Comment> zendeskResponse = new ZendeskResponse<Comment>();
		try
		{
			if (requestResponse.IsError)
			{
				zendeskResponse.IsError = true;
				zendeskResponse.ErrorResponse = requestResponse.ErrorResponse;
			}
			else if (requestResponse.Result != null && requestResponse.Result.Request != null)
			{
				if (requestResponse.Result.Request.CommentCount != 0)
				{
					ZendeskLocalStorage.SaveSupportRequest(requestResponse.Result.Request, isRead: true);
				}
				zendeskResponse.Result = requestResponse.Result.Request.LastComment;
			}
		}
		catch (Exception ex)
		{
			ZendeskUtils.CheckErrorAndAssign(null, ex.Message, zendeskResponse);
		}
		finally
		{
			addCommentRequestResponse(zendeskResponse);
		}
	}

	private void CreateAttachmentList(ZendeskResponse<UploadResponse> response)
	{
		try
		{
			if (!response.IsError)
			{
				UploadResponse result = response.Result;
				attachmentTokens.Add(result.Upload.Token);
				attachmentCounter++;
				if (attachmentCount.Equals(attachmentCounter))
				{
					UpdateRequestWrapper updateRequestWrapper = new UpdateRequestWrapper();
					updateRequestWrapper.Request = new UpdateRequest();
					EndUserComment endUserComment = new EndUserComment();
					endUserComment.Value = commentBody;
					endUserComment.Uploads = attachmentTokens;
					updateRequestWrapper.Request.Comment = endUserComment;
					StartCoroutine(provider.AddComment(requestResponseCallback, requestId, updateRequestWrapper, zendeskCore.ClientId, zendeskCore.zendeskUserAgent));
				}
				return;
			}
			throw new Exception(response.ErrorResponse.Reason);
		}
		catch (Exception ex)
		{
			ZendeskResponse<RequestResponse> zendeskResponse = new ZendeskResponse<RequestResponse>();
			ZendeskUtils.CheckErrorAndAssign(null, ex.Message, zendeskResponse);
			if (requestResponseCallback != null)
			{
				requestResponseCallback(zendeskResponse);
			}
		}
	}

	private IEnumerator UploadMultipleAttachmentsForExistingReq(Action<ZendeskResponse<RequestResponse>> callback, string id, string value, List<string> attachmentPaths)
	{
		try
		{
			requestResponseCallback = callback;
			requestId = id;
			commentBody = value;
			if (attachmentPaths != null && attachmentPaths.Count > 0)
			{
				attachmentCount = 0;
				attachmentCounter = 0;
				attachmentCount = attachmentPaths.Count;
				foreach (string attachmentPath in attachmentPaths)
				{
					byte[] attachment = File.ReadAllBytes(attachmentPath);
					StartCoroutine(Upload(CreateAttachmentList, Path.GetFileName(attachmentPath), attachment, zendeskCore.ClientId, zendeskCore.zendeskUserAgent));
				}
			}
			else
			{
				UpdateRequestWrapper updateRequestWrapper = new UpdateRequestWrapper();
				updateRequestWrapper.Request = new UpdateRequest();
				EndUserComment endUserComment = new EndUserComment();
				endUserComment.Value = commentBody;
				endUserComment.Uploads = attachmentTokens;
				updateRequestWrapper.Request.Comment = endUserComment;
				StartCoroutine(provider.AddComment(requestResponseCallback, requestId, updateRequestWrapper, zendeskCore.ClientId, zendeskCore.zendeskUserAgent));
			}
		}
		catch (Exception ex)
		{
			ZendeskResponse<RequestResponse> zendeskResponse = new ZendeskResponse<RequestResponse>();
			ZendeskUtils.CheckErrorAndAssign(null, ex.Message, zendeskResponse);
			callback(zendeskResponse);
		}
		yield return null;
	}

	private IEnumerator UploadMultipleAttachmentsForNewReq(Action<ZendeskResponse<RequestResponse>> callback, List<string> attachmentPaths)
	{
		try
		{
			if (attachmentPaths != null && attachmentPaths.Count > 0)
			{
				attachmentCount = attachmentPaths.Count;
				requestResponseCallback = callback;
				attachmentCounter = 0;
				foreach (string attachmentPath in attachmentPaths)
				{
					byte[] attachment = File.ReadAllBytes(attachmentPath);
					StartCoroutine(Upload(AddAttachmentToRequest, Path.GetFileName(attachmentPath), attachment, zendeskCore.ClientId, zendeskCore.zendeskUserAgent));
				}
			}
			else
			{
				StartCoroutine(provider.CreateRequest(callback, createRequestWrapper, zendeskCore.ClientId, zendeskCore.zendeskUserAgent));
				createRequestWrapper = null;
			}
		}
		catch (Exception ex)
		{
			ZendeskResponse<RequestResponse> zendeskResponse = new ZendeskResponse<RequestResponse>();
			ZendeskUtils.CheckErrorAndAssign(null, ex.Message, zendeskResponse);
			callback(zendeskResponse);
		}
		yield return null;
	}

	private void AddAttachmentToRequest(ZendeskResponse<UploadResponse> result)
	{
		try
		{
			if (!result.IsError)
			{
				UploadResponse result2 = result.Result;
				if (createRequestWrapper.Request.Comment == null)
				{
					createRequestWrapper.Request.Comment = new Comment();
					createRequestWrapper.Request.Comment.Uploads = new List<string>();
				}
				else if (createRequestWrapper.Request.Comment.Uploads == null)
				{
					createRequestWrapper.Request.Comment.Uploads = new List<string>();
				}
				createRequestWrapper.Request.Comment.Uploads.Add(result2.Upload.Token);
				attachmentCounter++;
				if (attachmentCount.Equals(attachmentCounter))
				{
					StartCoroutine(provider.CreateRequest(requestResponseCallback, createRequestWrapper, zendeskCore.ClientId, zendeskCore.zendeskUserAgent));
				}
			}
			else
			{
				ZendeskResponse<RequestResponse> obj = new ZendeskResponse<RequestResponse>
				{
					IsError = true,
					ErrorResponse = result.ErrorResponse
				};
				requestResponseCallback(obj);
			}
		}
		catch (Exception ex)
		{
			ZendeskResponse<RequestResponse> zendeskResponse = new ZendeskResponse<RequestResponse>();
			ZendeskUtils.CheckErrorAndAssign(null, ex.Message, zendeskResponse);
			if (requestResponseCallback != null)
			{
				requestResponseCallback(zendeskResponse);
			}
		}
	}

	private void ClearGlobalVarsForReq()
	{
		requestResponseCallback = null;
		attachmentCount = 0;
		attachmentCounter = 0;
		createRequestWrapper = null;
		addCommentRequestResponse = null;
		requestId = null;
		commentBody = null;
		attachmentTokens.Clear();
	}

	private IEnumerator Upload(Action<ZendeskResponse<UploadResponse>> callback, string filename, object attachment, string clientIdentifier, string userAgent)
	{
		try
		{
			Attachment attachment2 = new Attachment();
			if (attachment is Attachment)
			{
				attachment2 = (Attachment)attachment;
			}
			else
			{
				byte[] array = (byte[])attachment;
				attachment2.FileName = filename;
				attachment2.Content = attachment;
				attachment2.Size = array.Length;
			}
			if (ValidateFileSize(attachment2))
			{
				StartCoroutine(provider.Upload(callback, filename, attachment2, clientIdentifier, userAgent));
			}
			else
			{
				ZendeskResponse<UploadResponse> zendeskResponse = new ZendeskResponse<UploadResponse>();
				ZendeskUtils.CheckErrorAndAssign(null, zendeskLocalizationHandler.translationGameObjects["usdk_file_exceeds_max_size_20_mb"], zendeskResponse);
				callback(zendeskResponse);
			}
		}
		catch (Exception ex)
		{
			ZendeskResponse<UploadResponse> zendeskResponse2 = new ZendeskResponse<UploadResponse>();
			ZendeskUtils.CheckErrorAndAssign(null, ex.Message, zendeskResponse2);
			callback(zendeskResponse2);
		}
		yield return null;
	}

	public void Upload(Action<ZendeskResponse<UploadResponse>> callback, string filename, string filePath)
	{
		try
		{
			Attachment attachment = new Attachment();
			byte[] content = File.ReadAllBytes(filePath);
			FileStream fileStream = File.Open(filePath, FileMode.Open);
			attachment.FileName = Path.GetFileName(filePath);
			attachment.Content = content;
			attachment.Size = fileStream.Length;
			if (ValidateFileSize(attachment))
			{
				StartCoroutine(provider.Upload(callback, filename, attachment, zendeskCore.ClientId, zendeskCore.zendeskUserAgent));
				return;
			}
			ZendeskResponse<UploadResponse> zendeskResponse = new ZendeskResponse<UploadResponse>();
			ZendeskUtils.CheckErrorAndAssign(null, zendeskLocalizationHandler.translationGameObjects["usdk_file_exceeds_max_size_20_mb"], zendeskResponse);
			callback(zendeskResponse);
		}
		catch (Exception ex)
		{
			ZendeskResponse<UploadResponse> zendeskResponse2 = new ZendeskResponse<UploadResponse>();
			ZendeskUtils.CheckErrorAndAssign(null, ex.Message, zendeskResponse2);
			callback(zendeskResponse2);
		}
	}

	private bool ValidateFileSize(Attachment attachment)
	{
		if (attachment.Size > zendeskCore.zendeskSettings.Support.Attachments.MaxAttachmentSize)
		{
			return false;
		}
		return true;
	}

	public void GetRequestUpdates(Action<ZendeskResponse<ZendeskRequestUpdates>> callback, List<string> requestIds = null)
	{
		ZendeskResponse<ZendeskRequestUpdates> zendeskResponse2 = null;
		if (zendeskAuthorizationHandler.IsAuthenticated)
		{
			if (ZendeskLocalStorage.IsRequestUpdateCacheValid())
			{
				ZendeskRequestUpdates zendeskRequestUpdates = new ZendeskRequestUpdates();
				zendeskRequestUpdates.ParseLocalStorage(ZendeskLocalStorage.GetSupportRequests());
				zendeskResponse2 = new ZendeskResponse<ZendeskRequestUpdates>
				{
					IsError = false,
					Result = zendeskRequestUpdates
				};
				callback(zendeskResponse2);
			}
			else
			{
				GetAllRequests(delegate(ZendeskResponse<RequestsResponse> zendeskResponse)
				{
					GetAllRequestCallback(zendeskResponse, callback);
				}, "");
			}
		}
		else
		{
			zendeskResponse2 = new ZendeskResponse<ZendeskRequestUpdates>
			{
				IsError = true,
				ErrorResponse = new ErrorResponse(isAuthError: true, "End user not authenticated"),
				Result = null
			};
			callback(zendeskResponse2);
		}
	}

	private void GetAllRequestCallback(ZendeskResponse<RequestsResponse> getAllRequestsResponse, Action<ZendeskResponse<ZendeskRequestUpdates>> callback)
	{
		ZendeskResponse<ZendeskRequestUpdates> zendeskResponse = null;
		ZendeskRequestUpdates zendeskRequestUpdates = new ZendeskRequestUpdates();
		ZendeskLocalStorage.SaveSupportRequests(getAllRequestsResponse.Result.Requests);
		zendeskRequestUpdates.ParseLocalStorage(ZendeskLocalStorage.GetSupportRequests());
		ZendeskLocalStorage.UpdateLastTimeRetrivedForRequestUpdates();
		zendeskResponse = new ZendeskResponse<ZendeskRequestUpdates>
		{
			IsError = false,
			Result = zendeskRequestUpdates
		};
		callback(zendeskResponse);
	}

	public ZendeskResponse<bool> MarkRequestAsRead(List<string> requestIds = null)
	{
		bool result = false;
		ZendeskResponse<bool> zendeskResponse = new ZendeskResponse<bool>();
		List<SupportRequestStorageItem> supportRequests = ZendeskLocalStorage.GetSupportRequests();
		int num = supportRequests.Count;
		if (requestIds != null && requestIds.Count > 0)
		{
			num = requestIds.Count;
		}
		if (supportRequests.Count > 0)
		{
			foreach (SupportRequestStorageItem item in supportRequests)
			{
				if (requestIds == null || requestIds.Contains(item.id))
				{
					item.isRead = true;
					item.newComments = 0;
					ZendeskLocalStorage.SaveSupportRequest(item);
					num--;
				}
			}
		}
		if (num == 0)
		{
			result = true;
		}
		zendeskResponse.Result = result;
		return zendeskResponse;
	}
}
