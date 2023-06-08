using System;
using System.Collections.Generic;
using KnowledgeAdventure.Multiplayer.Events;

public class MessageListInstance
{
	private const string MESSAGE_EXTENSION = "che";

	private const string POST_MESSAGE = "SMB";

	private const string REPLY_TO_MESSAGE = "SMR";

	private const string MESSAGE_SUCCESS = "SMA";

	private const string MESSAGE_FAILURE = "SMF";

	private const string MESSAGE_BLOCKED = "BLOCKED";

	private const string MESSAGE_ERROR = "ERROR";

	private const string RESPONSE_USER_CHAT_BANNED = "CB";

	private const string RESPONSE_CHAT_MESSAGE_BLOCKED = "MB";

	private const string RESPONSE_CHAT_NOT_ALLOWED = "NC";

	private const string TOKEN_MESSAGE_API_KEY = "key";

	private const string TOKEN_MESSAGE_API_TOKEN = "tkn";

	private const string TOKEN_MESSAGE_CONTENT = "cnt";

	private const string TOKEN_MESSAGE_LEVEL = "lvl";

	private const string TOKEN_MESSAGE_DISPLAY_ATTRIBUTE = "att";

	private const string TOKEN_MESSAGE_TARGET = "tgt";

	private const string TOKEN_MESSAGE_TARGET_LIST = "tlt";

	private const string TOKEN_MESSAGE_REPLY_TO = "rtm";

	private const string TOKEN_MESSAGE_REPLY_TO_SAME_BOARD = "rtsb";

	public CombinedListMessage[] mCombinedMessages;

	public bool pIsError;

	public MessageList pInstance;

	private bool mHandlersAdded;

	private PostEventHandler mPostEventHandler;

	private bool mMessagesDownloaded;

	public bool pIsReady => mMessagesDownloaded;

	public static MessageListInstance Load(string userID, string filterName, MessageLoadCompleteEvent callbackEvent)
	{
		MessageListInstance messageListInstance = new MessageListInstance();
		messageListInstance.Init(userID, filterName, callbackEvent);
		return messageListInstance;
	}

	public void Unload()
	{
		if (MainStreetMMOClient.pInstance != null)
		{
			MainStreetMMOClient.pInstance.RemoveExtensionResponseEventHandler("SMA", SendMessageSuccess);
			MainStreetMMOClient.pInstance.RemoveExtensionResponseEventHandler("SMF", SendMessageFailure);
		}
	}

	private void Init(string userID, string filterName, MessageLoadCompleteEvent callbackEvent)
	{
		pInstance = null;
		mMessagesDownloaded = false;
		WsWebService.GetCombinedListMessage(userID, filterName, EventHandler, callbackEvent);
		AddHandlers();
	}

	private void AddHandlers()
	{
		if (!mHandlersAdded && MainStreetMMOClient.pInstance != null)
		{
			mHandlersAdded = true;
			MainStreetMMOClient.pInstance.AddExtensionResponseEventHandler("SMA", SendMessageSuccess);
			MainStreetMMOClient.pInstance.AddExtensionResponseEventHandler("SMF", SendMessageFailure);
		}
	}

	public void PostMessage(string userID, string message, MessageLevel level, string attribute, PostEventHandler inCallback)
	{
		mPostEventHandler = inCallback;
		AddHandlers();
		if (MainStreetMMOClient.pInstance != null && MainStreetMMOClient.pInstance.pState == MMOClientState.IN_ROOM)
		{
			if (!MessageSilenced())
			{
				Dictionary<string, object> dictionary = new Dictionary<string, object>();
				dictionary.Add("key", ProductConfig.pApiKey);
				dictionary.Add("tkn", ProductConfig.pToken);
				if (userID != UserInfo.pInstance.UserID)
				{
					dictionary.Add("tgt", userID);
				}
				else
				{
					dictionary.Add("tgt", "");
				}
				dictionary.Add("cnt", message);
				dictionary.Add("lvl", (int)level);
				dictionary.Add("att", attribute);
				MainStreetMMOClient.pInstance.SendExtensionMessage("che", "SMB", dictionary);
			}
		}
		else
		{
			MessageBoardPostResult messageBoardPostResult = new MessageBoardPostResult();
			messageBoardPostResult.ErrorID = 2;
			if (mPostEventHandler != null)
			{
				mPostEventHandler(messageBoardPostResult);
			}
		}
	}

	public void ReplyToMessage(string userID, int messageID, string message, MessageLevel level, string attribute, PostEventHandler inCallback)
	{
		mPostEventHandler = inCallback;
		AddHandlers();
		if (MainStreetMMOClient.pInstance != null && MainStreetMMOClient.pInstance.pState == MMOClientState.IN_ROOM)
		{
			if (!MessageSilenced())
			{
				Dictionary<string, object> dictionary = new Dictionary<string, object>();
				dictionary.Add("key", ProductConfig.pApiKey);
				dictionary.Add("tkn", ProductConfig.pToken);
				if (userID != UserInfo.pInstance.UserID)
				{
					dictionary.Add("tgt", userID);
				}
				else
				{
					dictionary.Add("tgt", "");
				}
				dictionary.Add("cnt", message);
				dictionary.Add("lvl", (int)level);
				dictionary.Add("att", attribute);
				dictionary.Add("rtm", messageID);
				dictionary.Add("rtsb", true);
				MainStreetMMOClient.pInstance.SendExtensionMessage("che", "SMR", dictionary);
			}
		}
		else
		{
			MessageBoardPostResult messageBoardPostResult = new MessageBoardPostResult();
			messageBoardPostResult.ErrorID = 2;
			if (mPostEventHandler != null)
			{
				mPostEventHandler(messageBoardPostResult);
			}
		}
	}

	private bool MessageSilenced()
	{
		if (MainStreetMMOClient.pInstance != null && MainStreetMMOClient.pInstance.pIsSilenced)
		{
			if (mPostEventHandler != null)
			{
				mPostEventHandler(null);
			}
			MainStreetMMOClient.pInstance.DisplaySilenceMessage();
			return true;
		}
		return false;
	}

	public void DeleteMessage(int messageID, DeleteEventHandler inCallback)
	{
		WsWebService.RemoveMessageFromBoard(messageID, EventHandler, inCallback);
	}

	private void EventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inType)
		{
		case WsServiceType.GET_COMBINED_LIST_MESSAGE:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
				mMessagesDownloaded = true;
				mCombinedMessages = (CombinedListMessage[])inObject;
				((MessageLoadCompleteEvent)inUserData)?.Invoke();
				break;
			case WsServiceEvent.ERROR:
				mMessagesDownloaded = true;
				pIsError = true;
				UtDebug.LogError("Error while recieving Combined Messages");
				break;
			}
			break;
		case WsServiceType.GET_MESSAGE_BOARD:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
				pInstance = (MessageList)inObject;
				if (pInstance == null)
				{
					pInstance = new MessageList();
					pInstance.Messages = new Message[0];
				}
				break;
			case WsServiceEvent.ERROR:
				pIsError = true;
				break;
			}
			break;
		case WsServiceType.REMOVE_MESSAGE_FROM_BOARD:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
				((DeleteEventHandler)inUserData)(success: true);
				break;
			case WsServiceEvent.ERROR:
				((DeleteEventHandler)inUserData)(success: false);
				break;
			}
			break;
		}
	}

	public void SendMessageSuccess(object sender, MMOExtensionResponseReceivedEventArgs args)
	{
		MessageBoardPostResult messageBoardPostResult = new MessageBoardPostResult();
		messageBoardPostResult.MessageID = Convert.ToInt32(args.ResponseDataObject["3"].ToString());
		messageBoardPostResult.CreateTime = Convert.ToDateTime(args.ResponseDataObject["4"].ToString());
		if (args.ResponseDataObject.ContainsKey("5"))
		{
			messageBoardPostResult.UpdateDate = Convert.ToDateTime(args.ResponseDataObject["5"].ToString());
		}
		if (mPostEventHandler != null)
		{
			mPostEventHandler(messageBoardPostResult);
		}
	}

	public void SendMessageFailure(object sender, MMOExtensionResponseReceivedEventArgs args)
	{
		MessageBoardPostResult messageBoardPostResult = new MessageBoardPostResult();
		string text = args.ResponseDataObject["2"].ToString();
		if (text == "CB")
		{
			messageBoardPostResult = null;
		}
		else if (text == "BLOCKED")
		{
			messageBoardPostResult.ErrorID = 3;
		}
		else
		{
			messageBoardPostResult.ErrorID = 2;
		}
		if (mPostEventHandler != null)
		{
			mPostEventHandler(messageBoardPostResult);
		}
	}
}
