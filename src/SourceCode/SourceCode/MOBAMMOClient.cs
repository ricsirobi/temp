using KnowledgeAdventure.Multiplayer.Events;
using UnityEngine;

public class MOBAMMOClient : MMOClient
{
	public delegate void ResponseEvent(MMOExtensionResponseReceivedEventArgs responseArgs);

	public delegate void MessageEvent(MMOMessageReceivedEventArgs messageArgs);

	private const string COMMAND_NAME = "MOBA";

	private bool mIsInitialized;

	private GameObject mObject;

	private MOBAManager mManager;

	private MOBALobbyManager mLobbyManager;

	public event ResponseEvent OnReponseEvent;

	public event MessageEvent OnMessageEvent;

	public static MOBAMMOClient Init(MOBALobbyManager lobbyManager = null, MOBAManager levelManager = null)
	{
		MOBAMMOClient mOBAMMOClient = new GameObject("MOBA_MMOClient").AddComponent<MOBAMMOClient>();
		mOBAMMOClient.mManager = levelManager;
		mOBAMMOClient.mLobbyManager = lobbyManager;
		if (MainStreetMMOClient.pInstance != null && MainStreetMMOClient.pInstance.pState == MMOClientState.DISCONNECTING)
		{
			MainStreetMMOClient.pInstance.Connect();
		}
		else
		{
			MainStreetMMOClient.Init();
		}
		MainStreetMMOClient.AddClient(mOBAMMOClient);
		return mOBAMMOClient;
	}

	public override void Destroy()
	{
		if (mObject != null)
		{
			Object.Destroy(mObject);
		}
		MainStreetMMOClient.RemoveClient(this);
		mObject = null;
		mIsInitialized = false;
	}

	private void OnDisable()
	{
		Destroy();
	}

	private void Update()
	{
		if (!mIsInitialized && MainStreetMMOClient.pIsReady)
		{
			mIsInitialized = true;
			MainStreetMMOClient.pInstance.AddExtensionResponseEventHandler("MOBA", MOBAResponseEventHandler);
			MainStreetMMOClient.pInstance.AddMessageReceivedEventHandler(MOBAMessageEventHandler);
		}
	}

	public override void OnClose()
	{
	}

	public void ResetLoadingState()
	{
	}

	public override void Reset()
	{
		Object.Destroy(mObject);
		mIsInitialized = false;
	}

	public override void Disconnected()
	{
	}

	public override void AddPlayer(MMOAvatar avatar)
	{
		if (mLobbyManager != null)
		{
			mLobbyManager.AddPlayer(avatar);
		}
	}

	public override void RemovePlayer(MMOAvatar avatar)
	{
		if (mLobbyManager != null)
		{
			mLobbyManager.RemovePlayer(avatar);
		}
		if (mManager != null)
		{
			mManager.RemovePlayer(avatar);
		}
	}

	public override void RemoveAll()
	{
	}

	private void MOBAResponseEventHandler(object sender, MMOExtensionResponseReceivedEventArgs args)
	{
		if (this.OnReponseEvent != null)
		{
			this.OnReponseEvent(args);
		}
	}

	private void MOBAMessageEventHandler(object sender, MMOMessageReceivedEventArgs args)
	{
		if (this.OnMessageEvent != null)
		{
			this.OnMessageEvent(args);
		}
	}

	private void RoomVariablesChanged(object sender, MMORoomVariablesChangedEventArgs args)
	{
	}
}
